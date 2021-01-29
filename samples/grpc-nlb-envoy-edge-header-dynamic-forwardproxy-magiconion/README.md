## grpc-nlb-envoy-edge-header

* run grpc under NLB with Envoy routing via Header.
* NLB (TCP) + Envoy + grpc.
* Routing: Header + k8s svc (Headless)

## App image

* guitarrapc/echo-grpc

> https://github.com/guitarrapc/envoy-lab/blob/master/src/go/README.md


## Deploy Local

* envoy

```shell
getenvoy run standard:1.16.0 -- -c ./envoy_config_dynamic.yaml
```

* upstream

launch `docker-compose up` for https://github.com/guitarrapc/envoy_discovery.

* client

curl to localhost

```shell
// simple
curl -H 'X-Host-Port: 10-1-0-148.echo-rest:8081' http://$MY_DOMAIN:10000
curl -H 'X-Host-Port: 10.1.0.148:8081' http://$MY_DOMAIN:10000

// lua
curl -H 'X-Host-Port: 10-1-0-148' http://$MY_DOMAIN:10000 # use default port pass by envoy.conf
curl -H 'X-Host-Port: 10-1-0-148:8081' http://$MY_DOMAIN:10000 # use header, no change by envoy
curl http://$MY_DOMAIN:10000 # use envoy default route
```

## Deploy Kubernetes

### AWS

add nlb annotations to k8s/envoy-service.yaml

```yaml
metadata:
  annotations:
    service.beta.kubernetes.io/aws-load-balancer-type: "nlb"
spec:
  externalTrafficPolicy: Local
```

## Build Image

> https://cloud.google.com/solutions/exposing-grpc-services-on-gke-using-envoy-proxy?hl=ja

```shell
DOCKER_BUILDKIT=1
docker build -t guitarrapc/echo-magiconion EchoGrpcMagicOnion -f EchoGrpcMagicOnion/EchoGrpcMagicOnion/Dockerfile
docker push guitarrapc/echo-magiconion
```

## Deploy Local

run envoy

```shell
getenvoy run standard:1.16.0 -- -c ./envoy_config_dynamic.yaml
```

run server `EchoGrpcMagicOnion` with VS.

test grpc response

unary
```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Echo -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -message "echo"
```

streaming hub

```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Stream -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -roomName "A" -userName "hoge"
```


## Deploy Kubernetes

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-magiconion
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./k8s |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" | 
    sed -e "s|<namespace>|$NAMESPACE|g" | 
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" | 
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```

test grpc response

unary
```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Echo -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -message "echo"
```

streaming hub

```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Stream -hostPort $MY_DOMAIN:12345 -H 'x-host-port: 10-1-0-210' -roomName "A" -userName "hoge"
```

