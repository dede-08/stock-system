# ShopStore — Sistema de Stock

Monorepo con backend ASP.NET Core 8 + PostgreSQL y frontend Angular 17.

```
├── system-stock-backend/   # API REST (JWT + refresh, EF Core, Serilog)
├── system-stock-frontend/  # SPA Angular (standalone, lazy loading)
├── compose.yaml            # Postgres + API para dev/prod local
└── .github/workflows/      # CI backend (build+test) y frontend (lint+test+build)
```

## Arranque rápido (Docker)

```bash
cp system-stock-backend/.env.example system-stock-backend/.env
# completa JWT_KEY (64+ chars) y PGPASSWORD
docker compose up --build
```

API en `http://localhost:8080` (`/health`, Swagger solo en Development).

## Arranque local

Backend:

```bash
cd system-stock-backend
cp .env.example .env   # JWT_KEY + ConnectionStrings__DefaultConnection
dotnet ef database update
dotnet run             # http://localhost:5173
dotnet test api-gestion-productos.sln
```

Frontend:

```bash
cd system-stock-frontend
npm ci
npm start              # http://localhost:4200 (proxy: actualiza environment.ts si cambia el puerto)
npm run test:ci
npm run build -- --configuration production
```

## Variables clave

| Variable | Dónde | Descripción |
|---|---|---|
| `JWT_KEY` | backend | Clave HMAC 64+ chars (nunca commitear) |
| `ConnectionStrings__DefaultConnection` | backend | Cadena Npgsql |
| `ADMIN_EMAIL` | backend | Email promovido a ADMIN al arrancar |
| `CORS_ALLOWED_ORIGINS` / `Cors__AllowedOrigins__0` | backend | Orígenes frontend permitidos |

## Contratos principales

- `POST /api/auth/add-user` · `POST /api/auth/login` → `{token, refreshToken, email, fullName}`
- `POST /api/auth/refresh` (rotación) · `POST /api/auth/logout`
- `GET /api/products?page=&pageSize=&sortBy=&desc=` → `{items, total, page, pageSize, totalPages}`
- `GET /api/stats/products` · `GET /health`

Docs por proyecto en cada `README.md`.
