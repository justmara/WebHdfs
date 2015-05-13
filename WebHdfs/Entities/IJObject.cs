using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// Base interface for JObject-based entities.
    /// </summary>
    public interface IJObject
    {
        /// <summary>
        /// Parse JObject instance.
        /// </summary>
        /// <param name="obj">Instance of <see cref="JObject"/>, received from HttpClient</param>
        void Parse(JObject obj);
    }
}