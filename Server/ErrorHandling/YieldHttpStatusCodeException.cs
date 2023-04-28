using System.Net;

namespace StronglyTypedApi.Server.ErrorHandling
{
    public class YieldHttpStatusCodeException : ApplicationException
    {

        public YieldHttpStatusCodeException(HttpStatusCode code, string message) : base(message)
        {
            this.StatusCode = code;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
