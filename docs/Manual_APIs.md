# Manual de uso — APIs REST (SSF.Interop.SIIFNacion.API)

## 1. Descripción general

La API **ASP.NET Core 9** expone integración con **SIIF Nación** bajo la ruta base:

```text
/api/siif
```

El controlador principal es `SiifController`. Los contratos JSON están definidos en `SSF.Interop.SIIFNacion.API/Models/SIIF/`.

**Documentación interactiva:** con la aplicación en modo desarrollo suele estar disponible OpenAPI/Swagger según `Program.cs`. En producción, deshabilite o proteja Swagger según política de seguridad.

**Salud:**

- `GET /healthcheck` — comprobación de salud.
- `GET /healthcheckui` — UI de health checks (si está configurada).

**Raíz:**

- `GET /` — respuesta simple de estado (`DefaultController`).

**Contenido:** todas las operaciones SIIF documentadas aquí usan **`POST`** con cuerpo **JSON** (`Content-Type: application/json`).

---

## 2. URL base y entornos

Ejemplos locales según `Properties/launchSettings.json`:

- HTTP: `http://localhost:5038`
- HTTPS: `https://localhost:7166`

En **IIS**, use el host, puerto o encabezado `Host` configurado en el sitio (por ejemplo `http://servidor:8080` si desplegó con el instalador en el puerto 8080).

---

## 3. Convenciones

- **Cabeceras SIIF en el cuerpo:** los campos `CodPciHeader`, `LoginUsuarioSiifHeader`, `ConsecutivoHeader` y opcionalmente `HashHeader` se envían en el JSON (no como cabeceras HTTP del cliente hacia la API SSF), y la capa de aplicación los proyecta a las llamadas a SIIF según el handler.
- **Errores:** respuestas 400 si el cuerpo es nulo; 500 ante fallos no controlados (detalle según `ExceptionMiddleware` y configuración de entorno).
- **Autenticación hacia la API:** el proyecto puede no imponer JWT/API Key en el código mostrado; en producción debe acotarse el acceso (IIS, API Management, red, etc.).

---

## 4. Endpoints

### 4.1 Consultar CDP (puntual)

**Ruta:** `POST /api/siif/consultar-cdp`

**Cuerpo (`ConsultarCdpApiRequest`):**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `codPciHeader` | string | Código PCI (cabecera SIIF). |
| `loginUsuarioSiifHeader` | string | Usuario SIIF (cabecera). |
| `consecutivoHeader` | string | Consecutivo (cabecera). |
| `hashHeader` | string (opcional) | Hash si aplica. |
| `identificacionPCI` | string | Identificación PCI del servicio consultar CDP. |
| `consecutivoCDP` | string | Consecutivo del CDP. |

**Respuesta:** objeto devuelto por el handler `ConsultarCdpQuery` (payload de SIIF enriquecido o mapeado según implementación).

**Ejemplo mínimo:**

```json
{
  "codPciHeader": "13-01-01",
  "loginUsuarioSiifHeader": "usuario",
  "consecutivoHeader": "1",
  "hashHeader": null,
  "identificacionPCI": "13-01-01-000",
  "consecutivoCDP": "1012"
}
```

---

### 4.2 Consultar CDP paginada

**Ruta:** `POST /api/siif/consultar-cdp-paginada`

**Cuerpo (`ConsultarCdpPaginadaApiRequest`):**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `codPciHeader` | string | Cabecera PCI. |
| `loginUsuarioSiifHeader` | string | Usuario SIIF. |
| `consecutivoHeader` | string | Consecutivo. |
| `hashHeader` | string (opcional) | Hash. |
| `page` | int | Número de página. |
| `size` | int | Tamaño de página. |
| `pciConsulta` | string | Filtro PCI de consulta. |
| `pciSubUnidades` | string | Subunidades PCI. |
| `fechaRegistroIni` | string | Inicio de rango de registro (formato acordado con SIIF). |
| `fechaRegistroFin` | string | Fin de rango. |
| `tipoGasto` | string | Tipo de gasto. |
| `rango` | string | Rango. |

**Respuesta:** resultado del query paginado (estructura definida por el handler).

---

### 4.3 Consulta compromiso presupuestal RP

**Ruta:** `POST /api/siif/consulta-compromisopresupuestal-RP`

**Cuerpo (`ConsultarCompromisoPresupuestalRPApiRequest`):**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `codPciHeader` | string | Cabecera PCI. |
| `loginUsuarioSiifHeader` | string | Usuario SIIF. |
| `consecutivoHeader` | string | Consecutivo. |
| `hashHeader` | string (opcional) | Hash. |
| `pci` | string | PCI del compromiso. |
| `codCompromisoPptalGastos` | int | Código compromiso presupuestal de gastos. |
| `vigencia` | string | Vigencia. |

---

### 4.4 Sincronizar lista de obligaciones

**Ruta:** `POST /api/siif/sincronizar-lista-obligaciones`

Consulta todas las páginas en SIIF (tamaño de página manejado en el handler, típicamente hasta 50 por página según contrato SIIF) y persiste en la tabla **`DYNTBLSIIFOBLIGAPO`**.

**Cuerpo (`SincronizarListaObligacionesApiRequest`):**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `codPciHeader` | string | Cabecera PCI. |
| `loginUsuarioSiifHeader` | string | Usuario SIIF. |
| `consecutivoHeader` | string | Consecutivo. |
| `hashHeader` | string (opcional) | Hash. |
| `codPCI` | string | PCI del cuerpo `LstObliEnt`. |
| `fechaInicio` | DateTime | Inicio del rango. |
| `fechaFin` | DateTime | Fin del rango. |
| `tipoGasto` | string | Tipo de gasto. |
| `rango` | string | Rango. |
| `vigencia` | string | Vigencia del listado en SIIF (`"1"`, `"2"`, `"3"`, etc.). |
| `detalleUsosPresupuestales` | string | Detalle de usos presupuestales. |

**Respuesta:** `SincronizarListaObligacionesResultDto` (resumen: registros insertados/actualizados, páginas procesadas, etc., según implementación).

---

## 5. Configuración requerida (servidor API)

En `appsettings.json` (o variables de entorno con el mismo jerarquía):

- `ConnectionStrings:SqlServerProceso` — conexión SQL principal.
- `Siif` — `BaseUrl`, `AuthenticationPath`, `User`, `Password`, tiempos de token y `TimeoutSeconds`.
- `WorkerSettings:Convenio` — puede estar presente para otros usos; los endpoints SIIF no lo listan todos en el controlador.

Tras cambiar configuración, recicle el **application pool** de IIS o reinicie el proceso.

---

## 6. Pruebas con curl (ejemplo)

```bash
curl -s -X POST "http://localhost:5038/api/siif/consultar-cdp" ^
  -H "Content-Type: application/json" ^
  -d "{\"codPciHeader\":\"13-01-01\",\"loginUsuarioSiifHeader\":\"usuario\",\"consecutivoHeader\":\"1\",\"identificacionPCI\":\"13-01-01-000\",\"consecutivoCDP\":\"1012\"}"
```

(Ajuste comillas si usa bash en lugar de cmd/PowerShell.)

---

## 7. Buenas prácticas

- Use **HTTPS** en producción y restrinja orígenes si habilita CORS de forma explícita en el pipeline.
- No exponga credenciales SIIF en repositorios; use secretos por ambiente.
- Monitoree `/healthcheck` desde su plataforma de observabilidad.

---

*Documento alineado a `SiifController` y modelos en `SSF.Interop.SIIFNacion.API/Models/SIIF/`.*
