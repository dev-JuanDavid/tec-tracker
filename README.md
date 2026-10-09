# TEC Tracker

Aplicativo web para administrar empresas y empleados, consultar actividad laboral y visualizar inventarios de software y hardware.

## 1. Qué contiene este repositorio

- **Servidor web:** ASP.NET MVC 5, Razor y Web API sobre **.NET Framework 4.8**. Solución: `Queue.sln`; proyecto web: `Queue/Queue.csproj`.
- **SQL Server:** Entity Framework 6 y ASP.NET Identity. Almacena empresas, empleados, usuarios, roles, grupos, clasificaciones, configuración y licencias.
- **MongoDB:** almacena actividad e inventarios. Los informes consultan, entre otras, las colecciones `TrackerTime`, `Software` y `Hardware`.
- **Interfaz:** Tailwind cargado desde CDN, componentes Razor reutilizables y gráficas ApexCharts en las vistas migradas. Algunas pantallas y dependencias son heredadas.
- **Logs:** log4net, con archivo local y un appender de errores a SQL.

El **agente MonitorTracker Cliente**, que se instala en los computadores y recoge actividad, no está incluido en esta solución. Este repositorio recibe sus datos y los presenta. Ejecutar la web por sí sola no inicia el monitoreo de un computador.

## 2. Requisitos de desarrollo

En Windows:

1. Visual Studio con la carga de trabajo **Desarrollo de ASP.NET y web**, IIS Express y herramientas de compilación de aplicaciones web.
2. **Developer Pack / Targeting Pack de .NET Framework 4.8**. Tener solo el runtime no garantiza que el proyecto compile.
3. Acceso a SQL Server y a la base de datos del entorno.
4. Acceso a MongoDB local, remoto o Atlas, según la configuración.
5. Acceso a NuGet para restaurar los paquetes declarados en `Queue/packages.config`.
6. Conexión a Internet para los recursos de interfaz servidos mediante CDN.

Es un proyecto clásico de .NET Framework: la ejecución habitual es con Visual Studio e IIS Express. No tiene el flujo `dotnet run` de un proyecto ASP.NET Core ni requiere `npm install` para el Tailwind actual.

## 3. Primera ejecución: orden recomendado

### Paso 1. Abrir la solución y restaurar paquetes

1. Abre `Queue.sln` y establece **Queue** como proyecto de inicio.
2. Habilita la descarga/restauración de paquetes en las opciones de NuGet de Visual Studio.
3. Haz clic derecho sobre la solución y selecciona **Restaurar paquetes NuGet**.
4. Compila después de restaurar. Si aparece «The build restored NuGet packages. Build the project again», compila una segunda vez.

Si tienes `nuget.exe` instalado, también puedes restaurar desde la raíz:

```powershell
nuget restore .\Queue.sln
```

Los paquetes se esperan en `packages/`, junto a la solución. Conserva las versiones de `packages.config`; una actualización masiva puede introducir incompatibilidades.

### Paso 2. Preparar SQL Server

La conexión se llama **`QueueContext`** y está en `Queue/Web.config`, dentro de `connectionStrings`.

Ejemplo orientativo; reemplaza los nombres por los de tu entorno:

```xml
<add name="QueueContext"
     connectionString="Data Source=SERVIDOR_SQL;Initial Catalog=BASE_TEC;Integrated Security=True;MultipleActiveResultSets=True"
     providerName="System.Data.SqlClient" />
```

Si utilizas autenticación SQL, configura sus credenciales localmente. No publiques contraseñas ni cadenas de conexión reales en documentación o commits.

**La base no se inicializa automáticamente al arrancar.** `QueueContext` desactiva el inicializador de Entity Framework y el método `Seed` de las migraciones no agrega los datos iniciales de negocio.

Para un entorno nuevo, la opción más directa es restaurar una copia autorizada y compatible de la base de pruebas. Si necesitas partir de una base vacía, primero revisa las migraciones y prepara los datos iniciales; no basta con crear una base con un nombre cualquiera.

En la Consola del Administrador de Paquetes, con Queue como proyecto predeterminado, puedes generar un script para revisar los cambios de esquema:

