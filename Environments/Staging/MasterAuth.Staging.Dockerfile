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
COPY ["Services/Fsel.Master.Identity/Fsel.Master.Identity.Authentication/Fsel.Master.Identity.Authentication.csproj", "Services/Fsel.Master.Identity/Fsel.Master.Identity.Authentication/"]
COPY ["Services/Fsel.Master.Identity/Fsel.Master.Identity.Application/Fsel.Master.Identity.Application.csproj", "Services/Fsel.Master.Identity/Fsel.Master.Identity.Application/"]
COPY ["Services/Fsel.Master.Identity/Fsel.Master.Identity.Infrastructure/Fsel.Master.Identity.Infrastructure.csproj", "Services/Fsel.Master.Identity/Fsel.Master.Identity.Infrastructure/"]
COPY ["Services/Fsel.Master.Identity/Fsel.Master.Identity.Domain/Fsel.Master.Identity.Domain.csproj", "Services/Fsel.Master.Identity/Fsel.Master.Identity.Domain/"]
RUN dotnet restore "./Services/Fsel.Master.Identity/Fsel.Master.Identity.Authentication/./Fsel.Master.Identity.Authentication.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Master.Identity/Fsel.Master.Identity.Authentication"
RUN dotnet build "./Fsel.Master.Identity.Authentication.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fsel.Master.Identity.Authentication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Staging

# Copy the publish directory
COPY --from=publish /app/publish .

# Start the application
ENTRYPOINT ["dotnet", "Fsel.Master.Identity.Authentication.dll"]
