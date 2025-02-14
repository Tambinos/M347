
# Play:Infra

##Add the package source


#Troubleshooting in Docker 
Vi Docker Desktop ins Terminal, geht nur wenn der Container nicht auf den User beschränkt ist. 
#RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
#USER appuser



apt-get -y update; apt-get -y install curl
apt-get install -y iputils-ping

curl  http://identity:5002/.well-known/openid-configuration 
curl       http://itentity:5002/.well-known/openid-configuration

curl  https://host.docker.internal:5003/.well-known/openid-configuration 

host.docker.internal

curl --insecure https://identity:5003/.well-known/openid-configuration 

wget https://identity:5003/.well-known/openid-configuration 

curl -4 https://localhost:5003/.well-known/openid-configuration

curl https://localhost:5001/swagger/index.html

curl http://localhost:5002/.well-known/openid-configuration


 docker-compose -f .\docker-compose-all.yml up


 http://host.docker.internal:3000/
 http://host.docker.internal:5008/
 