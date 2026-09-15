
Una API REST completa para la gestión de inventario con autenticación JWT, desarrollada en ASP.NET Core 8.0.

## Características

- **Autenticación JWT**: Sistema de autenticación seguro con tokens JWT
- **CRUD Completo**: Operaciones Create, Read, Update, Delete para productos
- **Validaciones**: Validaciones robustas con Data Annotations
- **DTOs**: Separación de modelos de entrada y salida
- **Servicios**: Arquitectura en capas con servicios de negocio
- **Estadísticas**: Endpoints para reportes y estadísticas
- **Documentación**: Swagger UI integrado con documentación completa
- **Logging**: Middleware para logging de requests
- **CORS**: Configuración para desarrollo frontend
- **Soft Delete**: Eliminación lógica de productos

## Tecnologías

- **ASP.NET Core 8.0**
- **Entity Framework Core**
- **PostgreSQL**
- **JWT Authentication**
- **Swagger/OpenAPI**
- **CORS**

## Requisitos Previos

- .NET 8.0 SDK
- PostgreSQL (o Docker)
- Visual Studio 2022 o VS Code

## Puesta en marcha

```bash
cp .env.example .env   # completa JWT_KEY y PGPASSWORD
dotnet ef database update
dotnet run
```

Con Docker (desde la raíz del repo):

```bash
docker compose up --build
```

## Endpoints clave

- `GET /health` — health check (sin auth)
- `POST /api/auth/add-user` — registro (rate-limit 10/min)
- `POST /api/auth/login` — login, devuelve `{token,refreshToken,email,fullName}`
- `POST /api/auth/refresh` — rota el par con `{refreshToken}`
- `POST /api/auth/logout` — revoca el refresh
- `GET /api/auth/users` — solo ADMIN
- `POST /api/auth/promote` — solo ADMIN, body `{email,role}`
- `GET /api/products?page=1&pageSize=20&sortBy=price&desc=true` — paginado
- `GET /api/stats/products` — stats (cache 60s)

## Tests y CI

```bash
dotnet test api-gestion-productos.sln
```

CI en `.github/workflows/backend-ci.yml` (build + test).


## Arquitectura

```
api-gestion-productos/
├── Controllers/          # Controladores de la API
├── Data/                # Contexto de Entity Framework
├── Models/              # Modelos de datos y DTOs
├── Services/            # Servicios de negocio
├── Middleware/          # Middleware personalizado
├── Program.cs           # Configuración de la aplicación
└── README.md           # Documentación
```


