using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EdsServer.Responses
{
    public class Service
    {
        [JsonPropertyName("hosts")]
        public Host[]? Hosts { get; set; }
    }

    public class Host
    {
        [JsonPropertyName("ip_address")]
        public string IPAddress { get; set; } = "us-central-a";
        [JsonPropertyName("port")]
        public int Port { get; set; } = 18080;
        [JsonPropertyName("tags")]
        public Tags? Tags { get; set; }
    }

    public class Tags
    {
        [JsonPropertyName("as")]
        public string Az { get; set; } = "us-central-a";
        [JsonPropertyName("canary")]
        public bool Canary { get; set; }
        [JsonPropertyName("load_balancing_weight")]
        public int LoadBalancingWeight { get; set; } = 50;
    }
}
