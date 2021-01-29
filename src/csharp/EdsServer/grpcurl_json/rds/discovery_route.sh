cat grpcurl_json/rds/discovery_route.json | grpcurl -plaintext -d @ localhost:8080 envoy.api.v2.RouteDiscoveryService.FetchRoutes
