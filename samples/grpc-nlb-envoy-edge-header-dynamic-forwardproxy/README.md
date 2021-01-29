## grpc-nlb-envoy-edge-header

* run grpc under NLB with Envoy routing via Header.
* NLB (TCP) + Envoy + grpc.
* Routing: Header + k8s svc (Headless)

## App image

* guitarrapc/echo-grpc

> https://github.com/guitarrapc/envoy-lab/blob/master/src/go/README.md

## Kubernetes

### Local (could not work.....)

add nginx ingress to distribute EXTERANL host to svc.

```shell
  externalIPs:
  - $(hostname -I | cut -d' ' -f1)
```

### AWS

add nlb annotations to k8s/envoy-service.yaml

```yaml
metadata:
  annotations:
    service.beta.kubernetes.io/aws-load-balancer-type: "nlb"
spec:
  externalTrafficPolicy: Local
```

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

### rest

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-rest
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./k8s_rest |
    sed -e "s|<namespace>|$NAMESPACE|g" | 
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" | 
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-rest  -o yaml | grep podIP
```

run curl through envoy

```shell
// simple
curl -H 'X-Host-Port: 10-1-1-48.echo-rest:8081' http://$MY_DOMAIN:10000
curl -H 'X-Host-Port: 10.1.1.48:8081' http://$MY_DOMAIN:10000

// lua
curl -H 'X-Host-Port: 10-1-1-48' http://$MY_DOMAIN:10000 # use default port pass by envoy.conf
curl -H 'X-Host-Port: 10-1-1-48:8081' http://$MY_DOMAIN:10000 # use header, no change by envoy
curl http://$MY_DOMAIN:10000 # use envoy default route
```

### grpc non tls

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-grpc
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

run grpcurl through envoy

```shell
# simple dynamic
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10-1-0-174.echo-grpc:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10.1.0.174:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
# dynamic + lua
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10-1-0-174' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
```


## test grpc response from envoy pod

```shell
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: echo-grpc-74c97dcc47-67pvc.echo-grpc.grpc-nlb-envoy-edge-header-dynamic.svc.cluster.local:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10.1.0.35:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
```

## debug envoy header routing.

use `envoy-configmap_debug.yaml` to check how envoy detect headers.
