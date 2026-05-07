# 📂 SSF.Interop.Mintrabajo.API - Archivos Generados

API para la gestión y descarga de **archivos generados** por el Worker de MinTrabajo.  
Implementada en **.NET Core** con **arquitectura limpia (Clean Architecture)**, usando **CQRS + MediatR** y separación por capas:

- **Domain** → Entidades y lógica de negocio base.
- **Application** → Casos de uso, validaciones y handlers CQRS.
- **Infrastructure** → Servicios externos, almacenamiento de archivos.
- **Persistence** → DbContexts, configuración EF Core y repositorios.
- **API** → Exposición de endpoints REST..

---

## ⚙️ Funcionalidad

### 1. Listar archivos generados
Endpoint para consultar los archivos disponibles, mostrando información relevante como:
- `IdArchivo`
- `NombreArchivo`
- `RutaArchivo`
- `TamanoBytes`
- `CantidadRegistros`
- `Wk_Mes`
- `HashArchivo`
- `FechaCreacion`

### 2. Descargar archivo
Endpoint que permite descargar un archivo generado en formato **Parquet** (u otros soportados), a partir de su `IdArchivo`.

---

## 📌 Endpoints

Base URL:  
https://{host}/api/archivosgenerados

### 🔹 Listar archivos disponibles
```http
GET /api/archivosgenerados

[
  {
    "idArchivo": 1,
    "nombreArchivo": "MinTrabajo_202510_20251010123000.parquet",
    "rutaArchivo": "C:\\EstructurasMintrabajo\\Exportados",
    "tamanoBytes": 102400,
    "cantidadRegistros": 5000,
    "wk_Mes": 202510,
    "hashArchivo": "A1B2C3...",
    "fechaCreacion": "2025-10-10T12:30:00"
  }
]
Descargar archivo por Id
GET /api/archivosgenerados/{id}/download
Respuesta (200 OK):
Devuelve el archivo como application/octet-stream o application/parquet.

🛠️ Tecnologías utilizadas

.NET 9 / .NET 8 (según configuración del repositorio)

Entity Framework Core (SQL Server)

MediatR (CQRS)

Clean Architecture

Swagger / OpenAPI (documentación de endpoints)

🏗️ Estructura del proyecto

SSF.Interop.Mintrabajo.API/
│── SSF.Interop.Mintrabajo.Domain/         # Entidades
│── SSF.Interop.Mintrabajo.Application/    # CQRS (Handlers, Requests, UseCases)
│── SSF.Interop.Mintrabajo.Infrastructure/ # Servicios externos (Storage, Logger)
│── SSF.Interop.Mintrabajo.Persistence/    # DbContexts y repositorios
│── SSF.Interop.Mintrabajo.API/            # Web API (Controllers)

▶️ Ejecución local

Configurar la base de datos en appsettings.json:

"ConnectionStrings": {
  "ProcesoConnection": "Server=.;Database=MinTrabajo;Trusted_Connection=True;TrustServerCertificate=True"
}


dotnet run --project SSF.Interop.Mintrabajo.API