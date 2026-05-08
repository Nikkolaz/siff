# Visión general — SSF.Interop.SIIFNacion.API

Documento de síntesis para onboarding y contexto antes de modificar el código.  
**Stack:** ASP.NET Core 9, MediatR, EF Core (SQL Server), integración HTTP con SIIF (MinHacienda).

**Versión Word (diagramas incrustados):** [VISION-GENERAL-PROYECTO.docx](./VISION-GENERAL-PROYECTO.docx). Para regenerarla: `py -3 docs/generate_vision_docx.py` (requiere `python-docx` y `matplotlib`).

---

## 1. Qué es esta aplicación

API REST de **interoperabilidad con SIIF Nación**: expone endpoints que orquestan llamadas al sistema SIIF y persisten resultados en **SQL Server** (base GESTORDOC / tablas relacionadas). Los casos de uso se implementan con **MediatR** (comandos y consultas con handlers), en una estructura de capas tipo **Clean Architecture** ligera.

---

## 2. Proyectos de la solución


| Proyecto                                  | Rol                                                                                                         |
| ----------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| **SSF.Interop.SIIFNacion.API**            | Host web: controladores, middleware, Swagger/OpenAPI, health checks.                                        |
| **SSF.Interop.SIIFNacion.Application**    | Casos de uso: MediatR, DTOs, contratos (interfaces), AutoMapper, FluentValidation. Depende de **Domain**.   |
| **SSF.Interop.SIIFNacion.Domain**         | Entidades y modelos de dominio (CDP, obligaciones SIIF, piezas de auditoría/cola heredadas).                |
| **SSF.Interop.SIIFNacion.Infrastructure** | Clientes HTTP hacia SIIF, opciones, token y caché, logging, red (`WebClientWrapper`), almacenamiento local. |
| **SSF.Interop.SIIFNacion.Persistence**    | EF Core, `DbContext`, repositorios que implementan contratos de Application.                                |


En el `.sln` existen carpetas lógicas (**src → API / Core / Infrastructure**). La carpeta **Test** no incluye hoy un proyecto de pruebas referenciado.

**Grafo de dependencias (resumen):**

- API → Application, Infrastructure, Persistence  
- Application → Domain  
- Infrastructure → Application  
- Persistence → Application, Domain

---

## 3. Punto de entrada y arranque

- **Archivo:** `SSF.Interop.SIIFNacion.API/Program.cs`
- **Flujo:** `WebApplication.CreateBuilder` → registro de servicios (`ConfigureServices`) → `Build` → pipeline HTTP → `Run()`.

**Registro de servicios (extensiones):**

- `ConfigureApplicationServices()` — Application (MediatR, AutoMapper).
- `ConfigureInfrastructureServices(configuration)` — Infrastructure (SIIF, logging, etc.).
- `ConfigurePresistenceServices(auditoria, proceso)` — Persistence (EF + repositorios).  
*Nota:* el código lee dos cadenas de conexión; el registro actual de contextos usa la cadena de **proceso**; el parámetro de auditoría puede quedar sin uso en ese método.

**Pipeline HTTP (orden aproximado):** HTTPS, autorización ASP.NET, `MapControllers`, Swagger/SwaggerUI, `ExceptionMiddleware`, health checks.

**Detalles a tener en cuenta:** hay `AddControllers` duplicado (cuerpo principal y `ConfigureServices`). CORS se registra en servicios pero conviene verificar si `UseCors` está en el pipeline si se necesita CORS real.

---

## 4. Arquitectura por capas / módulos

- **API:** Traduce peticiones HTTP a **Commands/Queries** de Application y devuelve respuestas; validación mínima del contrato HTTP en controladores.
- **Application:** Un **handler MediatR** por caso de uso; orquesta **interfaces** (`ISiifBudgetService`, `ICdpRepository`, `IObligacionApoRepository`, etc.), sin depender de EF ni de `HttpClient` concretos.
- **Domain:** Entidades y tipos compartidos; parte del código usa namespaces `**SSF.Interop.SIIFNacion.Service.`*** (herencia de otro servicio o copia), lo que no coincide con el nombre del ensamblado `SSF.Interop.SIIFNacion.Domain`.
- **Infrastructure:** Implementa integración **SIIF** (`HttpClient` + `DelegatingHandler` para Bearer y reintento en 401), **proveedor de token** con caché en memoria, logging compuesto.
- **Persistence:** **EF Core** + SQL Server; repositorios concretos.

---

## 5. Acceso a datos

