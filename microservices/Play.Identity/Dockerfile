FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE  80
ENV ASPNETCORE_URLS=http://+:80
#EXPOSE 443 80
#ENV ASPNETCORE_URLS=https://+:443;http://+:80
#ENV ASPNETCORE_URLS="http://+5002;https://+5002" 
# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY ["src/Play.Identity.Contracts/Play.Identity.Contracts.csproj", "src/Play.Identity.Contracts/"]
COPY ["src/Play.Identity.Service/Play.Identity.Service.csproj", "src/Play.Identity.Service/"]
COPY ["src/Play.Identity.Service/nuget.config", "src/Play.Identity.Service/"]

COPY ./src ./src


RUN dotnet restore "src/Play.Identity.Service/Play.Identity.Service.csproj"
WORKDIR "/src/Play.Identity.Service"
RUN dotnet publish "Play.Identity.Service.csproj" -c Release --no-restore -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Play.Identity.Service.dll"]
