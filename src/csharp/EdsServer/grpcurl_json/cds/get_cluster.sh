grpcurl -plaintext -d '{}' localhost:8080 envoy.ClusterRegisterService.List
grpcurl -plaintext -d '{ "service_name": "my_clusters" }' localhost:8080 envoy.ClusterRegisterService.Get