- Tecnología: **Entity Framework Core** + **Microsoft.EntityFrameworkCore.SqlServer**.
- Contextos relevantes: `GestordocDbContext`, `DbContextCdp` (según registro en Persistence).
- Repositorios registrados (ejemplos): `ICdpRepository`, `ICdpPaginadoRepository`, `IObligacionApoRepository`.
- Configuración: sección `**ConnectionStrings`** en `appsettings` (p. ej. `SqlServerProceso`). En `Program` también se referencia `SqlServerAuditoria` al construir el par de cadenas.

---

## 6. Autenticación y seguridad

**Hacia los consumidores de la API**

- No aparece un esquema típico de protección de API (p. ej. JWT + `[Authorize]`) en el análisis realizado; `launchSettings` sugiere acceso anónimo al host.

**Hacia SIIF (saliente)**

- Credenciales en configuración (`Siif:User`, `Siif:Password`).
- `SiifAuthenticationService` obtiene token (JSON o XML según respuesta).
- `SiifTokenProvider` cachea el token en **memoria** con ventana de renovación (`TokenRenewalSafetyWindowSeconds`).
- `SiifAuthDelegatingHandler` añade **Authorization: Bearer**.
- `SiifRetryOnUnauthorizedHandler` invalida token y reintenta ante 401.

**Riesgo:** secretos en archivos de configuración si no se externalizan (User Secrets, variables de entorno, AWS Parameter Store/Secrets Manager — hay referencias NuGet en Infrastructure).

---

## 7. Integraciones externas

- **SIIF (MinHacienda):** `Siif:BaseUrl` + rutas; servicios tipados `ISiifAuthenticationService`, `ISiifBudgetService`, `ISiifCatalogService` con implementaciones en `Infrastructure/Services/External/SIIF`.
- **WebClientWrapper:** cliente HTTP genérico (patrón distinto al `IHttpClientFactory` usado para SIIF).
- **AWS SDK** (Infrastructure): presente en el proyecto; conviene confirmar uso real en código activo.

---

## 8. Configuración crítica

Archivos: `appsettings.json`, `appsettings.Development.json`, `appsettings.Qa.json`.


| Área           | Claves / secciones                                                                                                                          |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Entorno lógico | `Environment`                                                                                                                               |
| SIIF           | `Siif` — `BaseUrl`, `AuthenticationPath`, `User`, `Password`, `TokenExpirationSeconds`, `TokenRenewalSafetyWindowSeconds`, `TimeoutSeconds` |
| Base de datos  | `ConnectionStrings:SqlServerProceso` (y en código `SqlServerAuditoria`)                                                                     |
| Otros          | `WorkerSettings:Convenio`                                                                                                                   |


Definición fuerte de opciones SIIF: `Infrastructure/Options/SiifOptions.cs` (`SectionName = "Siif"`).

---

## 9. Flujos de negocio principales

1. **Consultar CDP** — `POST api/siif/consultar-cdp` → `ConsultarCdpQuery` → SIIF + persistencia/actualización según respuesta.
2. **Consultar CDP paginada** — `POST api/siif/consultar-cdp-paginada` → `ConsultarCdpPaginadaQuery` + repositorio paginado.
3. **Sincronizar lista de obligaciones** — `POST api/siif/sincronizar-lista-obligaciones` → comando MediatR → paginación en SIIF (máx. 50 por página) → persistencia en tabla **DYNTBLSIIFOBLIGAPO**.
4. **Salud y raíz** — Health checks (`/healthcheck`, UI), `GET /` en `DefaultController`.

---

## 10. Archivos clave para entender el sistema

### Arranque y composición

- `SSF.Interop.SIIFNacion.API/Program.cs`
- `SSF.Interop.SIIFNacion.Application/ApplicationServiceRegistration.cs`
- `SSF.Interop.SIIFNacion.Infrastructure/InfrastructureServicesRegistration.cs`
- `SSF.Interop.SIIFNacion.Persistence/PersistenceServicesRegistration.cs`

### API y errores

- `SSF.Interop.SIIFNacion.API/Controllers/SiifController.cs`
- `SSF.Interop.SIIFNacion.API/Controllers/DefaultController.cs`
- `SSF.Interop.SIIFNacion.API/Middleware/ExceptionMiddleware.cs`

### Casos de uso SIIF

- `Application/Features/SIIF/Requests/Queries/ConsultarCdpQueryHandler.cs`
- `Application/Features/SIIF/Requests/Queries/ConsultarCdpPaginadaQueryHandler.cs`
- `Application/Features/SIIF/Requests/Commands/SincronizarListaObligaciones/SincronizarListaObligacionesCommandHandler.cs`

