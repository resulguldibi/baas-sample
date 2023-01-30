#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src
COPY ["baas-sample-api/baas-sample-api.csproj", "baas-sample-api/"]
COPY ["job-dispatcher/job-dispatcher.csproj", "job-dispatcher/"]
COPY ["client.cassandra/client.cassandra.csproj", "client.cassandra/"]
COPY ["client.kafka/client.kafka.csproj", "client.kafka/"]
RUN dotnet restore "job-dispatcher/job-dispatcher.csproj"
RUN dotnet restore "client.cassandra/client.cassandra.csproj"
RUN dotnet restore "client.kafka/client.kafka.csproj"
RUN dotnet restore "baas-sample-api/baas-sample-api.csproj"
COPY . .
WORKDIR "/src/baas-sample-api"
RUN dotnet build "baas-sample-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "baas-sample-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "baas-sample-api.dll"]