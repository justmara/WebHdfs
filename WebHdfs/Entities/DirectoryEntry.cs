using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// Directory info.
    /// </summary>
    /// <inheritdoc cref="IJObject" />
    public class DirectoryEntry : IJObject
    {
        /// <summary>
        /// The access time.
        /// </summary>
        public long AccessTime
        { get; set; }

        /// <summary>
        /// Block size
        /// </summary>
        public long BlockSize
        { get; set; }

        /// <summary>
        /// Owner group
        /// </summary>
        public string Group
        { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        public long Length
        { get; set; }

        /// <summary>
        /// Last modification time
        /// </summary>
        public long ModificationTime
        { get; set; }

        /// <summary>
        /// Owner
        /// </summary>
        public string Owner
        { get; set; }

        /// <summary>
        /// The path suffix.
        /// </summary>
        /// <remarks>Really, why not?</remarks>
        public string PathSuffix
        { get; set; }

        /// <summary>
        /// The permission represented as a octal string.
        /// </summary>
        public string Permission
        { get; set; }

        /// <summary>
        /// Replication factor
        /// </summary>
        public int Replication
        { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type
        { get; set; }

        /// <inheritdoc />
        public void Parse(JObject value)
        {
            var tmp = value.Value<JObject>("FileStatus") ?? value;
            AccessTime = tmp.Value<long>("accessTime");
            BlockSize = tmp.Value<long>("blockSize");
            Group = tmp.Value<string>("group");
            Length = tmp.Value<long>("length");
            ModificationTime = tmp.Value<long>("modificationTime");
            Owner = tmp.Value<string>("owner");
            PathSuffix = tmp.Value<string>("pathSuffix");
            Permission = tmp.Value<string>("permission");
            Replication = tmp.Value<int>("replication");
            Type = tmp.Value<string>("type");
        }
    }
}
