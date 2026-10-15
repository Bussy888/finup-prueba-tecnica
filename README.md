# Punto de Reclamo

Aplicación de reclamos para una entidad financiera: el cliente registra un reclamo, recibe un folio y consulta su estado; el personal interno los gestiona desde un panel. Cada reclamo tiene 5 días hábiles de plazo y el sistema calcula y muestra el vencimiento.

- `backend/`: API en C# (.NET 10), EF Core y SQL Server.
- `frontend/`: Next.js 16 (App Router) con TypeScript.

Todos los datos de ejemplo son inventados.

## Requisitos

- .NET SDK 10
- Node.js 22 o superior
- SQL Server: LocalDB (viene con Visual Studio), Express o Docker
- Docker (solo si quieres usar `docker-compose`)

## Opción rápida: Docker

```bash
docker compose up --build
```

- Web: http://localhost:3000
- API y Swagger: http://localhost:8080/swagger

Levanta SQL Server, aplica las migraciones y carga los datos semilla automáticamente. La primera vez la API puede tardar unos segundos en conectarse a la base (reintenta sola).

Las credenciales de prueba se pueden cambiar con las variables `SA_PASSWORD`, `ADMIN_API_KEY` y `JWT_KEY`.

## Ejecución local

### 1. Base de datos

La cadena de conexión de desarrollo apunta a LocalDB (`backend/src/Claims.Api/appsettings.Development.json`). Si usas otro SQL Server, cámbiala ahí o con user-secrets:

```bash
cd backend/src/Claims.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=ClaimsDb;User Id=sa;Password=...;TrustServerCertificate=True"
```

No hace falta crear nada a mano: en desarrollo la API aplica las migraciones (`Database:MigrateOnStartup`) y carga los datos semilla la primera vez que arranca. Para hacerlo manualmente:

```bash
cd backend
dotnet ef database update -p src/Claims.Infrastructure -s src/Claims.Api
```

Los datos semilla son 6 reclamos en distintos estados; el `REC-AAAA-000005` está vencido. Los correos son `ana.torrez@example.com`, `luis.mamani@example.com`, etc. (ver `DbSeeder.cs`).

### 2. Backend

```bash
cd backend/src/Claims.Api
dotnet run --launch-profile http
```

Queda en http://localhost:5282 y Swagger en http://localhost:5282/swagger.

### 3. Frontend

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

Abre http://localhost:3000. La URL de la API sale de `NEXT_PUBLIC_API_URL` (navegador) y `API_URL` (servidor de Next, opcional).

### Clave de administrador (solo pruebas)

```
admin-prueba-123
```

Está en `appsettings.Development.json` y en el `docker-compose.yml` como valor por defecto. En `appsettings.json` la clave y la clave del JWT vienen vacías a propósito: la API no arranca si no se configuran (`Admin__ApiKey`, `Jwt__Key`).

## Pruebas y calidad

```bash
cd backend && dotnet test          # 60 pruebas: días hábiles, transiciones, validaciones, servicio
cd frontend && npm test            # validaciones, transiciones, cliente HTTP, URLs del panel
cd frontend && npm run lint && npm run format:check && npm run typecheck
```

## API

| Método y ruta | Acceso | Respuestas |
| --- | --- | --- |
| `POST /api/claims` | Público | 201, 400, 422 |
| `GET /api/claims/{folio}?email=` | Público | 200, 404 |
| `GET /api/admin/claims?status=&page=&pageSize=` | Admin | 200, 401 |
| `GET /api/admin/claims/{folio}` | Admin | 200, 401, 404 |
| `PATCH /api/admin/claims/{folio}/status` | Admin | 200, 401, 404, 409, 422 |
| `POST /api/auth/login` | Público | 200 (JWT), 401 |

Los endpoints de admin aceptan la clave en el header `X-Api-Key` o un JWT en `Authorization: Bearer`. El JWT se obtiene con `POST /api/auth/login` enviando `{ "apiKey": "..." }`.

Los errores usan `ProblemDetails`. Los 422 incluyen `errors` por campo (`fullName`, `email`...) y el frontend los muestra junto a cada campo.

## Supuestos

