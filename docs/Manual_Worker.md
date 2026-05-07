# Manual de uso — Worker SSF.Interop.SIIFNacion.Worker

## 1. Propósito

El **Worker** es un **servicio de Windows** (también ejecutable por consola para pruebas) que ejecuta de forma **periódica** los mismos casos de uso SIIF que la API, sin necesidad de invocaciones HTTP externas.

Características principales:

- Programación mediante **expresión cron** (cinco campos: minuto, hora, día del mes, mes, día de semana) interpretada con la librería **Cronos** (`CronFormat.Standard`).
- Zona horaria configurable (`TimeZoneId`) para calcular el “hoy” local del año en curso.
- **Paralelismo seguro:** cada flujo abre su propio **scope** de inyección de dependencias (nuevo `DbContext` y pipeline MediatR por tarea).
- Registro de **ciclo** en auditoría con un **`idCiclo`** (GUID) común a todos los flujos de una misma corrida.

---

## 2. Arranque y modo servicio

- **Archivo de entrada:** `Program.cs` registra `AddWindowsService()`, logging a **Visor de eventos** en Windows, carga de configuración desde el directorio del ensamblado (corrige el problema de directorio de trabajo en `System32` cuando se usa `sc create` con `dotnet.exe`).
- **Host:** `Host.CreateApplicationBuilder` con `ContentRootPath` = `AppContext.BaseDirectory`.

Si el proceso falla antes de construir el host, se intenta escribir la excepción en el registro de eventos (`WorkerEventLogDiagnostics`).

---

## 3. Sección de configuración `SiifWorker`

Toda la programación y los parámetros de negocio del Worker viven bajo la sección **`SiifWorker`** en `appsettings.json` (clase `SiifWorkerOptions`, constante `SectionName = "SiifWorker"`).

### 3.1 Propiedades generales

| Clave | Tipo | Descripción |
|--------|------|-------------|
| `Enabled` | bool | Si es `false`, el worker **no** programa ejecuciones (sale tras registrar en log). |
| `RunOnStartup` | bool | Si es `true`, ejecuta **un ciclo completo** al iniciar el proceso, antes de entrar al bucle del cron. |
| `CronExpression` | string | Expresión cron estándar. Ejemplo: `0 2 * * *` = todos los días a las 02:00 en la zona de `TimeZoneId`. |
| `TimeZoneId` | string | Id de zona del sistema operativo. En Windows use ids como `SA Pacific Standard Time`; en Linux suelen usarse ids IANA como `America/Bogota`. Si el id no existe, se usa **UTC**. |

### 3.2 Jobs configurables

#### A) `Cdp` — Consultar CDP puntual

| Clave | Descripción |
|--------|-------------|
| `Enabled` | Activa o desactiva este flujo. |
| `Headers` | Objeto `codPciHeader`, `loginUsuarioSiifHeader`, `consecutivoHeader`, `hashHeader`. |
| `IdentificacionPCI` | Identificador PCI del CDP. |
| `ConsecutivoCDP` | Consecutivo del CDP. |

Si `Enabled` es `true` pero faltan `IdentificacionPCI` o `ConsecutivoCDP`, el flujo se **omite** (advertencia en log).

#### B) `CdpPaginado` — Consultar CDP paginada

| Clave | Descripción |
|--------|-------------|
| `Enabled` | Activa o desactiva este flujo. |
| `Headers` | Mismas cabeceras que en la API. |
| `PCIConsulta`, `PCISubUnidades`, `TipoGasto`, `Rango` | Filtros del listado. |

**Fechas:** el orquestador **no** lee `fechaRegistroIni`/`Fin` del JSON. Calcula automáticamente el rango **YTD** (desde el 1 de enero del año calendario actual hasta “hoy” **local** según `TimeZoneId`) y lo envía al query como cadenas ISO.

**Paginación interna:** el orquestador fija `Page = 1` y `Size = 50` al comando; la iteración completa de páginas la resuelve el handler de aplicación.

#### C) `Obligaciones` — Sincronizar lista de obligaciones

| Clave | Descripción |
|--------|-------------|
| `Enabled` | Activa o desactiva el bloque de obligaciones. |
| `Headers` | Cabeceras SIIF. |
| `CodPCI`, `TipoGasto`, `Rango`, `DetalleUsosPresupuestales` | Parámetros del comando hacia SIIF. |

**Fechas:** igual que CDP paginado, se usa **YTD** local (inicio y fin de año hasta hoy).

**Vigencias:** el Worker **no** usa un solo `Vigencia` del JSON. Lanza **tres** ejecuciones en paralelo con:

- `Vigencia = "1"` — etiqueta interna `ObligacionesPaginadaActual`
- `Vigencia = "2"` — `ObligacionesPaginadaReservas`
- `Vigencia = "3"` — `PaginadaCXP`

Cada una es un `SincronizarListaObligacionesCommand` independiente con el mismo rango de fechas y parámetros base.

**Nota:** El endpoint API de **compromiso presupuestal RP** no está incluido en el orquestador del Worker; solo CDP, CDP paginado y obligaciones.

---

## 4. Otras secciones de `appsettings.json`

