## grpc-nlb-envoy-edge-header

* run grpc under NLB with Envoy (xDS Server)
* NLB (TCP) + Envoy xDS Server.
* Routing: NLB + envoy xDS SErver + k8s svc

## App image

* guitarrapc/echo-grpc

> https://github.com/guitarrapc/envoy-lab/blob/master/src/go/README.md

## xDS image

* guitarrapc/envoy_discovery_sds_rest:3.1

> https://github.com/guitarrapc/envoy-lab/blob/master/src/csharp/README.md

## xDS API Reference

* [data-plane-api/API_OVERVIEW.md at master · envoyproxy/data-plane-api](https://github.com/envoyproxy/data-plane-api/blob/master/API_OVERVIEW.md)
  * [EDS](https://github.com/envoyproxy/data-plane-api/blob/master/envoy/api/v2/eds.proto)
  * [LDS](https://github.com/envoyproxy/data-plane-api/blob/master/envoy/api/v2/lds.proto)

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


## Deploy Kubernetes

deploy app and service.

```shell
MY_DOMAIN=envoy-proxy-edge-header-eds.example.com
NAMESPACE=grpc-nlb-envoy-edge-header-headless-eds
```

prepare ns

```shell
kubectl create namespace $NAMESPACE
kubens $NAMESPACE
```

create self signed cert.

```shell
openssl req -x509 -nodes -newkey rsa:2048 -days 365 -keyout privkey.pem -out cert.pem -subj "/CN=$MY_DOMAIN"
kubectl create secret tls envoy-certs --key privkey.pem --cert cert.pem --dry-run -o yaml | kubectl apply -f -
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

## test grpc response

```shell
grpcurl -d '{"content": "echo"}' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
grpcurl -d '{"content": "echo"}' -H 'x-pod-name: echo-grpc-0' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
```

X-Route-Selector: vip=172.10.10.20







[2020-10-19 15:10:43.893][7365][warning][config] [source/common/config/http_subscription_impl.cc:124] REST config for /v2/discovery:endpoints rejected: Unable to parse JSON as proto (INVALID_ARGUMENT:: Root element must be a message.): [{"version_info":"v1","resources":[{"@type":"type.googleapis.com/envoy.api.v2.ClusterLoadAssignment","cluster_name":"myservice","endpoint":{"lb_endpoints":[{"address":{"socket_address":{"address":"127.0.0.1","port_value":8081}}},{"address":{"socket_address":{"address":"127.0.0.1","port_value":8082}}},{"address":{"socket_address":{"address":"127.0.0.1","port_value":8083}}}]}}]}]