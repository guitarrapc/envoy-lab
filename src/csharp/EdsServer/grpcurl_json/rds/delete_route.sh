grpcurl -plaintext -d '{ "route_name": "local_route" }' localhost:8080 envoy.RouteRegisterService.Delete
