cat grpcurl_json/cds/add_clusters.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Add
