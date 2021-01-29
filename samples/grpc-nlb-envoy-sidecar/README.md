## grpc-nlb-envoy-edge-header

* run grpc under NLB with sidecar Envoy.
* NLB (TCP) + grpc (envoy sidecar).
* Routing: NLB

## App image

* guitarrapc/reverse-grpc

> https://github.com/guitarrapc/envoy-lab/blob/master/src/go/README.md

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
MY_DOMAIN=envoy-proxy-sidecar.example.com
NAMESPACE=grpc-gke-nlb-sidecar-tutorial
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
grpcurl -d '{"content": "reverse"}' -proto reverse-grpc/api/reverse.proto -insecure -v $MY_DOMAIN:443 api.Reverse/Reverse
```