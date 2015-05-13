using Newtonsoft.Json.Linq;

namespace WebHdfs.Entities
{
    /// <summary>
    /// Parses boolean resul
    /// </summary>
    /// <inheritdoc cref="IJObject" />
    public class BooleanResult : IJObject
    {
        /// <summary>
        /// Value retrieved
        /// </summary>
        public bool Value { get; set; }

        /// <inheritdoc />
        public void Parse(JObject obj)
        {
            Value = obj.Value<bool>("boolean");
        }
    }
}
