
##Docker Lokal
docker build  -t play.catalog:1.0 .                

docker run -it --rm -p 5000:5000 --name catalog -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq  --network playinfra_default play.catalog:1.0


##Docker via Repo
docker login git-registry.gibb.ch
Username: thomas.staub
Zugangstoaken unter Profil Zugangstoaken zu finden / generieren

docker build -t git-registry.gibb.ch/thomas.staub/microservices/play.catalog:1.0 .

docker push git-registry.gibb.ch/thomas.staub/microservices/play.catalog:1.0

docker run -it --rm -p 5000:5000 --name catalog -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network playinfra_default registry.gitlab.com/thomas-staub/cloudmodules/microservices/play.catalog:1.0

