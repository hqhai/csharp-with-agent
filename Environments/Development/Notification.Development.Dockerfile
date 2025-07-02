#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Notification/Fsel.Notification.Api/Fsel.Notification.Api.csproj", "Services/Fsel.Notification/Fsel.Notification.Api/"]
COPY ["Services/Fsel.Notification/Fsel.Notification.Application/Fsel.Notification.Application.csproj", "Services/Fsel.Notification/Fsel.Notification.Application/"]
COPY ["Services/Fsel.Notification/Fsel.Notification.Infrastructure/Fsel.Notification.Infrastructure.csproj", "Services/Fsel.Notification/Fsel.Notification.Infrastructure/"]
COPY ["Services/Fsel.Notification/Fsel.Notification.Domain/Fsel.Notification.Domain.csproj", "Services/Fsel.Notification/Fsel.Notification.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
RUN dotnet restore "Services/Fsel.Notification/Fsel.Notification.Api/Fsel.Notification.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Notification/Fsel.Notification.Api"
RUN dotnet build "Fsel.Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Notification.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Notification.Api.dll"]
