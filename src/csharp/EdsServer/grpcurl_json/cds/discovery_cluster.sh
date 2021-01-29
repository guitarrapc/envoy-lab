cat grpcurl_json/cds/discovery_cluster.json | grpcurl -plaintext -d @ localhost:8080 envoy.api.v2.ClusterDiscoveryService.FetchClusters
