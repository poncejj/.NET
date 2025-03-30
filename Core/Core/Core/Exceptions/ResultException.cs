using System.Net;

namespace Core.Exceptions {

    public class ResultException : Exception {

        public ResultException() {
        }

        public ResultException(string message) : base(message) {
        }

        public ResultException(string message, Exception innerException) : base(message, innerException) {
        }

        public string Key { get; set; }
        public string Reason { get; set; }
        public string ErrorMessage { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}