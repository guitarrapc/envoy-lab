#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["GrpcEdsService/GrpcEdsService.csproj", "GrpcEdsService/"]
RUN dotnet restore "GrpcEdsService/GrpcEdsService.csproj"
COPY . .
WORKDIR "/src/GrpcEdsService"
RUN dotnet build "GrpcEdsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GrpcEdsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GrpcEdsService.dll"]