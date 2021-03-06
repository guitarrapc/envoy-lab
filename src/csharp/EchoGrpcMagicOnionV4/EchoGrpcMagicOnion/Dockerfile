#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
RUN GRPC_HEALTH_PROBE_VERSION=v0.3.4 && \
    curl -sL -o /bin/grpc_health_probe https://github.com/grpc-ecosystem/grpc-health-probe/releases/download/${GRPC_HEALTH_PROBE_VERSION}/grpc_health_probe-linux-amd64 && \
    chmod +x /bin/grpc_health_probe

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["EchoGrpcMagicOnion/EchoGrpcMagicOnion.csproj", "EchoGrpcMagicOnion/"]
COPY ["EchoGrpcMagicOnion.Shared/EchoGrpcMagicOnion.Shared.csproj", "EchoGrpcMagicOnion.Shared/"]
RUN dotnet restore "EchoGrpcMagicOnion/EchoGrpcMagicOnion.csproj"
COPY . .
WORKDIR "/src/EchoGrpcMagicOnion"
RUN dotnet build "EchoGrpcMagicOnion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EchoGrpcMagicOnion.csproj" -c Release -o /app/publish
WORKDIR "/src/EchoGrpcMagicOnion.Client"
RUN dotnet publish "EchoGrpcMagicOnion.Client.csproj" -r linux-x64 -p:PublishSingleFile=true --self-contained=false -p:PublishReadyToRun=true -c Release -o /app/client

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/client/EchoGrpcMagicOnion.Client /usr/local/bin/EchoGrpcMagicOnion.Client
RUN chmod +x /usr/local/bin/EchoGrpcMagicOnion.Client
ENTRYPOINT ["dotnet", "EchoGrpcMagicOnion.dll"]