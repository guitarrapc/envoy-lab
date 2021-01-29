cat grpcurl_json/cds/update_cluster.json | grpcurl -plaintext -d @ localhost:8080 envoy.ClusterRegisterService.Update