El Worker reutiliza la misma configuración SIIF y SQL que la API:

- **`Siif`:** URL, autenticación, usuario, contraseña, tiempos de token, timeout.
- **`ConnectionStrings:SqlServerProceso`:** obligatoria (`Connection` o cadena plana según convención del proyecto).
- **`ConnectionStrings:SqlServerAuditoria`:** opcional; si está vacía o ausente, el Worker usa la misma cadena que proceso (ver `BuildSqlPair` en `Program.cs`).

---

## 5. Ciclo de ejecución (resumen)

1. Si `SiifWorker:Enabled` es falso → termina sin trabajo.
2. Se parsea el cron; si es inválido → **fallo crítico** al arranque.
3. Si `RunOnStartup` → ejecuta un ciclo.
4. Bucle: calcula la próxima ocurrencia con `TimeZoneId`, espera hasta esa hora, ejecuta **un ciclo**.
5. Un ciclo:
   - Genera `idCiclo` (GUID sin guiones).
   - Escribe auditoría de **inicio de ciclo**.
   - Encola tareas paralelas: CDP (si aplica), CDP paginado, y tres obligaciones.
   - `Task.WhenAll` espera a todas.
   - Escribe auditoría de **fin de ciclo** con tiempos y estado **Exitoso** o **Parcial** si algún flujo falló.

Cada flujo registra inicio, fin o error en auditoría con identificadores derivados de `idCiclo` (por ejemplo `{idCiclo}:cdp`).

---

## 6. Instalación como servicio Windows

1. Publique el proyecto Worker (`dotnet publish`).
2. Use el **Instalador** del repositorio (pestaña **Servicio Windows**) o cree manualmente el servicio apuntando a:

   ```text
   "C:\Program Files\dotnet\dotnet.exe" "R:\uta\SSF.Interop.SIIFNacion.Worker.dll"
   ```

3. Asegúrese de que `appsettings.json` esté en la **misma carpeta** que el DLL.

Cuenta de servicio: debe tener permisos de red hacia SQL Server y SIIF.

---

## 7. Operación diaria

| Tarea | Acción |
|-------|--------|
| Cambiar horario | Edite `CronExpression` y reinicie el servicio. |
| Pausar integración | Ponga `SiifWorker:Enabled` en `false` y reinicie (o use recarga si habilita `IOptionsMonitor` con proveedor que recargue archivo). |
| Prueba puntual | Ponga `RunOnStartup` en `true` temporalmente o invoque los mismos flujos vía **API REST**. |
| Diagnóstico | Visor de eventos + trazas en base de datos de auditoría (`IAuditoriaLogger`). |

---

## 8. Ejemplo de fragmento `SiifWorker`

```json
"SiifWorker": {
  "Enabled": true,
  "RunOnStartup": false,
  "CronExpression": "0 2 * * *",
  "TimeZoneId": "America/Bogota",
  "Cdp": {
    "Enabled": true,
    "Headers": {
      "CodPciHeader": "13-01-01",
      "LoginUsuarioSiifHeader": "usuario",
      "ConsecutivoHeader": "1",
      "HashHeader": null
    },
    "IdentificacionPCI": "13-01-01-000",
    "ConsecutivoCDP": "1012"
  },
  "CdpPaginado": {
    "Enabled": true,
    "Headers": {
      "CodPciHeader": "13-01-01",
      "LoginUsuarioSiifHeader": "usuario",
      "ConsecutivoHeader": "1",
      "HashHeader": null
    },
    "PCIConsulta": "27-01-02",
    "PCISubUnidades": "TODAS",
    "TipoGasto": "Todos",
    "Rango": "Todos"
  },
  "Obligaciones": {
    "Enabled": true,
    "Headers": {
      "CodPciHeader": "13-01-01",
      "LoginUsuarioSiifHeader": "usuario",
      "ConsecutivoHeader": "1",
      "HashHeader": null
    },
    "CodPCI": "46-02-00-001",
    "TipoGasto": "Todos",
    "Rango": "Todos",
    "DetalleUsosPresupuestales": "No"
  }
}
```

El enlazador de configuración de .NET suele aceptar **PascalCase** y **camelCase**; el repositorio usa principalmente **PascalCase** en `appsettings.json`.

---

## 9. Solución de problemas

| Problema | Causa probable | Qué revisar |
|----------|----------------|-------------|
| El servicio no inicia (1053) | Falta `UseWindowsService` o binPath incorrecto | Versión publicada actual; binPath con `dotnet` + DLL. |
| No lee `appsettings.json` | Directorio de trabajo incorrecto | Código fija `ContentRoot`/`CurrentDirectory` al directorio del ensamblado. |
| Cron “no dispara” | Zona horaria o expresión | `TimeZoneId` válido en el SO; expresión con ocurrencias futuras. |
| SQL errors | Cadena incorrecta o permisos | `SqlServerProceso` / firewall / login. |
| 401/timeout SIIF | Credenciales o red | Sección `Siif`, conectividad TLS. |

---

*Documento alineado a `SiifScheduledIntegrationWorker`, `SiifWorkerExecutionOrchestrator` y `SiifWorkerOptions`.*
