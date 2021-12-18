using System;
using System.Collections.Generic;
using System.Net;

namespace ExtendedHttpClient.Common
{
    public class OperationResult
    {
        public bool IsSuccess => Exception == null;

        public Exception Exception { get; set; }

        public string RawResponse { get; set; }

        public string RawRequest { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public KeyValuePair<string, IEnumerable<string>>[] Headers { get; set; }

        public OperationResult()
        {
        }

        public OperationResult(Exception exception)
        {
            Exception = exception;
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Result { get; set; }

        public OperationResult() { }

        public OperationResult(Exception exception) : base(exception) { }

        public OperationResult(T result)
        {
            Result = result;
        }
    }
}
