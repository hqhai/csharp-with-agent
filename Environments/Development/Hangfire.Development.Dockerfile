#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Hangfire/Fsel.Hangfire.Host/Fsel.Hangfire.Host.csproj", "Services/Fsel.Hangfire/Fsel.Hangfire.Host/"]
COPY ["Services/Fsel.Hangfire/Fsel.Hangfire.Application/Fsel.Hangfire.Application.csproj", "Services/Fsel.Hangfire/Fsel.Hangfire.Application/"]
COPY ["Services/Fsel.Realtime/Fsel.Realtime.Infrastructure/Fsel.Realtime.Infrastructure.csproj", "Services/Fsel.Realtime/Fsel.Realtime.Infrastructure/"]
COPY ["Services/Fsel.Realtime/Fsel.Realtime.Domain/Fsel.Realtime.Domain.csproj", "Services/Fsel.Realtime/Fsel.Realtime.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
RUN dotnet restore "Services/Fsel.Hangfire/Fsel.Hangfire.Host/Fsel.Hangfire.Host.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Hangfire/Fsel.Hangfire.Host"
RUN dotnet build "Fsel.Hangfire.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Hangfire.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Hangfire.Host.dll"]