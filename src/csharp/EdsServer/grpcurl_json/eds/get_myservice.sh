grpcurl -plaintext -d '{}' localhost:8080 envoy.EndpointRegisterService.List
grpcurl -plaintext -d '{ "service_name": "myservice" }' localhost:8080 envoy.EndpointRegisterService.Get
