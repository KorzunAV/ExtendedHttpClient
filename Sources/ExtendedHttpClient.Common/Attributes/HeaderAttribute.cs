using System;

namespace ExtendedHttpClient.Common.Attributes
{
    public class HeaderAttribute : Attribute
    {
        public string HeaderKey { get; }
        public bool IsAuthorization { get; set; } = false;

        public HeaderAttribute(string headerKey)
        {
            HeaderKey = headerKey;
        }
    }
}