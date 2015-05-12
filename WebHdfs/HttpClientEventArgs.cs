using System;
using System.Net.Http;

namespace WebHdfs
{
    /// <summary>
    /// HttpClient Event Arguments holder
    /// </summary>
    public class HttpClientEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="System.Net.Http.HttpResponseMessage"/> returned by client.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        /// <summary>
        /// <see cref="System.Exception"/> thrown while executing request.
        /// </summary>
        /// <tocexclude />
        public Exception Exception { get; private set; }

        /// <summary>
        /// Public constructor (really)
        /// </summary>
        /// <param name="response"><see cref="System.Net.Http.HttpResponseMessage"/> returned by client.</param>
        /// <param name="exception"><see cref="System.Exception"/> thrown while executing request.</param>
        public HttpClientEventArgs(HttpResponseMessage response, Exception exception)
        {
            Response = response;
            Exception = exception;
        }
    }
}
