https://www.youtube.com/watch?v=_lpDfMkxccc


kubectl apply -n rabbits -f namespace.yaml
kubectl apply -n rabbits -f rabbit-rbac.yaml
kubectl apply -n rabbits -f rabbit-configmap.yaml
kubectl apply -n rabbits -f rabbit-secret.yaml
kubectl apply -n rabbits -f rabbit-statefulset.yaml

kubectl -n rabbits port-forward rabbitmq-0 8080:15672