```powershell
Update-Database -Script -ProjectName Queue -StartUpProjectName Queue
```

Después de revisar el script y confirmar que la conexión apunta a la base correcta, la aplicación de migraciones puede hacerse con:

```powershell
Update-Database -ProjectName Queue -StartUpProjectName Queue
```

No ejecutes migraciones sobre una base compartida o de producción sin revisar su estado y disponer de respaldo. La configuración de migraciones permite migraciones automáticas; los comandos anteriores no garantizan por sí solos que una base existente sea compatible.

### Paso 3. Preparar los datos iniciales

Antes de iniciar sesión o registrar nuevas empresas, necesitas:

- Una empresa inicial existente.
- Los roles usados por el aplicativo. `SAdmin` y `SuperAdmin` se consideran equivalentes en la política; `Admin`, `Employer` y `User` tienen funciones diferentes.
- Una licencia asociada a la empresa inicial con `enddate >= DateTime.Today`.
- Una cuenta con contraseña de ASP.NET Identity y relación en `Agent_UserCompanies`, o el bootstrap descrito abajo.
- Una empresa plantilla identificada por **`BaseCompany`**, con una funcionalidad asignada y una licencia de referencia.

El registro de una empresa copia funcionalidad, licencia, horarios y configuración desde `BaseCompany`. Si falta configuración, actualmente usa valores predeterminados: inactividad, envío y capturas de **10 segundos**, y ubicación de **1 minuto**. Si faltan funcionalidad o licencia, el registro falla con un mensaje en los logs.

**La licencia inicial copia la fecha de la plantilla.** Aunque exista un comentario en el código sobre 30 días, el registro no calcula automáticamente 30 días desde la creación. Mantén vigente la licencia de referencia y revisa la de la nueva empresa en Licencias.

### Paso 4. Configurar MongoDB

En `appSettings` de `Queue/Web.config`:

```xml
<add key="MongoConnectionString" value="mongodb://localhost:27017" />
<add key="MongoDatabase" value="BASE_MONGO_DEL_ENTORNO" />
```

Usa la URI correspondiente al entorno, con autenticación y TLS cuando corresponda. SQL Server y MongoDB son dependencias independientes: lograr el login no prueba que los informes puedan conectarse a MongoDB.

Si utilizas Atlas, comprueba las reglas de acceso de red, el usuario de base de datos y la resolución DNS de la URI. Verifica el acceso desde el computador que ejecuta la web.

### Paso 5. Configurar el primer administrador

El arranque llama a `DefaultAdministrator.EnsureCreated()`, en `Queue/App_Start/IdentityConfig.cs`. Estas claves controlan la creación:

| Clave | Contenido esperado |
| --- | --- |
| `BootstrapAdmin.Enabled` | `true` para ejecutar el bootstrap |
| `BootstrapAdmin.Email` | Correo de la cuenta inicial |
| `BootstrapAdmin.Password` | Contraseña local, de al menos 12 caracteres según el validador del bootstrap |
| `BootstrapAdmin.UserId` | GUID de la cuenta |
| `BootstrapAdmin.RoleId` | ID de un rol existente, no su nombre |
| `BootstrapAdmin.CompanyId` | GUID de una empresa existente |

Este proceso **no crea la empresa ni el rol**. Verifica que existan antes de habilitarlo. Si el ID o correo pertenece a una cuenta diferente de la configurada, el bootstrap rechaza la operación.

Después de crear la cuenta inicial, deshabilita el bootstrap y retira la contraseña de su configuración. Si ya existe una cuenta válida, no necesitas habilitarlo.

La aplicación valida una licencia vigente al iniciar sesión. El estado «Activa» de la empresa y la vigencia de su licencia son conceptos diferentes.

### Paso 6. Configurar correo y archivos

Claves de correo: `SMTPHost`, `SMTPPort`, `SMTPUserName`, `SMTPPassword`, `SMTPEnableSsl`, `SMTPTimeout`, `SenderEmailAddress` y `SenderDisplayName`.

También existen `WelcomeTemplate`, `ForgotTemplate`, `AlertsTemplate`, `EmailSubjec` y `EmailSubjectForgot`. **`EmailSubjec` tiene ese nombre exacto en el código.** Comprueba que las rutas de las plantillas existan dentro del aplicativo.

