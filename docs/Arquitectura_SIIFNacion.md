# Documento de arquitectura — Interoperabilidad SIIF Nación (SSF)

**Referencia de formato y alcance:** documento institucional *Arquitectura Interoperabilidad MinTrabajo* (FO-COP-002, formato nuevo), usado como plantilla de secciones. El contenido técnico siguiente corresponde al repositorio **SSF.Interop.SIIFNacion** (API REST + Worker + instalador WPF), no al proyecto MinTrabajo/Parquet del PDF de referencia.

**Versión del documento:** alineada al código de la solución `SSF.Interop.SIIFNacion.API.sln` (.NET 9).

---

## 1. Objetivo del documento

Describir la arquitectura técnica, los componentes de software, los patrones aplicados y el mecanismo de interoperabilidad con **SIIF Nación** (Ministerio de Hacienda y Crédito Público), para que los equipos técnicos puedan:

- Comprender la estructura de proyectos (API, Worker, capas compartidas).
- Entender el flujo desde HTTP o el programador del Worker hasta SIIF y la base de datos.
- Operar y evolucionar el sistema con criterios de separación de responsabilidades y trazabilidad.

---

## 2. Alcance

**Incluye:**

- Descripción lógica de la solución: **API REST**, **Worker Service** (Windows Service), **Instalador** (WPF de apoyo a despliegue).
- Arquitectura en capas tipo **Clean Architecture** / cebolla: Domain, Application, Infrastructure, Persistence, presentación (API y Worker).
- Integración HTTP con SIIF (autenticación, CDP, CDP paginado, obligaciones, compromiso presupuestal RP según endpoints expuestos).
- Persistencia y auditoría asociadas a los casos de uso.
- Configuración por `appsettings.json` y variables de entorno estándar de ASP.NET Core.

**Queda fuera del alcance:**

- Aprovisionamiento de servidores, redes y firewalls.
- Gestión de identidades en Active Directory (salvo que se documente en otro entregable).
- Detalle funcional del negocio tributario/presupuestal de SIIF (contratos oficiales del Ministerio).

---

## 3. Introducción

La solución expone capacidades de integración con **SIIF Nación** mediante:

1. **API REST** (`SSF.Interop.SIIFNacion.API`): invocación bajo demanda de consultas y comandos (MediatR), útil para orquestadores externos, pruebas o integraciones puntuales.
2. **Worker** (`SSF.Interop.SIIFNacion.Worker`): ejecución **programada** (cron) de los mismos casos de uso vía **orquestador**, con registro en **Visor de eventos de Windows** (Event Log) en errores de arranque y logging estándar en operación.

Ambos reutilizan las mismas capas **Application**, **Infrastructure** y **Persistence**, garantizando una sola implementación de reglas de aplicación e integración.

---

## 4. Información técnica resumida

| Ítem | Descripción |
|------|-------------|
| Nombre | Interoperabilidad SSF — SIIF Nación |
| Plataforma | .NET 9 |
| Estilo arquitectónico | Clean Architecture (presentación → aplicación → dominio; infraestructura y persistencia en el borde) |
| Patrones destacados | CQRS (queries/commands), MediatR, inyección de dependencias, cliente HTTP tipado para SIIF |
| Integración externa | SIIF Nación (REST, token Bearer con renovación) |
| Base de datos | SQL Server (cadena de proceso; opcional cadena de auditoría) |
| Despliegue API | IIS (recomendado con ASP.NET Core Hosting Bundle) o contenedor (Dockerfile en repo) |
| Despliegue Worker | Servicio de Windows (`dotnet.exe` + `SSF.Interop.SIIFNacion.Worker.dll`) |

---

## 5. Requisitos obligatorios (resumen)

**Software:**

- .NET 9 (SDK para compilar; **ASP.NET Core Runtime** + **Hosting Bundle** para API en IIS; runtime para Worker).
- SQL Server accesible desde API/Worker.
- Windows Server (Worker + Instalador + IIS) o entorno compatible con el perfil de publicación elegido.

