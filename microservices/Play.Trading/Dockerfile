FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE  80
ENV ASPNETCORE_URLS=http://+:80


# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

COPY ["src/Play.Trading.Service/Play.Trading.Service.csproj", "src/Play.Trading.Service/"]
COPY ["src/Play.Trading.Service/nuget.config", "src/Play.Trading.Service/"]

COPY ./src ./src

RUN dotnet restore "src/Play.Trading.Service/Play.Trading.Service.csproj"
WORKDIR "/src/Play.Trading.Service"

RUN dotnet publish "Play.Trading.Service.csproj" -c Release --no-restore -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Play.Trading.Service.dll"]
