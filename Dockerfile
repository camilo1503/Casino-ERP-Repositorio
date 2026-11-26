# PitBoss ERP · imagen de la API con el front (HU-09, taller ejercicio 1)
# La usan Docker Compose (local), Kubernetes (k8s/) y Vercel (Container Runtime).

# Etapa 1: compilar y publicar con el SDK de .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primero solo los .csproj, para que la restauración quede en caché mientras no cambien las dependencias
COPY src/CasinoErp.Dominio/CasinoErp.Dominio.csproj src/CasinoErp.Dominio/
COPY src/CasinoErp.Infraestructura/CasinoErp.Infraestructura.csproj src/CasinoErp.Infraestructura/
COPY src/CasinoErp.Api/CasinoErp.Api.csproj src/CasinoErp.Api/
RUN dotnet restore src/CasinoErp.Api/CasinoErp.Api.csproj

COPY src/ src/
RUN dotnet publish src/CasinoErp.Api/CasinoErp.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Etapa 2: solo el runtime de ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Carpeta de la base SQLite, escribible por el usuario sin privilegios ($APP_UID lo define la imagen base)
RUN mkdir -p /app/datos && chown "$APP_UID" /app/datos

# Kestrel escucha en 0.0.0.0:8080. No se define ASPNETCORE_URLS porque anularía ASPNETCORE_HTTP_PORTS.
# PORT=8080 es el puerto al que Vercel envía el tráfico (debe coincidir con la variable PORT del proyecto).
ENV ASPNETCORE_HTTP_PORTS=8080 \
    PORT=8080 \
    DOTNET_EnableDiagnostics=0 \
    ConnectionStrings__CasinoErp="Data Source=/app/datos/casino-erp.db"

# APP_UID es numérico (1654) en las imágenes oficiales de .NET 8; hadolint no puede resolver la variable.
# hadolint ignore=DL3066
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CasinoErp.Api.dll"]
