grpcurl -plaintext -d '{}' localhost:8080 envoy.RouteRegisterService.List
grpcurl -plaintext -d '{ "route_name": "local_route" }' localhost:8080 envoy.RouteRegisterService.Get
