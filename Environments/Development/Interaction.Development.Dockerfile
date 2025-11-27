#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.Interaction/Fsel.Interaction.Api/Fsel.Interaction.Api.csproj", "Services/Fsel.Interaction/Fsel.Interaction.Api/"]
COPY ["Services/Fsel.Interaction/Fsel.Interaction.Application/Fsel.Interaction.Application.csproj", "Services/Fsel.Interaction/Fsel.Interaction.Application/"]
COPY ["Services/Fsel.Interaction/Fsel.Interaction.Infrastructure/Fsel.Interaction.Infrastructure.csproj", "Services/Fsel.Interaction/Fsel.Interaction.Infrastructure/"]
COPY ["Services/Fsel.Interaction/Fsel.Interaction.Domain/Fsel.Interaction.Domain.csproj", "Services/Fsel.Interaction/Fsel.Interaction.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
RUN dotnet restore "Services/Fsel.Interaction/Fsel.Interaction.Api/Fsel.Interaction.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Interaction/Fsel.Interaction.Api"
RUN dotnet build "Fsel.Interaction.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Interaction.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Interaction.Api.dll"]
