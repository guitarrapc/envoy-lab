using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EdsServer.Models
{
    public class Req
    {
        [JsonPropertyName("node")]
        public Node? Node { get; set; }
        [JsonPropertyName("resource_names")]
        public string[]? ResourceNames { get; set; }
    }

    public class Node
    {
        [JsonPropertyName("build_version")]
        public string BuildVersion { get; set; } = "fd44fd6051f5d1de3b020d0e03685c24075ba388/1.6.0-dev/Clean/RELEASE";
        [JsonPropertyName("cluster")]
        public string Cluster { get; set; } = "mycluster";
        [JsonPropertyName("id")]
        public string Id { get; set; } = "test-id";
    }
}