`EmailNotification=false` omite la invitación por correo durante el registro de empresas. No desactiva todos los envíos: recuperación de contraseña y registro de usuarios tienen sus propios flujos. Una empresa creada sin invitación necesita un procedimiento para entregar o establecer el acceso del administrador; la contraseña se genera automáticamente.

El código de envío actualmente fuerza `EnableSsl=true`, aunque lea `SMTPEnableSsl`. Configura un servidor compatible con ese comportamiento.

Revisa también:

- `FilesRepository`: ubicación de los archivos y permisos de escritura necesarios.
- `CSV_Separator`: separador empleado en la carga de empleados.
- `JWT_SECRET_KEY`, `JWT_AUDIENCE_TOKEN`, `JWT_ISSUER_TOKEN`, `JWT_EXPIRE_MINUTES`: autenticación utilizada por los agentes. Mantén esos valores privados y coordinados con el cliente.

### Paso 7. Compilar y ejecutar

1. Selecciona **Debug / Any CPU**.
2. Compila la solución.
3. Ejecuta con IIS Express desde Visual Studio.
4. Usa la dirección que abra Visual Studio. El proyecto tiene configurada `http://localhost:51741/`, pero el puerto puede variar entre entornos.
5. Inicia sesión con una cuenta asociada a una empresa con licencia vigente.
6. Revisa los logs del arranque y abre el panel de actividad.

## 4. Probar los informes sin instalar el agente

Existe la pantalla **`/ReportTestData`**, disponible desde localhost para roles administradores. Está limitada a una empresa y un empleado de pruebas definidos en `Queue/Controllers/ReportTestDataController.cs`.

En otra base debes adaptar esos identificadores a registros existentes. El empleado debe pertenecer a esa empresa, tener el campo `Usuario` informado y coincidir la empresa con la sesión actual.

La carga genera:

- 24 registros de actividad: **2 horas entre 08:00 y 10:00**, en la fecha seleccionada.
- 3 registros de software y 3 de hardware.
- Equipo identificado como **`TEC-PRUEBA-FE78B48F`**.

Repetirla actualiza la misma muestra, incluida su fecha. El botón de limpieza elimina los registros de ese equipo asociados al empleado y empresa de prueba. No modifica los grupos ni crea capturas.

Para comprobar:

| Pantalla | Qué seleccionar |
| --- | --- |
| Panel de actividad | Fecha de la carga y empleado; todos los grupos inicialmente |
| Reporte de actividades (`/ReportGantt`) | Misma fecha, intervalo de 5 minutos, todos los grupos, Consultar reporte y después pulsar el usuario |
| Sumado de actividades | Misma fecha y empleado |
| Software / Hardware | Buscar el usuario o equipo de prueba y abrir su detalle |

Los colores y porcentajes dependen de las clasificaciones existentes. La muestra usa programas de cada tipo disponible; si faltan, avisa para clasificar los programas `PRUEBA`. Los datos reales del empleado también pueden aparecer. Las horas usan la zona horaria del servidor.

Para monitorear otro computador necesitas el instalador/código del agente y una URL del servidor accesible desde ese computador; `localhost` no apunta a tu servidor desde un equipo diferente.

## 5. Dónde revisar los logs

Archivo de desarrollo:

```text
Queue/App_Data/Logs/web-log.txt
```

En una publicación, la ruta está bajo `App_Data/Logs` del sitio. La identidad de IIS debe poder escribir allí. El archivo rota según `Queue/log4net.config`; también hay un appender SQL para errores en `dbo.LogApi`, que depende de que exista esa tabla y sus permisos.

Busca estos prefijos:

- `BootstrapAdmin:`: primer administrador y sus requisitos.
- `Login:`: autenticación, empresa y licencia.
- `EmpresaRegistro:`: empresa base, datos iniciales, administrador, rol, correo, asociación y limpieza.
- `UsuarioRegistro:`: registro de cuentas y relación con empresas.
- `HTTP`: ruta, estado, tiempo y `requestId`.

Ejemplo para seguir el archivo mientras reproduces un error, desde la raíz:

