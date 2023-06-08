#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Training/Fsel.Training.Api/Fsel.Training.Api.csproj", "Services/Fsel.Training/Fsel.Training.Api/"]
COPY ["Services/Fsel.Training/Fsel.Training.Application/Fsel.Training.Application.csproj", "Services/Fsel.Training/Fsel.Training.Application/"]
COPY ["Services/Fsel.Training/Fsel.Training.Infrastructure/Fsel.Training.Infrastructure.csproj", "Services/Fsel.Training/Fsel.Training.Infrastructure/"]
COPY ["Services/Fsel.Training/Fsel.Training.Domain/Fsel.Training.Domain.csproj", "Services/Fsel.Training/Fsel.Training.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
COPY ["Services/Shared/Fsel.Core/Fsel.Core.csproj", "Services/Shared/Fsel.Core/"]
COPY ["Services/Shared/Fsel.Common/Fsel.Common.csproj", "Services/Shared/Fsel.Common/"]
RUN dotnet restore "Services/Fsel.Training/Fsel.Training.Api/Fsel.Training.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Training/Fsel.Training.Api"
RUN dotnet build "Fsel.Training.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Training.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Training.Api.dll"]
