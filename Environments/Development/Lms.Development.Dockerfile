#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt update && apt install ffmpeg -y 
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt update && apt install ffmpeg -y 
WORKDIR /src
COPY ["Services/Fsel.Course/Fsel.Course.Lms.Api/Fsel.Course.Lms.Api.csproj", "Services/Fsel.Course/Fsel.Course.Lms.Api/"]
COPY ["Services/Fsel.Course/Fsel.Course.Application/Fsel.Course.Application.csproj", "Services/Fsel.Course/Fsel.Course.Application/"]
COPY ["Services/Fsel.Course/Fsel.Course.Infrastructure/Fsel.Course.Infrastructure.csproj", "Services/Fsel.Course/Fsel.Course.Infrastructure/"]
COPY ["Services/Fsel.Course/Fsel.Course.Domain/Fsel.Course.Domain.csproj", "Services/Fsel.Course/Fsel.Course.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
COPY ["Services/Shared/Fsel.Core/Fsel.Core.csproj", "Services/Shared/Fsel.Core/"]
COPY ["Services/Shared/Fsel.Common/Fsel.Common.csproj", "Services/Shared/Fsel.Common/"]
RUN dotnet restore "Services/Fsel.Course/Fsel.Course.Lms.Api/Fsel.Course.Lms.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.Course/Fsel.Course.Lms.Api"
ARG CACHEBUST=1
RUN dotnet build "Fsel.Course.Lms.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.Course.Lms.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.Course.Lms.Api.dll"]