**Configuración:**

- `ConnectionStrings:SqlServerProceso` (obligatoria en API y Worker).
- `ConnectionStrings:SqlServerAuditoria` (opcional en Worker: si falta, se reutiliza la de proceso).
- Sección `Siif` (URL base, credenciales o mecanismo acordado, tiempos de token y timeout).

**Seguridad:**

- Conexiones SQL cifradas y certificados según política institucional (`TrustServerCertificate` solo donde proceda).
- Secretos fuera del repositorio en producción (variables de entorno, almacén de secretos, etc.).

---

## 6. Visión general de la solución

| Componente | Responsabilidad principal |
|------------|---------------------------|
| **API** | Exponer endpoints REST bajo `api/siif/...`, traducir JSON a queries/commands MediatR, devolver respuestas JSON. |
| **Worker** | Disparar ciclos según **cron** y ejecutar en paralelo CDP, CDP paginado (YTD) y tres sincronizaciones de obligaciones (vigencias 1, 2 y 3). |
| **Application** | Casos de uso, handlers CQRS, contratos; sin EF ni `HttpClient` directos. |
| **Infrastructure** | Clientes SIIF, opciones, servicios técnicos (HTTP, logging auxiliar, etc.). |
| **Persistence** | DbContext, repositorios, mapeos EF Core. |
| **Domain** | Entidades y modelos de dominio. |
| **Installer** | Asistente WPF: prerrequisitos .NET/IIS, edición de `appsettings.json`, creación de servicio Windows y sitio IIS. |

**Grafo de dependencias (resumen):**

- API → Application, Infrastructure, Persistence  
- Worker → Application, Infrastructure, Persistence  
- Application → Domain  
- Infrastructure → Application  
- Persistence → Application, Domain  

---

## 7. Enfoque arquitectónico

Se adopta **Clean Architecture** sobre .NET 9:

- **Bajo acoplamiento** entre reglas de aplicación y detalles (SQL, SIIF, hosting).
- **Testabilidad** de handlers con dependencias sustituibles.
- **Un solo núcleo** de casos de uso consumido por API y Worker.

La **API** actúa como adaptador HTTP. El **Worker** actúa como adaptador de proceso en segundo plano (`BackgroundService` + `UseWindowsService()` para integración correcta con el SCM de Windows).

---

## 8. Capas y proyectos

### 8.1 Presentación

| Proyecto | Rol |
|----------|-----|
| `SSF.Interop.SIIFNacion.API` | Controladores (`SiifController`, `DefaultController`), modelos de request API, middleware de excepciones, Swagger, health checks. |
| `SSF.Interop.SIIFNacion.Worker` | `Program.cs` (host genérico), `SiifScheduledIntegrationWorker`, `SiifWorkerExecutionOrchestrator`. |
| `SSF.Interop.SIIFNacion.Installer` | Herramienta de despliegue (no forma parte del runtime de integración en producción). |

### 8.2 Aplicación — `SSF.Interop.SIIFNacion.Application`

- Comandos y queries MediatR por feature (carpeta `Features/SIIF/...`).
- Interfaces que Infrastructure y Persistence implementan.
- Servicios de auditoría transversales (`IAuditoriaLogger`, etc.).

No debe depender de detalles de infraestructura concretos (solo de abstracciones).

### 8.3 Dominio — `SSF.Interop.SIIFNacion.Domain`

Entidades y tipos compartidos sin referencias a frameworks de infraestructura.

### 8.4 Infraestructura — `SSF.Interop.SIIFNacion.Infrastructure`

- Implementación de clientes **SIIF** (`HttpClient`, manejo de token, reintentos donde aplique).
- Registro de opciones (`SiifOptions`, sección `Siif`).
- Paquetes opcionales (por ejemplo AWS SDK) según evolución del proyecto.

### 8.5 Persistencia — `SSF.Interop.SIIFNacion.Persistence`

- `DbContext`, configuración Fluent API, repositorios.
- Cadenas de conexión provistas por el host (API o Worker).

