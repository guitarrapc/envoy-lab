cat grpcurl_json/eds/update_myservice.json | grpcurl -plaintext -d @ localhost:8080 envoy.EndpointRegisterService.Update
