#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.System/Fsel.System.Api/Fsel.System.Api.csproj", "Services/Fsel.System/Fsel.System.Api/"]
COPY ["Services/Fsel.System/Fsel.System.Application/Fsel.System.Application.csproj", "Services/Fsel.System/Fsel.System.Application/"]
COPY ["Services/Fsel.System/Fsel.System.Infrastructure/Fsel.System.Infrastructure.csproj", "Services/Fsel.System/Fsel.System.Infrastructure/"]
COPY ["Services/Fsel.System/Fsel.System.Domain/Fsel.System.Domain.csproj", "Services/Fsel.System/Fsel.System.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
COPY ["Services/Shared/Fsel.Core/Fsel.Core.csproj", "Services/Shared/Fsel.Core/"]
COPY ["Services/Shared/Fsel.Common/Fsel.Common.csproj", "Services/Shared/Fsel.Common/"]
RUN dotnet restore "Services/Fsel.System/Fsel.System.Api/Fsel.System.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.System/Fsel.System.Api"
RUN dotnet build "Fsel.System.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.System.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.System.Api.dll"]
