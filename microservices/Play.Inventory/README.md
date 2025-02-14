##Docker Lokal
docker build  -t play.inventory:1.0 .                

docker run -it --rm -p 5004:5004 --name inventory -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq  --network playinfra_default play.inventory:1.0


##Docker via Repo

docker login git-registry.gibb.ch




docker build -t git-registry.gibb.ch/thomas.staub/microservices/play.inventory:1.0 .

docker push git-registry.gibb.ch/thomas.staub/microservices/play.inventory:1.0


docker run -it --rm -p 5004:5004 --name inventory -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network playinfra_default git-registry.gibb.ch/thomas.staub/microservices/play.inventory:1.0
