using System;

namespace ExtendedHttpClient.Common.Attributes
{
    public enum BodyMimeType
    {
        ApplicationJson,
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BodyAttribute : Attribute
    {
        public BodyMimeType Type { get; }

        public BodyAttribute(BodyMimeType type)
        {
            Type = type;
        }
    }
}
