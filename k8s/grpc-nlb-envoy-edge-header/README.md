## grpc-nlb-envoy-edge-header

* run grpc under NLB with Envoy routing via Header.
* NLB (TCP) + Envoy + grpc.
* Routing: Header + k8s svc

## App image

* guitarrapc/echo-grpc

> https://github.com/guitarrapc/envoy-lab/blob/master/src/go/README.md


### Local (could not work.....)

add nginx ingress to distribute EXTERANL host to svc.

```shell
  externalIPs:
  - $(hostname -I | cut -d' ' -f1)
```

### AWS

add nlb annotations to envoy-service.yaml

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
MY_DOMAIN=envoy-proxy-edge-header.example.com
NAMESPACE=grpc-gke-nlb-edge-header
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
kubectl kustomize ./grpc-nlb-envoy-edge-header |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    kubectl apply -f -
```

## test grpc response

```shell
grpcurl -d '{"content": "echo"}' -H 'x-pod-name: pod-name' -H 'method: POST' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $MY_DOMAIN:443 api.Echo/Echo
```
