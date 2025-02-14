# Play.Identity
Play Economy Identity microservice


cd 
## Run the docker image
```powershell
$adminPass="[PASSWORD HERE]"
docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentitySettings__AdminUserPassword=$adminPass --network playinfra_default play.identity:$version
```


## Lokale Nuget Datei
C:\Users\thomas.staub\AppData\Roaming\NuGet


## Ein Nuget nach devops Azure pushen
C:\Users\thomas.staub\Downloads\nuget.exe sources add -Name Play.Common -Source "https://pkgs.dev.azure.com/staub-microservices/Play.Common/_packaging/Play.Common/nuget/v3/index.json" -username play.common -password [Toaken]

C:\Users\thomas.staub\Downloads\nuget.exe sources list

C:\Users\thomas.staub\Downloads\nuget.exe push -Source "Play.Common" -ApiKey az C:\Temp\Net_Kurs1\Dotnetmicroservices_Kurs1\packages\Play.Common.1.0.7.nupkg




##Docker Lokal
$adminPass="[PASSWORD HERE]"

docker build  -t play.identity:1.0 .
docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentitySettings__AdminUserPassword=$adminPass --network playinfra_default play.identity:1.0


##Ein ZErtifikat generieren
 dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx"  -p my-password-for-the-Cert+Start.is!not!so.complex! 

##Docker via Repo

docker login git-registry.gibb.ch


docker build -t git-registry.gibb.ch/thomas.staub/microservices/play.identity:1.0 .

docker push git-registry.gibb.ch/thomas.staub/microservices/play.identity:1.0


docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentitySettings__AdminUserPassword=$adminPass --network playinfra_default git-registry.gibb.ch/thomas.staub/microservices/play.identity:1.0



##Projekt zum entwickel ausführen 
im Verzeichnis /src/Play.Identity.Services dotnet run ausführen

Danach
https://localhost:5003/swagger/index.html




https://staub-microservices@dev.azure.com/staub-microservices/Play.Identity/_git/Play.Identity





