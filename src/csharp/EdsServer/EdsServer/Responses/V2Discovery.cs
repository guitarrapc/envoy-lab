using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EdsServer.Responses
{
    public class V2DiscoveryResponse
    {
        [JsonPropertyName("version_info")]
        public string VersionInfo { get; set; } = "v1";
        public V2DiscoveryResource[] Resources { get; set; } = new[] { new V2DiscoveryResource() };
    }

    public class V2DiscoveryResource
    {
        [JsonPropertyName("@type")]
        public string Type { get; set; } = "type.googleapis.com/envoy.api.v2.ClusterLoadAssignment";
        [JsonPropertyName("cluster_name")]
        public string? ClusterName { get; set; }
        [JsonPropertyName("endpoints")]
        public IDictionary<string, V2DiscoveryEndpointEndpoint[]>[]? Endpoint { get; set; }
    }

    public class V2DiscoveryEndpointEndpoint
    {
        [JsonPropertyName("endpoint")]
        public V2DiscoveryEndpointAddress? Endpoint { get; set; }
    }

    public class V2DiscoveryEndpointAddress
    {
        [JsonPropertyName("address")]
        public V2DiscoveryAddress? Address { get; set; }
    }

    public class V2DiscoveryAddress
    {
        [JsonPropertyName("socket_address")]
        public V2DiscoverySocketAddress? SocketAddress { get; set; }
    }

    public class V2DiscoverySocketAddress
    {
        [JsonPropertyName("address")]
        public string? Address { get; set; }
        [JsonPropertyName("port_value")]
        public int? PortValue { get; set; }
    }
}
