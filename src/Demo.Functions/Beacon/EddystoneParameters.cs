using Newtonsoft.Json;

namespace Demo.Functions.Beacon
{
    public class EddystoneParameters
    {
        [JsonProperty("instanceId")]

        public string InstanceId { get; set; }
        [JsonProperty("namespaceId")]

        public string NamespaceId { get; set; }
    }
}
