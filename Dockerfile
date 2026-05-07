# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SSF.Interop.SIIFNacion.API/SSF.Interop.SIIFNacion.API.csproj", "SSF.Interop.SIIFNacion.API/"]
COPY ["SSF.Interop.SIIFNacion.Application/SSF.Interop.SIIFNacion.Application.csproj", "SSF.Interop.SIIFNacion.Application/"]
COPY ["SSF.Interop.SIIFNacion.Domain/SSF.Interop.SIIFNacion.Domain.csproj", "SSF.Interop.SIIFNacion.Domain/"]
COPY ["SSF.Interop.SIIFNacion.Infrastructure/SSF.Interop.SIIFNacion.Infrastructure.csproj", "SSF.Interop.SIIFNacion.Infrastructure/"]
COPY ["SSF.Interop.SIIFNacion.Persistence/SSF.Interop.SIIFNacion.Persistence.csproj", "SSF.Interop.SIIFNacion.Persistence/"]
RUN dotnet restore "SSF.Interop.SIIFNacion.API/SSF.Interop.SIIFNacion.API.csproj"
COPY . .
WORKDIR "/src/SSF.Interop.SIIFNacion.API"
RUN dotnet build "SSF.Interop.SIIFNacion.API.csproj" -c Release -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
RUN dotnet publish "SSF.Interop.SIIFNacion.API.csproj"  -c Release -o /app/publish

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER app

ENV TZ=America/Bogota
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
  CMD curl -fsS "http://127.0.0.1/api/health/live" -o /dev/null || exit 1

ENTRYPOINT ["dotnet", "SSF.Interop.SIIFNacion.API.dll"]

