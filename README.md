## envoy-lab

## Fundamentals

### Access Envoy admin running on Kubernetes

port forward to envoy pod's admin port.
```shell
kubectl port-forward $(kubectl get pods -o name | grep envoy | head -n1) 8080:9901
```

now you can acces to admin portal via `localhost:8080`

