#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Api/Fsel.Cms.PlanetDefender.Api.csproj", "Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Api/"]
COPY ["Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Application/Fsel.Cms.PlanetDefender.Application.csproj", "Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Application/"]
COPY ["Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Infrastructure/Fsel.Cms.PlanetDefender.Infrastructure.csproj", "Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Infrastructure/"]
COPY ["Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Domain/Fsel.Cms.PlanetDefender.Domain.csproj", "Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
COPY ["Services/Shared/Fsel.Core/Fsel.Core.csproj", "Services/Shared/Fsel.Core/"]
COPY ["Services/Shared/Fsel.Common/Fsel.Common.csproj", "Services/Shared/Fsel.Common/"]
RUN dotnet restore "Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Api/Fsel.Cms.PlanetDefender.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Cms.PlanetDefender/Fsel.Cms.PlanetDefender.Api"
RUN dotnet build "Fsel.Cms.PlanetDefender.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Cms.PlanetDefender.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Cms.PlanetDefender.Api.dll"]
