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

add alb annotations to ingress.yaml

```yaml
metadata:
  annotations:
    kubernetes.io/ingress.class: alb
    alb.ingress.kubernetes.io/scheme: internet-facing
    alb.ingress.kubernetes.io/target-type: ip
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
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/rest |
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
# simple
curl -H 'X-Host-Port: 10-0-2-133.echo-rest:8081' http://$MY_DOMAIN
curl -H 'X-Host-Port: 10.0.2.133:8081' http://$MY_DOMAIN

# lua - debug
curl -H 'X-Host-Port: 10-0-2-133' http://$MY_DOMAIN # use default port pass by envoy.conf
curl -H 'X-Host-Port: 10-0-2-133:8081' http://$MY_DOMAIN # use header, no change by envoy
curl http://$MY_DOMAIN # use envoy default route

# lua
curl -H 'X-Host-Port: 10-0-2-133' http://$MY_DOMAIN # use default port pass by envoy.conf
curl http://$MY_DOMAIN # use envoy default route
```

### rest + tls

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-rest-tls
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/rest_tls |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-rest  -o yaml | grep podIP
```

run curl through envoy

```shell
# simple
curl -H 'X-Host-Port: 10-0-2-5.echo-rest:8081' https://$MY_DOMAIN
curl -H 'X-Host-Port: 10.0.2.5:8081' https://$MY_DOMAIN

# lua - debug
curl -H 'X-Host-Port: 10-0-2-5' https://$MY_DOMAIN # use default port pass by envoy.conf
curl -H 'X-Host-Port: 10-0-2-5:8081' https://$MY_DOMAIN # use header, no change by envoy
curl https://$MY_DOMAIN # use envoy default route

# lua
curl -H 'X-Host-Port: 10-0-2-5' https://$MY_DOMAIN # use default port pass by envoy.conf
curl https://$MY_DOMAIN # use envoy default route
```

### http2

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-rest-http2
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
TARGET_GROUP=arn:aws:elasticloadbalancing:us-west-2:xxxxx:targetgroup/xxxxxxx/xxxxxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/rest_http2 |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    sed -e "s|<targetgroup>|$TARGET_GROUP|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-rest  -o yaml | grep podIP
```

run curl through envoy

```shell
# lua - debug
curl -H 'X-Host-Port: 10-0-2-111' https://$MY_DOMAIN # use default port pass by envoy.conf
curl -H 'X-Host-Port: 10-0-2-111:8081' https://$MY_DOMAIN # use header, no change by envoy
curl https://$MY_DOMAIN # use envoy default route

# lua
curl -H 'X-Host-Port: 10-0-2-111' https://$MY_DOMAIN # use default port pass by envoy.conf
curl https://$MY_DOMAIN # use envoy default route
```


### grpc with http2 targetgroup

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-grpc-http2
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
TARGET_GROUP=arn:aws:elasticloadbalancing:us-west-2:xxxxx:targetgroup/xxxxxxx/xxxxxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/grpc_http2 |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    sed -e "s|<targetgroup>|$TARGET_GROUP|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```

run grpcurl through envoy

```shell
# dynamic + lua
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10-0-2-227' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
```

### grpc with grpc targetgroup

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-grpc
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
TARGET_GROUP=arn:aws:elasticloadbalancing:us-west-2:xxxxx:targetgroup/xxxxxxx/xxxxxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/grpc_grpctg |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    sed -e "s|<targetgroup>|$TARGET_GROUP|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```

run grpcurl through envoy

```shell
# dynamic + lua
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10-0-2-227' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
```

### magiconion with grpc targetgroup

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-grpc-http2
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
TARGET_GROUP=arn:aws:elasticloadbalancing:us-west-2:xxxxx:targetgroup/xxxxxxx/xxxxxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/grpc_magiconion |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    sed -e "s|<targetgroup>|$TARGET_GROUP|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```

run chatapp unity client.

```csharp
this.channel = new Channel($MY_DOMAIN, 443, new SslCredentials());
```

### magiconion v4 with grpc targetgroup

deploy app and service.

```shell
MY_DOMAIN=dummy.example.com
NAMESPACE=envoy-grpc-magiconion
ACM=arn:aws:acm:us-west-2:xxxxx:certificate/xxxxxxx
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

deploy app
```shell
kubectl kustomize ./grpc-alb-envoy-edge-header-dynamic-forwardproxy/grpc_magiconionv4 |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<acm>|$ACM|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    kubectl apply -f -
```

get pod ip.
```shell
k get pod -l app=echo-grpc -o yaml | grep podIP
```

run MagicOnion v4 client.

```csharp
this.channel = new Channel($MY_DOMAIN, 443, new SslCredentials());
```

## test grpc response from envoy pod

```shell
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: echo-grpc-74c97dcc47-67pvc.echo-grpc.grpc-nlb-envoy-edge-header-dynamic.svc.cluster.local:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'X-Host-Port: 10.1.0.35:8081' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -plaintext -v $MY_DOMAIN:10000 api.Echo/Echo
```

## debug envoy header routing.

use `envoy-configmap_debug.yaml` to check how envoy detect headers.
