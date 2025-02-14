
# Wichtig
#bedeutet dies muss nicht augeführt werden, das ist nur ein ev. nützlicher Hinweis  

# Namespace erstellen
kubectl apply  -f namespace.yaml
# Mongo
#helm repo add bitnami https://charts.bitnami.com/bitnami  (Falls das Repo nicht erkannt wird)  
helm install -f .\mongo\my-mongo-values.yaml mongo  bitnami/mongodb --namespace play  
#helm delete mongo --namespace play  (Wenn man es wieder löschen will)  

# Mongo-Expres
helm repo add truecharts https://charts.truecharts.org/  
helm install -f .\mongo\my-mongo-express-values.yaml mongo-expres truecharts/mongo-express --version 1.0.10  --namespace play  
#helm delete mongo-expres --namespace play   (Wenn man es wieder löschen will)  
Pordforwarding auf Pod möglich mit:  
#kubectl -n play port-forward mongodb-express-service 8081:8081  

------
# Rabbitmq
kubectl apply  -f rabbitmq\rabbit-rbac.yaml  
kubectl apply  -f rabbitmq\rabbit-configmap.yaml  
kubectl apply  -f rabbitmq\rabbit-secret.yaml  
kubectl apply  -f rabbitmq\rabbit-statefulset.yaml



hat kein Manager  
#kubectl -n play port-forward svc/rabbitmq 15672:15672  

# Tradeing 
kubectl apply  -f tradeing.yaml


# Catalog 
kubectl apply  -f catalog.yaml


# Inventory 
kubectl apply  -f inventory.yaml


# Identity 
kubectl apply  -f identity.yaml


# Frontend1 
kubectl apply  -f frontend1.yaml

# Frontend2 
kubectl apply  -f frontend2.yaml

# Kiali

kubectl apply  -f kiali.yaml
# Prometheus
kubectl apply  -f prometheus.yaml

# seq
helm repo add cloudnativeapp https://cloudnativeapp.github.io/charts/curated/  
helm install seq cloudnativeapp/seq --version 1.0.0 --namespace play  

#kubectl -n play port-forward seq-646f68d64f-d6ddd 8000:80   

#helm delete seq --namespace play  



# Prometheus
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts  
helm install -f .\prometheus\my-prometheus-values.yaml  prometheus prometheus-community/prometheus --version 24.5.0  --namespace play  

#kubectl -n play port-forward svc/prometheus-server 9090:80  

#helm delete prometheus --namespace play  

# Jaeger
helm install -f .\jaeger\my-jaeger-values.yaml jaeger jaeger-all-in-one/jaeger-all-in-one --version 0.1.11 --namespace play  
#helm delete jaeger --namespace play  

# Grafana
helm install -f .\grafana\my-grafana-values.yaml grafana bitnami/grafana --version 9.1.1 --namespace play   
kubectl -n play  port-forward svc/grafana 8085:3000   

#helm delete grafana --namespace play  


# Troubleshooting 
Portforward auf Pod  
#kubectl -n play port-forward catalog-deployment-5d49bdff6c-g595x 5000:80 


https://artifacthub.io/
https://helm.sh/

