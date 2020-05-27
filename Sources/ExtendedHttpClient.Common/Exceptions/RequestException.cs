using System;
using System.Net;

namespace ExtendedHttpClient.Common.Exceptions
{
    public sealed class RequestException : Exception
    {
        public HttpStatusCode ResponseStatusCode { get; set; }


        public string RawRequest
        {
            get
            {
                if (Data.Contains("RawRequest"))
                    return (string)Data["RawRequest"];
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (Data.Contains("RawRequest"))
                        Data["RawRequest"] = value;
                    else
                        Data.Add("RawRequest", value);
                }
            }
        }

        public string RawResponse
        {
            get
            {
                if (Data.Contains("RawResponse"))
                    return (string)Data["RawResponse"];
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (Data.Contains("RawResponse"))
                        Data["RawResponse"] = value;
                    else
                        Data.Add("RawResponse", value);
                }
            }
        }


        public RequestException(string rawRequest, string rawResponse, HttpStatusCode responseStatusCode)
        {
            RawRequest = rawRequest;
            RawResponse = rawResponse;
            ResponseStatusCode = responseStatusCode;
        }

        public override string ToString()
        {
            return $"{ResponseStatusCode} {RawResponse}";
        }
    }
}
