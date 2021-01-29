MagicOnion Client & Server for Envoy lab

## Build Image

* EchoGrpcMagicOnion

```shell
DOCKER_BUILDKIT=1
docker build -t echo-magiconion:latest3 EchoGrpcMagicOnion -f EchoGrpcMagicOnion/EchoGrpcMagicOnion/Dockerfile
docker tag echo-magiconion:latest3 guitarrapc/echo-magiconion:3.0.13
docker push guitarrapc/echo-magiconion:3.0.13
```

* EchoGrpcMagicOnion v4

```shell
DOCKER_BUILDKIT=1
docker build -t echo-magiconion:latest4 EchoGrpcMagicOnionV4 -f EchoGrpcMagicOnionV4/EchoGrpcMagicOnion/Dockerfile
docker tag echo-magiconion:latest4 guitarrapc/echo-magiconion:4.0.0-preview-1
docker push guitarrapc/echo-magiconion:4.0.0-preview-1
```

* EdsServer

> reference xds-server with Go impl.

```shell
docker build -t envoy_discovery_sds_rest:3.1 -f envoy/grpc-nlb-envoy-eds/EdsServer/EdsServer/Dockerfile envoy/grpc-nlb-envoy-eds/EdsServer
docker tag envoy_discovery_sds_rest:3.1 guitarrapc/envoy_discovery_sds_rest:3.1
docker tag envoy_discovery_sds_rest:3.1 guitarrapc/envoy_discovery_sds_rest:latest
docker push guitarrapc/envoy_discovery_sds_rest:3.1
docker push guitarrapc/envoy_discovery_sds_rest:latest
```

