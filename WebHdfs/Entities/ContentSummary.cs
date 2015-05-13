using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// Content summary.
    /// </summary>
    /// <inheritdoc cref="IJObject" />
    public class ContentSummary : IJObject
    {
        /// <summary>
        /// The number of directories.
        /// </summary>
        public int DirectoryCount
        { get; set; }

        /// <summary>
        /// The number of files.
        /// </summary>
        public int FileCount
        { get; set; }

        /// <summary>
        /// The number of bytes used by the content.
        /// </summary>
        public long Length
        { get; set; }

        /// <summary>
        /// The namespace quota of this directory.
        /// </summary>
        public int Quota
        { get; set; }

        /// <summary>
        /// The disk space consumed by the content.
        /// </summary>
        public long SpaceConsumed
        { get; set; }

        /// <summary>
        /// The disk space quota.
        /// </summary>
        public long SpaceQuota
        { get; set; }

        /// <inheritdoc />
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
}