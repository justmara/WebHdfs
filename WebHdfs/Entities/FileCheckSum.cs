using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// File checksum info
    /// </summary>
    /// <inheritdoc cref="IJObject" />
    public class FileChecksum : IJObject
    {
        /// <summary>
        /// The name of the checksum algorithm.
        /// </summary>
        public string Algorithm
        { get; set; }

        /// <summary>
        /// The byte sequence of the checksum in hexadecimal.
        /// </summary>
        public string Checksum
        { get; set; }

        /// <summary>
        /// The length of the bytes (not the length of the string).
        /// </summary>
        public int Length
        { get; set; }

        /// <inheritdoc />
        public void Parse(JObject value)
        {
            Algorithm = value.Value<string>("algorithm");
            Checksum = value.Value<string>("bytes");
            Length = value.Value<int>("length");
        }
    }
}