### Integración SIIF

- `Infrastructure/Services/External/SIIF/SiifAuthenticationService.cs`
- `Infrastructure/Services/External/SIIF/SiifTokenProvider.cs`
- `Infrastructure/Services/External/SIIF/SiifAuthDelegatingHandler.cs`
- `Infrastructure/Services/External/Handlers/SiifRetryOnUnauthorizedHandler.cs`
- `Infrastructure/Services/External/SIIF/SiifBudgetService.cs`
- `Infrastructure/Options/SiifOptions.cs`

### Persistencia

- `Persistence/DBContext/GestordocDbContext.cs`
- `Persistence/Repositories/GenericRepositories/CdpRepository.cs`
- `Persistence/Repositories/GenericRepositories/CdpPaginadoRepository.cs`
- `Persistence/Repositories/GenericRepositories/ObligacionApoRepository.cs`

### Contratos (Application)

- `Application/Common/Interfaces/ExternalServices/SIIF/*.cs`
- `Application/Contracts/Persistence/ICdpRepository.cs`, `ICdpPaginadoRepository.cs`, `IObligacionApoRepository.cs`

---

## 11. Riesgos y olores típicos de legacy / deuda


| Tema                                               | Descripción breve                                                                                                                                                                    |
| -------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Namespaces `Service.`* en proyectos `SIIFNacion.`* | Sugiere código portado de otro servicio; confunde búsquedas y límites de módulo.                                                                                                     |
| Contratos no registrados                           | Interfaces de auditoría/ETL, `IUnitOfWork`, repositorios genéricos — posible código muerto o preparado para otro ejecutable.                                                         |
| `WebClientWrapper` grande                          | Mucha responsabilidad en un tipo; uso de `HttpClient` sin factory en parte del código.                                                                                               |
| Handlers extensos                                  | `SincronizarListaObligacionesCommandHandler` concentra paginación, parseo y persistencia.                                                                                            |
| `SemaphoreSlim` estático en `SiifTokenProvider`    | Coordinación global en el proceso; coherente en una instancia, pero patrón “global”.                                                                                                 |
| Logging / DI                                       | `DatabaseLogger` con `IEnumerable<ICustomLogger>` y `ICustomLogger` → `CompositeLogger` que incluye `DatabaseLogger`: riesgo de ciclo o delegación recursiva según resolución de DI. |
| Orden del pipeline                                 | `ExceptionMiddleware` después de `MapControllers` puede no capturar excepciones de controladores como se espera en el patrón habitual.                                               |
| Secretos en configuración                          | Usuario/contraseña SIIF y SQL en JSON — revisar política de secretos por entorno.                                                                                                    |
| Duplicados / CORS                                  | `AddControllers` duplicado; CORS registrado pero verificar `UseCors` en pipeline.                                                                                                    |


---

## 12. Lista de lectura priorizada (onboarding)

**Nivel 1 — mínimo viable**

1. `Program.cs`
2. `ApplicationServiceRegistration.cs`
3. `InfrastructureServicesRegistration.cs`
4. `PersistenceServicesRegistration.cs`
5. `SiifController.cs`
6. `appsettings.json` (+ Development / Qa según entorno)

**Nivel 2 — flujos SIIF + datos**

1. `ConsultarCdpQueryHandler.cs`
2. `ConsultarCdpPaginadaQueryHandler.cs`
3. `SincronizarListaObligacionesCommandHandler.cs`
4. `SiifBudgetService.cs`
5. `SiifAuthenticationService.cs` + `SiifTokenProvider.cs` + handlers HTTP
6. `SiifOptions.cs`
7. `GestordocDbContext.cs` + repositorios CDP y obligaciones

**Nivel 3 — transversal y legado**

1. `ExceptionMiddleware.cs`
2. `ISiifBudgetService.cs` / `ISiifCatalogService.cs`
3. `WebClientWrapper.cs`
4. Loggers en `Infrastructure/Logging/`
5. Archivos bajo `Domain/Auditoria/` y contratos `Service.Application.Contracts.Persistence` para clasificar código activo vs heredado.

---

## 13. Diagrama de flujo de una petición (resumen)

```
HTTP → Pipeline (HTTPS, Auth ASP.NET, Routing)
     → Controller (DTO → Command/Query)
     → IMediator.Send
     → Handler (Application)
           → ISiif* (Infrastructure / HttpClient)
           → I*Repository (Persistence / EF)
     → IActionResult (JSON)
```

---

*Documento generado como referencia de arquitectura y contexto; actualizar si cambian registro DI, endpoints o integraciones.*