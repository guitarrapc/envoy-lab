#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj", "EchoGrpcMagicOnion.Client/"]
COPY ["EchoGrpcMagicOnion.Shared/EchoGrpcMagicOnion.Shared.csproj", "EchoGrpcMagicOnion.Shared/"]
RUN dotnet restore "EchoGrpcMagicOnion.Client/EchoGrpcMagicOnion.Client.csproj"
COPY . .
WORKDIR "/src/EchoGrpcMagicOnion.Client"
RUN dotnet build "EchoGrpcMagicOnion.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EchoGrpcMagicOnion.Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EchoGrpcMagicOnion.Client.dll"]