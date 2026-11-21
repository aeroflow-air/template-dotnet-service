# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/AeroFlow.ServiceDefaults/AeroFlow.ServiceDefaults.csproj src/AeroFlow.ServiceDefaults/
COPY src/AeroFlow.ServiceDefaults/build/ src/AeroFlow.ServiceDefaults/build/
COPY src/AeroFlow.ServiceDefaults/buildTransitive/ src/AeroFlow.ServiceDefaults/buildTransitive/
COPY src/AeroFlow.ServiceTemplate/AeroFlow.ServiceTemplate.csproj src/AeroFlow.ServiceTemplate/

RUN dotnet restore src/AeroFlow.ServiceTemplate/AeroFlow.ServiceTemplate.csproj

COPY src/AeroFlow.ServiceDefaults/ src/AeroFlow.ServiceDefaults/
COPY src/AeroFlow.ServiceTemplate/ src/AeroFlow.ServiceTemplate/

RUN dotnet publish src/AeroFlow.ServiceTemplate/AeroFlow.ServiceTemplate.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AeroFlow.ServiceTemplate.dll"]
