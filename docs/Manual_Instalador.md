# Manual de uso — Instalador SSF.Interop.SIIFNacion.Installer

## 1. Propósito

La aplicación **Instalador — API SIIF Nación y Worker** (`SSF.Interop.SIIFNacion.Installer`) es una utilidad **WPF para Windows** que ayuda a:

- Comprobar e instalar **prerrequisitos** (.NET 9, rol IIS, ASP.NET Core Hosting Bundle).
- Editar y guardar **`appsettings.json`** de la API y del Worker en las carpetas de publicación.
- Crear o actualizar el **servicio de Windows** del Worker.
- Crear o actualizar un **sitio en IIS** para la API.

Las acciones que modifican el sistema operativo o IIS se ejecutan **elevadas** (UAC): se abrirá PowerShell o un script por lotes como administrador.

---

## 2. Requisitos

- **Windows** con permisos para instalar software y roles (el operador debe poder aceptar elevación UAC).
- Conexión a Internet **solo** si va a usar la pestaña de prerrequisitos para descargar instaladores desde los metadatos oficiales de Microsoft (canal **9.0**).
- Carpetas locales donde ya haya publicado la API y el Worker (`dotnet publish` o artefacto de CI), por ejemplo:
  - `...\SSF.Interop.SIIFNacion.API\bin\Release\net9.0\publish\`
  - `...\SSF.Interop.SIIFNacion.Worker\bin\Release\net9.0\publish\`

---

## 3. Pantalla principal

### 3.1 Rutas de publicación

En **“Rutas de publicación (destino de appsettings.json)”**:

1. **API (IIS):** carpeta física donde está el contenido publicado de la API (debe existir `appsettings.json` o se creará al guardar).
2. **Worker:** carpeta publicada del Worker (donde está `SSF.Interop.SIIFNacion.Worker.dll`).

Use **Examinar…** para elegir la carpeta.

### 3.2 Pestañas

| Pestaña | Uso |
|---------|-----|
| **Prerrequisitos (.NET / IIS)** | Ver estado y descargar/instalar IIS, SDK 9, Hosting Bundle. |
| **API — appsettings.json** | Ver/editar JSON de la API; cargar plantilla o archivo existente. |
| **Worker — appsettings.json** | Igual para el Worker. |
| **Servicio Windows (Worker)** | Instalar/reinstalar servicio con `sc create` apuntando a `dotnet.exe` y al DLL. |
| **IIS (API)** | Crear sitio, app pool y enlace HTTP en un puerto. |

Al pie: **Guardar ambos appsettings.json** — valida JSON y escribe `appsettings.json` en **ambas** carpetas si son válidas y existen.

---

## 4. Prerrequisitos

1. Abra la pestaña **Prerrequisitos**.
2. Pulse **Actualizar estado** para ver texto tipo consola con:
   - SDK .NET 9.x
   - Runtime `Microsoft.NETCore.App` 9.x
   - Runtime `Microsoft.AspNetCore.App` 9.x
   - Módulo IIS **ANCM v2** (Hosting Bundle)
   - Rol IIS (Web Server)
3. Marque o desmarque:
   - Instalar rol IIS si falta  
   - Instalar .NET SDK 9.x si falta  
   - Instalar Hosting Bundle si falta  
4. Pulse **Descargar (si aplica) e instalar como administrador…**  
   - Si no hay pasos pendientes según su selección, verá un mensaje informativo.  
   - Si hay descargas, consultará metadatos de releases .NET y luego abrirá **PowerShell elevado** para la instalación.

**Criterios usados por el instalador:**

- **Worker listo:** runtime .NET 9 **o** SDK 9.
- **API en IIS listo:** rol IIS + módulo ANCM v2 + runtime ASP.NET Core 9.

---

## 5. Edición de appsettings.json

### 5.1 Plantilla desde el repositorio

Al iniciar, el instalador intenta cargar desde la subcarpeta **`Defaults`** junto al ejecutable:

- `Defaults\ApiAppsettings.json`
- `Defaults\WorkerAppsettings.json`

Si compiló el instalador **junto al repositorio** con esos archivos embebidos o copiados al directorio de salida, el botón **Plantilla (repo)** recarga la plantilla.

### 5.2 Cargar desde carpeta publicada

- **Cargar desde carpeta API / Worker:** lee `{carpeta}\appsettings.json` y lo muestra en el editor.

### 5.3 Reformatear JSON

Valida y formatea el contenido del cuadro de texto. Si el JSON es inválido, muestra el error.

### 5.4 Guardar

**Guardar ambos appsettings.json** exige:

- JSON válido en ambas pestañas.
- Rutas de API y Worker existentes.

Escribe `appsettings.json` en cada carpeta (UTF-8).

**Seguridad:** no almacene contraseñas reales en capturas de pantalla ni en repositorios; use secretos gestionados en el entorno de despliegue.

---

## 6. Servicio Windows (Worker)

1. Complete **Rutas de publicación → Worker** con la carpeta que contiene `SSF.Interop.SIIFNacion.Worker.dll`.
2. En la pestaña **Servicio Windows**:
   - **Nombre del servicio** (por defecto `SiifInteropWorker`).
   - **dotnet.exe** (por defecto `C:\Program Files\dotnet\dotnet.exe`).
3. Pulse **Instalar o reinstalar servicio (administrador)…**

El instalador genera un `.bat` temporal que:

- Detiene y elimina el servicio si ya existe (mismo nombre).
- Crea el servicio con `binPath` apuntando a `dotnet.exe` y al DLL del Worker.
- Establece descripción e inicia el servicio.

Si faltan runtimes, se le dirigirá a la pestaña **Prerrequisitos**.

---

## 7. IIS (API)

1. Complete **Rutas de publicación → API** con la carpeta física del sitio (contenido publicado de la API).
2. En **IIS (API)** indique:
   - **Nombre del sitio** (ej. `SiifInteropApi`)
   - **App pool** (ej. `SiifInteropApiPool`)
   - **Puerto HTTP** (ej. `8080`)
3. Pulse **Crear o actualizar sitio IIS (administrador)…**

El script PowerShell elevado:

- Crea el app pool si no existe y deja **managedRuntimeVersion vacío** (modo ASP.NET Core).
- Si el sitio ya existe con el mismo nombre, lo elimina y lo vuelve a crear.
- Crea el sitio enlazado al puerto y a la ruta física indicada.

Debe estar instalado el **Hosting Bundle** y el rol IIS según la pestaña de prerrequisitos.

---

## 8. Verificación posterior

- **API:** navegue a `http://{servidor}:{puerto}/` (mensaje de estado) y a Swagger si está habilitado en el entorno; revise `/healthcheck`.
- **Worker:** Visor de eventos de Windows → registro **Aplicación**, origen relacionado con el proceso; revise también logs configurados en `Logging` del Worker.

---

## 9. Solución de problemas

| Síntoma | Acción sugerida |
|---------|-------------------|
| “No se encontró Defaults\\ApiAppsettings.json” | Compile el instalador con los archivos de plantilla en `Defaults` o use **Cargar desde carpeta**. |
| Worker no arranca | Confirme `dotnet.exe`, ruta al DLL y que `appsettings.json` esté en la misma carpeta del DLL. |
| IIS 502.5 / ANCM | Instale/repare Hosting Bundle; reinicie el app pool. |
| UAC cancelado | Las tareas elevadas no se completan; vuelva a ejecutar y acepte elevación. |

---

*Documento generado a partir del código del proyecto `SSF.Interop.SIIFNacion.Installer`.*
