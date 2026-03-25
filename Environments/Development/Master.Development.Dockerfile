#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Master/Fsel.Master.Api/Fsel.Master.Api.csproj", "Services/Fsel.Master/Fsel.Master.Api/"]
COPY ["Services/Fsel.Master/Fsel.Master.Application/Fsel.Master.Application.csproj", "Services/Fsel.Master/Fsel.Master.Application/"]
COPY ["Services/Fsel.Master/Fsel.Master.Domain/Fsel.Master.Domain.csproj", "Services/Fsel.Master/Fsel.Master.Domain/"]
COPY ["Services/Fsel.Master/Fsel.Master.Infrastructure/Fsel.Master.Infrastructure.csproj", "Services/Fsel.Master/Fsel.Master.Infrastructure/"]
COPY ["Services/Shared/Fsel.Core/Fsel.Core.csproj", "Services/Shared/Fsel.Core/"]
COPY ["Services/Shared/Fsel.Common/Fsel.Common.csproj", "Services/Shared/Fsel.Common/"]
RUN dotnet restore "Services/Fsel.Master/Fsel.Master.Api/Fsel.Master.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Master/Fsel.Master.Api"
RUN dotnet build "Fsel.Master.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Master.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Master.Api.dll"]