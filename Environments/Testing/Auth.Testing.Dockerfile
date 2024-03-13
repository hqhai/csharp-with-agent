#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Services/Fsel.Identity/Fsel.Identity.Authentication/Fsel.Identity.Authentication.csproj", "Services/Fsel.Identity/Fsel.Identity.Authentication/"]
COPY ["Services/Fsel.Identity/Fsel.Identity.Application/Fsel.Identity.Application.csproj", "Services/Fsel.Identity/Fsel.Identity.Application/"]
COPY ["Services/Fsel.Identity/Fsel.Identity.Infrastructure/Fsel.Identity.Infrastructure.csproj", "Services/Fsel.Identity/Fsel.Identity.Infrastructure/"]
COPY ["Services/Fsel.Identity/Fsel.Identity.Domain/Fsel.Identity.Domain.csproj", "Services/Fsel.Identity/Fsel.Identity.Domain/"]
COPY ["Packages/IdentityServer/AspNetIdentity/src/IdentityServer4.AspNetIdentity.csproj", "Packages/IdentityServer/AspNetIdentity/src/"]
COPY ["Packages/IdentityServer/IdentityServer4/src/IdentityServer4.csproj", "Packages/IdentityServer/IdentityServer4/src/"]
COPY ["Packages/IdentityServer/Storage/src/IdentityServer4.Storage.csproj", "Packages/IdentityServer/Storage/src/"]
COPY ["Packages/IdentityServer/EntityFramework.Storage/src/IdentityServer4.EntityFramework.Storage.csproj", "Packages/IdentityServer/EntityFramework.Storage/src/"]
COPY ["Packages/IdentityServer/EntityFramework/src/IdentityServer4.EntityFramework.csproj", "Packages/IdentityServer/EntityFramework/src/"]
RUN dotnet restore "./Services/Fsel.Identity/Fsel.Identity.Authentication/./Fsel.Identity.Authentication.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Identity/Fsel.Identity.Authentication"
RUN dotnet build "./Fsel.Identity.Authentication.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fsel.Identity.Authentication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Testing
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Identity.Authentication.dll"]