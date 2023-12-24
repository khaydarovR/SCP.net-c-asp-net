#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SCP.Api/SCP.Api.csproj", "SCP.Api/"]
COPY ["SCP.Application/SCP.Application.csproj", "SCP.Application/"]
COPY ["SCP.DAL/SCP.DAL.csproj", "SCP.DAL/"]
COPY ["SCP.Domain/SCP.Domain.csproj", "SCP.Domain/"]
RUN dotnet restore "./SCP.Api/./SCP.Api.csproj"
COPY . .
WORKDIR "/src/SCP.Api"
RUN dotnet build "./SCP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SCP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SCP.Api.dll"]