### 8.6 Pruebas — `SSF.Interop.SIIFNacion.Application.Tests`

Pruebas unitarias / de aplicación sobre handlers y lógica aislable.

---

## 9. Flujos principales

### 9.1 Flujo API (ejemplo)

1. Cliente HTTP → `POST api/siif/consultar-cdp` con cuerpo JSON.
2. Controlador valida nulidad y mapea a `ConsultarCdpQuery`.
3. MediatR ejecuta el handler en Application.
4. Handler usa `ISiifBudgetService` / repositorios vía interfaces.
5. Respuesta serializada a JSON; errores no controlados pasan por `ExceptionMiddleware`.

### 9.2 Flujo Worker

1. `SiifScheduledIntegrationWorker` parsea `SiifWorker:CronExpression` (Cronos, formato estándar de cinco campos).
2. Calcula la próxima ocurrencia según `TimeZoneId` (si el id no existe en el SO, se usa UTC).
3. Opcionalmente ejecuta un ciclo al arranque si `RunOnStartup` es verdadero.
4. Por cada ciclo, `SiifWorkerExecutionOrchestrator`:
   - Calcula rango **YTD** (año en curso desde 01/01 hasta “hoy” local) para CDP paginado y obligaciones.
   - Lanza en **paralelo** tareas con **scope DI independiente** cada una (DbContext no es thread-safe).
   - Registra inicio/fin de ciclo y de cada flujo en auditoría.

**Nota:** El endpoint API de **compromiso presupuestal RP** existe en el controlador; el orquestador del Worker actual no lo invoca (solo CDP, CDP paginado y obligaciones).

---

## 10. Componentes externos y configuración

- **SIIF:** `Siif:BaseUrl`, `Siif:AuthenticationPath`, credenciales, ventanas de renovación de token y `TimeoutSeconds`.
- **SQL:** `ConnectionStrings:SqlServerProceso` (+ `SqlServerAuditoria` en Worker si aplica).

Detalle de claves: ver `appsettings.json` de API y Worker y la clase `SiifWorkerOptions` (`SectionName = "SiifWorker"`).

---

## 11. Persistencia y auditoría

Los handlers y el orquestador utilizan **`IAuditoriaLogger`** para dejar trazabilidad (puntos de control de ciclo, inicio/fin de flujo, errores). La ubicación física de tablas y el modelo relacional se documentan en el esquema de base de datos corporativo.

---

## 12. Despliegue

- **API en IIS:** sitio apuntando a la carpeta de publicación; application pool **sin** CLR clásico (`managedRuntimeVersion` vacío), típico de ASP.NET Core con ANCM.
- **Worker:** servicio Windows cuyo binPath ejecuta `dotnet.exe` con ruta al DLL del Worker (evita error 1053 al usar `UseWindowsService()` y fija el directorio de trabajo al directorio del ensamblado para leer `appsettings.json`).
- **Instalador:** automatiza comprobaciones, descarga de prerrequisitos y scripts elevados; ver `Manual_Instalador.md`.

---

## 13. Beneficios del enfoque

- Reutilización de casos de uso entre API síncrona y Worker programado.
- Evolución de integraciones SIIF localizada en Infrastructure y contratos de Application.
- Trazabilidad explícita por ciclo (`idCiclo`) y por flujo en auditoría.
- Escalabilidad operativa: ajuste de cron y flags `Enabled` por job sin recompilar (según política de despliegue).

---

## 14. Referencias en código

- Arranque API: `SSF.Interop.SIIFNacion.API/Program.cs`
- Endpoints SIIF: `SSF.Interop.SIIFNacion.API/Controllers/SiifController.cs`
- Worker: `SSF.Interop.SIIFNacion.Worker/Program.cs`, `SiifScheduledIntegrationWorker.cs`, `SiifWorkerExecutionOrchestrator.cs`
- Opciones Worker: `SSF.Interop.SIIFNacion.Worker/SiifWorkerOptions.cs`

---

*Fin del documento de arquitectura.*
