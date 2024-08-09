# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
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

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fsel.Identity.Authentication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Development

# Copy the publish directory
COPY --from=publish /app/publish .

# Create the directory for certificates
RUN mkdir -p /app/Resources/CertificateSSL

# Copy SSL certificate and key into the container
COPY Services/Fsel.Identity/Fsel.Identity.Authentication/Resources/CertificateSSL/certificate_20240809.pem /app/Resources/CertificateSSL/certificate_20240809.pem
COPY Services/Fsel.Identity/Fsel.Identity.Authentication/Resources/CertificateSSL/privatekey_20240809.pem /app/Resources/CertificateSSL/privatekey_20240809.pem

# Set environment variables for SSL
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/Resources/CertificateSSL/certificate_20240809.pem
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/app/Resources/CertificateSSL/privatekey_20240809.pem

# Start the application
ENTRYPOINT ["dotnet", "Fsel.Identity.Authentication.dll"]
