using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebHdfs
{

    /// <summary>
    /// Минимально необходимый клиент для Web HDFS
    /// </summary>
    public class WebHdfsClient
    {
        public string BaseUrl { get; private set; }
        public string HomeDirectory { get; private set; }
        public string User { get; private set; }

        internal const string PREFIX = "webhdfs/v1";

        /// <summary>
        /// Underlying <see cref="System.Net.Http.HttpMessageHandler"/> that will process web requests (for testing purpose mostly).
        /// </summary>
        public HttpMessageHandler InnerHandler
        { get; set; }

        private string GetAbsolutePath(string hdfsPath)
        {
            if (string.IsNullOrEmpty(hdfsPath))
            {
                return "/";
            }
            else if (hdfsPath[0] == '/')
            {
                return hdfsPath;
            }
            else if (hdfsPath.Contains(":"))
            {
                Uri uri = new Uri(hdfsPath);
                return uri.AbsolutePath;
            }
            else
            {
                return HomeDirectory + "/" + hdfsPath;
            }
        }

        private string GetFullyQualifiedPath(string path)
        {
            if (path.Contains(":"))
            {
                return path;
            }

            path = GetAbsolutePath(path);
            return "hdfs://" + BaseUrl + path;
        }

        public WebHdfsClient(string baseUrl, string user = null)
            :this(new HttpClientHandler(), baseUrl, user)
        {
        }

        public WebHdfsClient(HttpMessageHandler handler, string baseUrl, string user = null) 
        {
            InnerHandler = handler;
            BaseUrl = baseUrl;
            User = user;
            GetHomeDirectory().Wait();
        }

        #region "read"

        /// <summary>
        /// List the statuses of the files/directories in the given path if the path is a directory. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<DirectoryListing> GetDirectoryStatus(string path)
        {
            return CallWebHDFS<DirectoryListing>(path, "LISTSTATUS", HttpMethod.Get);
        }

        /// <summary>
        /// Return a file status object that represents the path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<DirectoryEntry> GetFileStatus(string path)
        {
            return CallWebHDFS<DirectoryEntry>(path, "GETFILESTATUS", HttpMethod.Get);
        }

        /// <summary>
        /// Return the current user's home directory in this filesystem. 
        /// The default implementation returns "/user/$USER/". 
        /// </summary>
        /// <returns></returns>
        public async Task GetHomeDirectory()
        {
            if (string.IsNullOrEmpty(HomeDirectory))
            {
                string uri = GetUriForOperation("/") + "op=" + "GETHOMEDIRECTORY";
                var response = await GetResponseMessageAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    JObject path = await response.Content.ReadAsAsync<JObject>();
                    HomeDirectory = path.Value<string>("Path");
                }
            }
        }

        /// <summary>
        /// Return the ContentSummary of a given Path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<ContentSummary> GetContentSummary(string path)
        {
            return CallWebHDFS<ContentSummary>(path, "GETCONTENTSUMMARY", HttpMethod.Get);
        }

        /// <summary>
        /// Get the checksum of a file
        /// </summary>
        /// <param name="path">The file checksum. The default return value is null, which 
        /// indicates that no checksum algorithm is implemented in the corresponding FileSystem. </param>
        /// <returns></returns>
        public Task<FileChecksum> GetFileChecksum(string path)
        {
            return CallWebHDFS<FileChecksum>(path, "GETFILECHECKSUM", HttpMethod.Get);
        }

        // todo, overloads with offset & length
        /// <summary>
        /// Opens an FSDataInputStream at the indicated Path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Stream> OpenFile(string path, CancellationToken token)
        {
            return await OpenFile(path, -1, -1, token);
        }

        /// <summary>
        /// Opens an FSDataInputStream at the indicated Path.  The offset and length will allow 
        /// you to get a subset of the file.  
        /// </summary>
        /// <param name="path"></param>
        /// <param name="offset">This includes any header bytes</param>
        /// <param name="length"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Stream> OpenFile(string path, int offset, int length, CancellationToken token)
        {
            string uri = GetUriForOperation(path) + "op=OPEN";
            if (offset > 0)
                uri += "&offset=" + offset.ToString();
            if (length > 0)
                uri += "&length=" + length.ToString();
            var client = new HttpClient(InnerHandler ?? new HttpClientHandler(), InnerHandler == null);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            return await response.Content.ReadAsStreamAsync();
        }

        #endregion

        #region "put"
        // todo: add permissions
        /// <summary>
        /// Make the given file and all non-existent parents into directories. 
        /// Has the semantics of Unix 'mkdir -p'. Existence of the directory hierarchy is not an error. 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<bool> CreateDirectory(string path)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "MKDIRS", HttpMethod.Put);
            return result.Value;

        }

        /// <summary>
        /// Renames Path src to Path dst.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public async Task<bool> RenameDirectory(string path, string newPath)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "RENAME&destination=" + newPath, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Delete a file.  Note, this will not recursively delete and will
        /// not delete if directory is not empty
        /// </summary>
        /// <param name="path">the path to delete</param>
        /// <returns>true if delete is successful else false. </returns>
        public Task<bool> DeleteDirectory(string path)
        {
            return DeleteDirectory(path, false);
        }


        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="path">the path to delete</param>
        /// <param name="recursive">if path is a directory and set to true, the directory is deleted else throws an exception.
        /// In case of a file the recursive can be set to either true or false. </param>
        /// <returns>true if delete is successful else false. </returns>
        public async Task<bool> DeleteDirectory(string path, bool recursive)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "DELETE&recursive=" + recursive.ToString().ToLower(), HttpMethod.Delete);
            return result.Value;
        }

        /// <summary>
        /// Set permission of a path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="permissions"></param>
        public async Task<bool> SetPermissions(string path, string permissions)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETPERMISSION&permission=" + permissions, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Sets the owner for the file 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="owner">If it is null, the original username remains unchanged</param>
        public async Task<bool> SetOwner(string path, string owner)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETOWNER&owner=" + owner, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Sets the group for the file 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="group">If it is null, the original groupname remains unchanged</param>
        public async Task<bool> SetGroup(string path, string group)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETOWNER&group=" + group, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Set replication for an existing file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replicationFactor"></param>
        /// <returns></returns>
        public async Task<bool> SetReplicationFactor(string path, int replicationFactor)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETREPLICATION&replication=" + replicationFactor.ToString(), HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Set access time of a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="accessTime">Set the access time of this file. The number of milliseconds since Jan 1, 1970. 
        /// A value of -1 means that this call should not set access time</param>
        public async Task<bool> SetAccessTime(string path, string accessTime)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETTIMES&accesstime=" + accessTime, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Set modification time of a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="modificationTime">Set the modification time of this file. The number of milliseconds since Jan 1, 1970.
        /// A value of -1 means that this call should not set modification time</param>
        public async Task<bool> SetModificationTime(string path, string modificationTime)
        {
            var result = await CallWebHDFS<BooleanResult>(path, "SETTIMES&modificationtime=" + modificationTime, HttpMethod.Put);
            return result.Value;
        }

        /// <summary>
        /// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
        /// </summary>
        /// <param name="localFile"></param>
        /// <param name="remotePath"></param>
        /// <returns></returns>
        public async Task<bool> CreateFile(string localFile, string remotePath, CancellationToken token)
        {
            var sc = new StreamContent(File.OpenRead(localFile));
            var result = await CallWebHDFS<BooleanResult>(remotePath, "CREATE&overwrite=true", sc, new HttpRequestOptions() { Token = token, Method = HttpMethod.Put });
            return result == null || result.Value;
        }

        /// <summary>
        /// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="remotePath"></param>
        /// <returns></returns>
        public async Task<bool> CreateFile(Stream content, string remotePath, CancellationToken token)
        {
            var sc = new StreamContent(content);
            var result = await CallWebHDFS<BooleanResult>(remotePath, "CREATE&overwrite=true", sc, new HttpRequestOptions() { Token = token, Method = HttpMethod.Put });
            return result == null || result.Value;
        }

        ///// <summary>
        ///// Append to an existing file (optional operation).
        ///// </summary>
        ///// <param name="localFile"></param>
        ///// <param name="remotePath"></param>
        ///// <returns></returns>
        //public async Task<string> AppendFile(string localFile, string remotePath)
        //{
        //	var hc = CreateHTTPClient(false);
        //	var resp = await hc.PostAsync(GetUriForOperation(remotePath) + "op=APPEND", null);
        //	if (!resp.IsSuccessStatusCode)
        //		return null;
        //	var postLocation = resp.Headers.Location;
        //	StreamContent sc = new StreamContent(File.OpenRead(localFile));
        //	var resp2 = await hc.PostAsync(postLocation, sc);
        //	if (!resp2.IsSuccessStatusCode)
        //		return null;

        //	// oddly, this is returning a 403 forbidden 
        //	// due to: "IOException","javaClassName":"java.io.IOException","message":"java.io.IOException: 
        //	// Append to hdfs not supported. Please refer to dfs.support.append configuration parameter.
        //	return resp2.Headers.Location.ToString();
        //}

        ///// <summary>
        ///// Append to an existing file (optional operation).
        ///// </summary>
        ///// <param name="localFile"></param>
        ///// <param name="remotePath"></param>
        ///// <returns></returns>
        //public async Task<string> AppendFile(Stream content, string remotePath)
        //{
        //	var hc = CreateHTTPClient(false);
        //	var resp = await hc.PostAsync(GetUriForOperation(remotePath) + "op=APPEND", null);
        //	if (!resp.IsSuccessStatusCode)
        //		return null;

        //	var postLocation = resp.Headers.Location;
        //	var sc = new StreamContent(content);
        //	var resp2 = await hc.PostAsync(postLocation, sc);
        //	if (!resp2.IsSuccessStatusCode)
        //		return null;
        //	return resp2.Headers.Location.ToString();
        //}

        #endregion

        private string GetUriForOperation(string path)
        {
            string uri = BaseUrl + PREFIX;
            if (!string.IsNullOrEmpty(path))
            {
                if (path[0] == '/')
                {
                    uri += path;
                }
                else
                {
                    uri += HomeDirectory + "/" + path;
                }
            }
            uri += "?";

            if (!string.IsNullOrEmpty(User))
                uri += "user.name=" + User + "&";

            return uri;
        }

        private Task<T> CallWebHDFS<T>(string path, string operation, HttpMethod method, HttpContent content = null) where T : IJObject, new()
        {
            return CallWebHDFS<T>(path, operation, content, new HttpRequestOptions() { Method = method });
        }

        private async Task<T> CallWebHDFS<T>(string path, string operation, HttpContent content = null, HttpRequestOptions options = null) where T : IJObject, new()
        {
            string uri = GetUriForOperation(path);
            uri += "op=" + operation;

            var response = await GetResponseMessageAsync(uri, content, options ?? new HttpRequestOptions());

            if (!response.IsSuccessStatusCode)
                OnError(response, null);

            var jobj = await response.Content.ReadAsAsync<JObject>();

            if (jobj == null)
                return default(T);

            var result = new T() as IJObject;
            result.Parse(jobj);
            return (T)result;
        }

        /// <summary>
        /// Send http request and return <see cref="HttpResponseMessage"/> received.
        /// </summary>
        /// <param name="url">Url to be requested.</param>
        /// <param name="data">Data to be sent.</param>
        /// <param name="options">Request options.</param>
        /// <returns>Asynchronous task.</returns>
        protected async Task<HttpResponseMessage> GetResponseMessageAsync(string url, object data = null, HttpRequestOptions options = null)
        {
            if (options == null)
                options = new HttpRequestOptions();

            using (var client = new HttpClient(InnerHandler ?? new HttpClientHandler(), InnerHandler == null))
            {
                client.BaseAddress = new Uri(BaseUrl);

                foreach (var mediaType in options.Formatter.SupportedMediaTypes)
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType.MediaType));
                }

                var request = new HttpRequestMessage(options.Method, url);
                if (options.Method != HttpMethod.Get && data != null)
                {
                    request.Content = data as HttpContent ?? new ObjectContent(data.GetType(), data, options.Formatter, options.Formatter.SupportedMediaTypes.FirstOrDefault());
                }

                try
                {
                    var result = await client.SendAsync(request, options.Completion, options.Token).ConfigureAwait(false);

                    if (!result.IsSuccessStatusCode)
                        OnError(result, null);

                    return result;
                }
                catch (Exception e)
                {
                    OnError(null, e);
                }
            }

            return null;
        }

        /// <summary>
        /// Event, fired up in case if client returns responseCode other than 20*.
        /// </summary>
        public event EventHandler<HttpClientEventArgs> Error = delegate { };

        /// <summary>
        /// Overridable handler for Error event.
        /// </summary>
        /// <param name="response"><see cref="System.Net.Http.HttpResponseMessage"/>, вернувшийся от сервера</param>
        /// <param name="exception"><see cref="System.Exception"/>, выпавший в процессе запроса</param>
        public virtual void OnError(HttpResponseMessage response, Exception exception)
        {
            Error(this, new HttpClientEventArgs(response, exception));
        }
    }

    public interface IJObject
    {
        void Parse(JObject obj);
    }

    public class DirectoryListing : IJObject
    {
        IEnumerable<DirectoryEntry> directoryEntries;

        public void Parse(JObject rootEntry)
        {
            directoryEntries = rootEntry.Value<JObject>("FileStatuses").Value<JArray>("FileStatus").Select(fs =>
            {
                var d = new DirectoryEntry();
                d.Parse(fs.Value<JObject>());
                return d;
            });
        }

        public IEnumerable<DirectoryEntry> Directories
        { get { return directoryEntries.Where(fs => fs.Type == "DIRECTORY"); } }
        public IEnumerable<DirectoryEntry> Files
        { get { return directoryEntries.Where(fs => fs.Type == "FILE"); } }
    }

    public class BooleanResult : IJObject
    {
        public bool Value { get; set; }
        public void Parse(JObject obj)
        {
            Value = obj.Value<bool>("boolean");
        }
    }

    public class DirectoryEntry : IJObject
    {
        // todo - should makt these immutable.
        public string AccessTime
        { get; set; }
        public string BlockSize
        { get; set; }
        public string Group
        { get; set; }
        public Int64 Length
        { get; set; }
        public string ModificationTime
        { get; set; }
        public string Owner
        { get; set; }
        public string PathSuffix
        { get; set; }
        // todo, replace with flag enum 
        public string Permission
        { get; set; }
        public int Replication
        { get; set; }
        // todo, replace with enum 
        public string Type
        { get; set; }

        public void Parse(JObject value)
        {
            var tmp = value.Value<JObject>("FileStatus") ?? value;
            AccessTime = tmp.Value<string>("accessTime");
            BlockSize = tmp.Value<string>("blockSize");
            Group = tmp.Value<string>("group");
            Length = tmp.Value<Int64>("length");
            ModificationTime = tmp.Value<string>("modificationTime");
            Owner = tmp.Value<string>("owner");
            PathSuffix = tmp.Value<string>("pathSuffix");
            Permission = tmp.Value<string>("permission");
            Replication = tmp.Value<int>("replication");
            Type = tmp.Value<string>("type");
        }
    }

    public class ContentSummary : IJObject
    {
        // todo - should makt these immutable.
        public int DirectoryCount
        { get; set; }
        public int FileCount
        { get; set; }
        public long Length
        { get; set; }
        public int Quota
        { get; set; }
        public long SpaceConsumed
        { get; set; }
        public long SpaceQuota
        { get; set; }

        public void Parse(JObject value)
        {
            DirectoryCount = value.Value<int>("directoryCount");
            FileCount = value.Value<int>("fileCount");
            Length = value.Value<long>("length");
            Quota = value.Value<int>("quota");
            SpaceConsumed = value.Value<long>("spaceConsumed");
            SpaceQuota = value.Value<long>("spaceQuota");
        }

    }

    public class FileChecksum : IJObject
    {
        public string Algorithm
        { get; set; }
        public string Checksum
        { get; set; }
        public int Length
        { get; set; }

        public void Parse(JObject value)
        {
            Algorithm = value.Value<string>("algorithm");
            Checksum = value.Value<string>("bytes");
            Length = value.Value<int>("length");
        }
    }
}