cat grpcurl_json/eds/discovery_endpoint.json | grpcurl -plaintext -d @ localhost:8080 envoy.api.v2.EndpointDiscoveryService.FetchEndpoints
