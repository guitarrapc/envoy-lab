## grpc-nlb-envoy

* run grpc under NLB with Envoy.
* NLB (TCP) + Envoy (TLS Terminate) + grpc
* Routing: NLB + k8s svc


## App image

* guitarrapc/echo-grpc
* guitarrapc/reverse-grpc

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
MY_DOMAIN=example.com
NAMESPACE=grpc-gke-nlb-tutorial
kubectl kustomize ./grpc-nlb-envoy |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    kubectl apply -f -
```

create self signed cert.

```shell
kubens grpc-gke-nlb-tutorial
EXTERNAL_IP=$(kubectl get service envoy -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
EXTERNAL_IP=$(kubectl get service envoy -o jsonpath='{.metadata.annotations.external-dns\.alpha\.kubernetes\.io/hostname}')
openssl req -x509 -nodes -newkey rsa:2048 -days 365 -keyout privkey.pem -out cert.pem -subj "/CN=$EXTERNAL_IP"
kubectl create secret tls envoy-certs --key privkey.pem --cert cert.pem --dry-run -o yaml | kubectl apply -f -
```

deploy envoy

```shell
cat k8s/envoy-configmap.yaml | sed -e "s|\.default|.$NAMESPACE|g" | kubectl apply -f -
kubectl apply -f k8s/envoy-deployment.yaml
```

## test grpc response

```shell
grpcurl -d '{"content": "echo"}' -proto ../../src/go/echo-grpc/api/echo.proto -insecure -v $EXTERNAL_IP:443 api.Echo/Echo
grpcurl -d '{"content": "reverse"}' -proto reverse-grpc/api/reverse.proto -insecure -v $EXTERNAL_IP:443 api.Reverse/Reverse
```