```powershell
Get-Content .\Queue\App_Data\Logs\web-log.txt -Tail 80 -Wait -Encoding UTF8
```

Detén la lectura con Ctrl+C. Correlaciona las líneas mediante `requestId`; el formulario de registro de empresas muestra una referencia cuando falla. Antes de compartir logs, retira información privada o detalles de infraestructura presentes en las excepciones.

## 6. Errores frecuentes y cómo abordarlos

### «NuGet packages are missing» o «Build the project again»

Restaura paquetes y vuelve a compilar. Si la descarga está incompleta, comprueba la fuente de NuGet, acceso a Internet y permisos de la carpeta `packages`.

### Paquetes duplicados, por ejemplo `Microsoft.Owin.Security.Cookies`

Revisa `Queue/packages.config` y conserva una sola entrada coherente para el mismo paquete. Después restaura. No elimines arbitrariamente referencias del `.csproj`: deben corresponder a la versión elegida.

### Falta `AWSSDK.SecurityToken.CodeAnalysis.dll`

Comprueba que se haya restaurado íntegramente `AWSSDK.SecurityToken` en la versión declarada y que la referencia del analizador en el proyecto apunte al archivo existente. No descargues una DLL suelta de sitios externos ni cambies su nombre para satisfacer la ruta.

### Falta `System.Net.Http`, `System.Runtime` o `System.Runtime.InteropServices.RuntimeInformation`

Revisa, en este orden:

1. .NET Framework 4.8 Developer Pack y restauración de paquetes.
2. Referencias y `HintPath` del proyecto.
3. Propiedad **Copiar local**, cuando corresponda, y DLLs presentes en `Queue/bin`.
4. `bindingRedirect` de `Web.config` frente a las versiones reales de ensamblado.
5. Limpieza y recompilación desde Visual Studio después de corregir referencias; reinicia el sitio.

**La versión del paquete NuGet no es necesariamente la versión del ensamblado.** No copies versiones de mensajes anteriores a los redirects sin comprobar la DLL. Para inspeccionar una DLL desde PowerShell:

```powershell
[System.Reflection.AssemblyName]::GetAssemblyName(
    (Resolve-Path .\Queue\bin\System.Runtime.InteropServices.RuntimeInformation.dll).Path
).FullName
```

El valor CLR `4.0.30319` en la página de error no significa por sí solo que la aplicación use .NET Framework 4.0; .NET Framework 4.x comparte esa familia del CLR.

### Timeout de MongoDB después de 30000 ms

Comprueba URI, credenciales, DNS, conectividad, firewall y acceso de red de Atlas si aplica. El timeout de selección de servidor puede ocurrir antes de ejecutar una consulta. Aumentarlo no corrige una conexión inaccesible.

### Vuelve al login o rechaza una cuenta válida

Revisa logs `Login:`, la relación en `Agent_UserCompanies`, la licencia de la empresa, el rol asignado y la sesión. Las credenciales de una cuenta web y el campo `Usuario` de un empleado no son la misma entidad.

### Registro de empresa: «La empresa base no tiene…»

Comprueba `BaseCompany` y los datos asociados a esa plantilla:

- Sin funcionalidad o licencia: debes prepararlas.
- Sin configuración: se usa la configuración predeterminada y se registra un aviso.
- Error en correo: revisa el fallo SMTP y `EmailNotification`.
- Error Identity: revisa duplicados de correo/usuario, reglas de nombres, contraseña y existencia del rol `Admin`.

La limpieza posterior al fallo puede dejar una cuenta Identity creada previamente; los logs lo advierten. Revisa los registros parciales antes de reintentar repetidamente.

### SMTP: «Access denied - Invalid HELO name»

El servidor rechaza el nombre con el que se presenta el cliente SMTP. Puede requerir un dominio completo aceptado por el proveedor. Consulta ese dominio y configura `clientDomain` en la sección `system.net/mailSettings/smtp/network`; no uses un dominio inventado.

Ejemplo de estructura, con un valor que debes reemplazar:

```xml
<system.net>
  <mailSettings>
    <smtp>
      <network clientDomain="servidor.tudominio.com" />
    </smtp>
  </mailSettings>
</system.net>
```

