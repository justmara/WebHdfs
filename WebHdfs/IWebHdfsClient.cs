using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebHdfs.Entities;

namespace WebHdfs
{
	/// <summary>
	/// Minimalistic WebHdfs client
	/// </summary>
	public interface IWebHdfsClient
	{
		/// <summary>
		/// Base url of WebHdfs service.
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Home directory.
		/// </summary>
		string HomeDirectory { get; }

		/// <summary>
		/// Username to be used with securify off (when only user.name required);
		/// </summary>
		string User { get; }

		/// <summary>
		/// Underlying <see cref="HttpMessageHandler"/> that will process web requests (for testing purpose mostly).
		/// </summary>
		HttpMessageHandler InnerHandler { get; set; }

		/// <summary>
		/// List the statuses of the files/directories in the given path if the path is a directory. 
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <returns></returns>
		Task<DirectoryListing> GetDirectoryStatus(string path);

		/// <summary>
		/// Return a file status object that represents the path.
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <returns></returns>
		Task<DirectoryEntry> GetFileStatus(string path);

		/// <summary>
		/// Return the current user's home directory in this filesystem. 
		/// The default implementation returns "/user/$USER/". 
		/// </summary>
		/// <returns></returns>
		Task GetHomeDirectory();

		/// <summary>
		/// Return the ContentSummary of a given Path
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <returns></returns>
		Task<ContentSummary> GetContentSummary(string path);

		/// <summary>
		/// Get the checksum of a file
		/// </summary>
		/// <param name="path">The file checksum. The default return value is null, which 
		/// indicates that no checksum algorithm is implemented in the corresponding FileSystem. </param>
		/// <returns></returns>
		Task<FileChecksum> GetFileChecksum(string path);

		/// <summary>
		/// Opens an FSDataInputStream at the indicated Path
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <returns>Async <see cref="Task{Stream}"/> with file content.</returns>
		Task<Stream> OpenFile(string path);

		/// <summary>
		/// Opens an FSDataInputStream at the indicated Path
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="token"><see cref="CancellationToken"/> to cancel call if needed.</param>
		/// <returns>Async <see cref="Task{Stream}"/> with file content.</returns>
		Task<Stream> OpenFile(string path, CancellationToken token);

		/// <summary>
		/// Opens an FSDataInputStream at the indicated Path.  The offset and length will allow 
		/// you to get a subset of the file.  
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="offset">The starting byte position. This includes any header bytes</param>
		/// <param name="length">The number of bytes to be processed.</param>
		/// <param name="token"><see cref="CancellationToken"/> to cancel call if needed.</param>
		/// <returns>Async <see cref="Task{Stream}"/> with file content.</returns>
		Task<Stream> OpenFile(string path, int offset, int length, CancellationToken token);

		/// <summary>
		/// Make the given file and all non-existent parents into directories. 
		/// Has the semantics of Unix 'mkdir -p'. Existence of the directory hierarchy is not an error. 
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> CreateDirectory(string path);

		/// <summary>
		/// Renames Path src to Path dst.
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="newPath"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> RenameDirectory(string path, string newPath);

		/// <summary>
		/// Delete a file.  Note, this will not recursively delete and will
		/// not delete if directory is not empty
		/// </summary>
		/// <param name="path">the path to delete</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> DeleteDirectory(string path);

		/// <summary>
		/// Delete a file
		/// </summary>
		/// <param name="path">the path to delete</param>
		/// <param name="recursive">if path is a directory and set to true, the directory is deleted else throws an exception.
		/// In case of a file the recursive can be set to either true or false. </param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> DeleteDirectory(string path, bool recursive);

		/// <summary>
		/// Set permission of a path.
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="permissions"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetPermissions(string path, string permissions);

		/// <summary>
		/// Sets the owner for the file 
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="owner">If it is null, the original username remains unchanged</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetOwner(string path, string owner);

		/// <summary>
		/// Sets the group for the file 
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="group">If it is null, the original groupname remains unchanged</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetGroup(string path, string group);

		/// <summary>
		/// Set replication for an existing file.
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="replicationFactor"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetReplicationFactor(string path, int replicationFactor);

		/// <summary>
		/// Set access time of a file
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="accessTime">Set the access time of this file. The number of milliseconds since Jan 1, 1970. 
		/// A value of -1 means that this call should not set access time</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetAccessTime(string path, string accessTime);

		/// <summary>
		/// Set modification time of a file
		/// </summary>
		/// <param name="path">The string representation a Path.</param>
		/// <param name="modificationTime">Set the modification time of this file. The number of milliseconds since Jan 1, 1970.
		/// A value of -1 means that this call should not set modification time</param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> SetModificationTime(string path, string modificationTime);

		/// <summary>
		/// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
		/// </summary>
		/// <param name="localFile"></param>
		/// <param name="remotePath"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> CreateFile(string localFile, string remotePath);

		/// <summary>
		/// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
		/// </summary>
		/// <param name="localFile"></param>
		/// <param name="remotePath"></param>
		/// <param name="token"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> CreateFile(string localFile, string remotePath, CancellationToken token);

		/// <summary>
		/// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="remotePath"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> CreateFile(Stream content, string remotePath);

		/// <summary>
		/// Opens an FSDataOutputStream at the indicated Path. Files are overwritten by default.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="remotePath"></param>
		/// <param name="token"></param>
		/// <returns>Async <see cref="Task{Boolean}"/> with result of operation.</returns>
		Task<bool> CreateFile(Stream content, string remotePath, CancellationToken token);

		/// <summary>
		/// Event, fired up in case if client returns responseCode other than 20*.
		/// </summary>
		event EventHandler<HttpClientEventArgs> Error;

		/// <summary>
		/// Overridable handler for Error event.
		/// </summary>
		/// <param name="response"><see cref="System.Net.Http.HttpResponseMessage"/>, вернувшийся от сервера</param>
		/// <param name="exception"><see cref="System.Exception"/>, выпавший в процессе запроса</param>
		void OnError(HttpResponseMessage response, Exception exception);
	}
}