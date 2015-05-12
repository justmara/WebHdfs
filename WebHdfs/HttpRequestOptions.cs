using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebHdfs
{
    /// <summary>
    /// Опции запроса
    /// </summary>
    public class HttpRequestOptions
    {
        /// <summary>
        /// дефолтовый конструктор
        /// </summary>
        public HttpRequestOptions()
        {
            Completion = HttpCompletionOption.ResponseContentRead;
            Token = CancellationToken.None;
            Formatter = new JsonMediaTypeFormatter();
            Method = HttpMethod.Get;
        }

        /// <summary>
        /// Условие завершения обработки
        /// </summary>
        public HttpCompletionOption Completion
        { get; set; }

        /// <summary>
        /// Токен для отмены
        /// </summary>
        public CancellationToken Token
        { get; set; }

        /// <summary>
        /// Форматировщик тела запроса
        /// </summary>
        public MediaTypeFormatter Formatter
        { get; set; }

        /// <summary>
        /// HTTP-метод
        /// </summary>
        public HttpMethod Method
        { get; set; }
    }
}