- **Días hábiles:** lunes a viernes contados en UTC. El plazo conserva la hora del registro (registrado el martes 14:30 UTC, vence el martes siguiente 14:30 UTC). Si el reclamo entra en fin de semana, el conteo arranca el siguiente día hábil. **Los feriados quedan fuera de alcance**, como indica la prueba.
- **Fechas:** se guardan en UTC y la web las muestra en hora de La Paz.
- **Folio:** `REC-AAAA-NNNNNN`, correlativo por año (el año se toma de la fecha de registro en UTC).
- **Seguimiento:** el email se compara sin distinguir mayúsculas. Un folio inexistente y un email incorrecto devuelven el mismo 404 con el mismo mensaje. La respuesta pública no incluye CI, teléfono ni descripción.
- **Vencido:** reclamo en `Recibido` o `EnProceso` cuya fecha límite ya pasó.
- **Historial:** al registrar un reclamo se crea la primera entrada (`fromStatus` nulo, `Recibido`).
- **Endpoints extra:** agregué `GET /api/admin/claims/{folio}` (el panel necesita el detalle) y `POST /api/auth/login`.
- **Paginación:** `page` mínimo 1, `pageSize` entre 1 y 100 (valores fuera de rango se corrigen, no dan error).
- **Documento de identidad:** solo se exige que no esté vacío (máx. 30 caracteres); no valido el formato boliviano.
- **Alcance de la protección del panel:** la clave se cambia por un JWT de 60 min que se guarda en una cookie `httpOnly` (nunca llega al JavaScript del navegador). No hay usuarios ni roles: cualquiera con la clave es administrador. La cookie es `secure` en producción salvo `COOKIE_SECURE=false` (necesario para Docker sobre HTTP).
- **CORS:** orígenes permitidos en `Cors:AllowedOrigins` (por defecto `http://localhost:3000`).

## Decisiones

**Tres decisiones de diseño**

1. **Folio con una tabla contador y `MERGE ... WITH (HOLDLOCK)`.** Necesitaba un correlativo por año, único aunque lleguen solicitudes simultáneas. `MAX(folio)+1` falla con concurrencia y una `SEQUENCE` de SQL Server no se reinicia sola cada año. El `MERGE` toma el bloqueo de la fila del año, incrementa y devuelve el número dentro de la misma transacción que inserta el reclamo; además `Folio` tiene índice único como red de seguridad. Lo probé disparando 8 solicitudes en paralelo y salieron 8 folios consecutivos.
2. **Reglas de negocio en el dominio, sin dependencias.** `BusinessDayCalculator` y `ClaimStatusRules` son clases estáticas puras y `ClaimService` recibe un `TimeProvider`, así que las pruebas fijan la fecha sin simular nada de infraestructura. El servicio lanza excepciones de dominio (`NotFound`, `Conflict`, `ValidationFailed`) y un único `IExceptionHandler` las traduce a 404/409/422, por lo que los controladores quedan delgados. Uso 422 para validación de negocio y dejo el 400 para JSON mal formado o enums inválidos, que es lo que devuelve el binding de ASP.NET.
3. **Panel admin con Server Components, Server Actions y cookie `httpOnly`.** El navegador nunca ve la clave ni el token: el login es una Server Action que pide el JWT a la API y lo guarda en cookie, y la página `/admin` consulta la API desde el servidor. Cambiar el estado es otra Server Action que llama a `revalidatePath`, así la tabla y el detalle se actualizan sin recargar. Elegí renderizado en servidor justo ahí porque los datos dependen de la sesión, cambian todo el tiempo y no tiene sentido exponer el token a código cliente. En cambio `/reclamos` y `/seguimiento` son formularios cliente que llaman directo a la API pública (por eso el CORS), porque necesitan validación y estado local por campo.

**Qué dejé fuera o simplifiqué**

- Feriados en el cálculo del plazo (documentado arriba).
- Pruebas de integración de la API contra SQL Server y pruebas de componentes/E2E en la web; solo tengo pruebas unitarias de lógica.
- Usuarios y roles reales, refresh tokens, límite de intentos en el login y captcha en el formulario público.
- Registrar *quién* hizo cada cambio de estado en el historial y una notificación por correo al cliente.
- Con más horas haría primero las pruebas de integración con Testcontainers, luego feriados configurables y la auditoría del historial.

**Algo que tuve que aprender**

La versión de Next.js que trae `create-next-app` (16) activa `cacheComponents`, que cambia bastante respecto a lo que conocía. En vez de asumir, leí las guías incluidas en `node_modules/next/dist/docs` (autenticación con Cache Components y *fetching data*). Con ese modo, leer cookies o `searchParams` obliga a envolver todo en `Suspense`, y mi caso (datos que cambian en cada petición y dependen de la sesión) no gana nada con el caché. Decidí quitar `cacheComponents` y usar el modelo dinámico normal, y dejé `output: "standalone"` para el Dockerfile.
