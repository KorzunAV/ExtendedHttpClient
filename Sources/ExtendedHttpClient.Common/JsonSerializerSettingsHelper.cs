using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExtendedHttpClient.Common
{
    public class JsonSerializerSettingsHelper
    {
        public static JsonSerializerSettings SnakeCaseJsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
        
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings();
    }
}