Si la sección ya existe, añade el atributo a su elemento `network`. La recuperación de contraseña usa actualmente `async void` en `SendForgot`; un error SMTP puede producir una página de error sin manejo adecuado. Está pendiente convertirlo a `Task` y esperar el resultado desde el controlador.

### Razor: «Expected a '{' but found…»

Usa llaves en los bloques de control de las vistas, incluso para una sola instrucción:

```cshtml
@if (condicion) {
    <p>Contenido</p>
}
```

El caso identificado en `_RoleForm.cshtml` se corrigió. Para cambios C# recompila; para cambios de vistas, recarga y comprueba si el sitio recompila Razor correctamente.

### Tildes como «DescripciÃ³n» o «configuraci�n»

Guarda vistas en UTF-8 con BOM y abre los logs con la codificación correcta. Comprueba la sección `globalization` de `Web.config`. Un texto ya guardado con caracteres corruptos necesita corregirse; cambiar la codificación no siempre lo reconstruye.

### Página vacía, gráfica ausente o estilos incompletos

Comprueba fecha, grupo y usuario seleccionados; en Reporte de actividades debes pulsar el usuario después de consultar. Revisa también las solicitudes a los CDN y la consola del navegador: Tailwind y ApexCharts dependen de recursos externos en el diseño actual.

## 7. Permisos y pendientes conocidos

La distribución prevista se documenta en `DESIGN_RULES.md`:

- `SAdmin` / `SuperAdmin`: administración completa.
- `Admin`: administración de su empresa y reportes.
- `Employer`: panel y reportes empresariales.
- `User`: cuenta personal; los reportes actuales no están filtrados como informes personales.

**No interpretes el menú oculto como garantía de autorización.** Existe `WorkspaceAccessFilter` y se agrega en `FilterConfig.RegisterGlobalFilters`, pero el `Application_Start` actual no llama a ese registro. Hay que conectar esa llamada y validar las rutas antes de considerar aplicada toda la política de acceso. Los atributos `[Authorize]` presentes siguen siendo relevantes.

La opción **Roles** está oculta por decisión del proyecto. En la prueba manual se detectó una diferencia entre `ApplicationRoleManager` (`ApplicationRole`) y el contexto (`IdentityRole`), con listado vacío y fallo de validación al guardar. Su corrección está pendiente; no uses esa pantalla para preparar los roles iniciales.

Otros límites de primera instalación:

- No hay un instalador de servidor ni del agente en este repositorio.
- No hay un seed completo de empresa, roles, funcionalidades y licencia para una base vacía.
- El registro de empresas realiza varias escrituras y una limpieza compensatoria; no representa una única transacción que incluya Identity y correo.
- La pantalla de datos de prueba tiene identificadores específicos del entorno actual.

## 8. Organización y desarrollo

| Ruta | Responsabilidad |
| --- | --- |
| `Queue/Controllers` | Pantallas MVC y endpoints Web API |
| `Queue/Models` / `Queue/ViewModels` | Entidades y modelos de interfaz |
| `Queue/DAL/QueueContext.cs` | Contexto SQL e Identity |
| `Queue/Models/MongoHelper.cs` | Conexión y acceso a MongoDB |
| `Queue/App_Start` | Rutas, Identity y política de acceso |
| `Queue/Migrations` | Migraciones de Entity Framework |
| `Queue/Views/Shared/Components` | Inputs, formularios, tablas, detalles y gráficas compartidos |
| `Queue/Scripts/components` | Comportamiento compartido de tablas y selectores |
| `Queue/Content/branding` | Logo TEC Tracker |
| `DESIGN_RULES.md` | Reglas de diseño y distribución prevista de permisos |

Al agregar archivos, regístralos en `Queue/Queue.csproj`, que utiliza entradas explícitas `Compile` y `Content`. Reutiliza los componentes y unifica crear/editar cuando sus campos sean equivalentes. Consulta también `Queue/Views/Shared/Components/README.md`.

Esta guía describe el código y los problemas observados del proyecto. Las credenciales, servidores y datos iniciales deben prepararse para cada entorno; no se han ejecutado migraciones, instalaciones ni pruebas de arranque como parte de la creación de este README.
