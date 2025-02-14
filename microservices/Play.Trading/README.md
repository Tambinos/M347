##Docker Lokal
docker build  -t play.trading:1.0 .                

docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq  --network playinfra_default play.trading:1.0


##Docker via Repo

docker login git-registry.gibb.ch




docker build  -t  git-registry.gibb.ch/thomas.staub/microservices/play.trading:1.0 .

docker push git-registry.gibb.ch/thomas.staub/microservices/play.trading:1.0

docker run -it --rm -p 5006:5006 --name trading -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq --network playinfra_default git-registry.gibb.ch/thomas.staub/microservices/play.trading:1.0

