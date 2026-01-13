#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN apt update \
    && apt install ffmpeg -y \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Api/Fsel.ExamPractice.Lms.Api.csproj", "Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Api/"]
COPY ["Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Application/Fsel.ExamPractice.Lms.Application.csproj", "Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Application/"]
COPY ["Services/Fsel.ExamPractice/Fsel.ExamPractice.Infrastructure/Fsel.ExamPractice.Infrastructure.csproj", "Services/Fsel.ExamPractice/Fsel.ExamPractice.Infrastructure/"]
COPY ["Services/Fsel.ExamPractice/Fsel.ExamPractice.Domain/Fsel.ExamPractice.Domain.csproj", "Services/Fsel.ExamPractice/Fsel.ExamPractice.Domain/"]
COPY ["Services/Shared/Fsel.Shared/Fsel.Shared.csproj", "Services/Shared/Fsel.Shared/"]
RUN dotnet restore "Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Api/Fsel.ExamPractice.Lms.Api.csproj"
COPY . .
WORKDIR "/src/Services/Fsel.ExamPractice/Fsel.ExamPractice.Lms.Api"
ARG CACHEBUST=1
RUN dotnet build "Fsel.ExamPractice.Lms.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fsel.ExamPractice.Lms.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Testing
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fsel.ExamPractice.Lms.Api.dll"]
