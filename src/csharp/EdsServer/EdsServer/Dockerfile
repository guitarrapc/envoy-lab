#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["EdsServer/EdsServer.csproj", "EdsServer/"]
RUN dotnet restore "EdsServer/EdsServer.csproj"
COPY . .
WORKDIR "/src/EdsServer"
RUN dotnet build "EdsServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EdsServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EdsServer.dll"]