## grpc-nlb-envoy-edge-header

* run grpc (MagicOnion) under NLB with Envoy routing via Header.
* NLB (TCP) + Envoy + grpc (MagicOnion).
* Routing: Header + k8s svc (Headless)

## App image

* guitarrapc/echo-magiconion

> https://github.com/guitarrapc/envoy-lab/blob/master/src/csharp/README.md


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
kubectl kustomize ./grpc-nlb-envoy-edge-header-headless-magiconion |
    sed -e "s|gcr.io/GOOGLE_CLOUD_PROJECT|guitarrapc|g" |
    sed -e "s|<namespace>|$NAMESPACE|g" |
    sed -e "s|\.default|.$NAMESPACE|g" |
    sed -e "s|<domain>|$MY_DOMAIN|g" |
    kubectl apply -f -
```

## test grpc response

unary
```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Echo -hostPort $MY_DOMAIN:12345 -H 'x-pod-name: echo-grpc-0' -message "echo"
```

streaming hub

```shell
dotnet run --project EchoGrpcMagicOnion/EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj Stream -hostPort $MY_DOMAIN:12345 -H 'x-pod-name: echo-grpc-0' -roomName "A" -userName "hoge"
```
