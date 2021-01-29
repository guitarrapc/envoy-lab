Grpc Server for Envoy lab

## Build Image

> https://cloud.google.com/solutions/exposing-grpc-services-on-gke-using-envoy-proxy?hl=ja

```shell
DOCKER_BUILDKIT=1 docker build -t guitarrapc/echo-grpc echo-grpc
docker push guitarrapc/echo-grpc
DOCKER_BUILDKIT=1 docker build -t guitarrapc/reverse-grpc reverse-grpc
docker push guitarrapc/reverse-grpc
```

## original and changes

### original

> https://github.com/GoogleCloudPlatform/grpc-gke-nlb-tutorial


### changes from original

* echo-grpc/go.mod
* reverse-grpc/go.mod

```go
- google.golang.org/grpc v1.21.0
+ google.golang.org/grpc v1.29.1
```

## personnal image

* [guitarrapc/echo-grpc:latest](https://hub.docker.com/r/guitarrapc/echo-grpc)
* [guitarrapc/reverse-grpc:latest](https://hub.docker.com/r/guitarrapc/reverse-grpc)
