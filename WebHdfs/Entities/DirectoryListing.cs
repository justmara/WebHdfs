using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// Entity for Directory listing.
    /// </summary>
    /// <inheritdoc cref="IJObject" />
    public class DirectoryListing : IJObject
    {
        IEnumerable<DirectoryEntry> directoryEntries;

        /// <inheritdoc />
        public void Parse(JObject rootEntry)
        {
            directoryEntries = rootEntry.Value<JObject>("FileStatuses").Value<JArray>("FileStatus").Select(fs =>
            {
                var d = new DirectoryEntry();
                d.Parse(fs.Value<JObject>());
                return d;
            });
        }

        /// <summary>
        /// List of subdirectories 
        /// </summary>
        public IEnumerable<DirectoryEntry> Directories
        { get { return directoryEntries.Where(fs => fs.Type == "DIRECTORY"); } }

        /// <summary>
        /// List of files
        /// </summary>
        public IEnumerable<DirectoryEntry> Files
        { get { return directoryEntries.Where(fs => fs.Type == "FILE"); } }
    }
}
