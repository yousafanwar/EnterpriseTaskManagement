# Enterprise Task Management Platform (ETMP)

ASP.NET Core Web API for enterprise task tracking: projects, users and roles, skills-based assignment, comments, and audit history. Optional AI turns free-text feedback (and images) into structured tasks; WhatsApp Cloud API can drive the same intake flow.

## Features

- **Users & auth** — registration, JWT login, refresh-token rotation (HttpOnly cookie), password reset, roles
- **Projects** — CRUD, member membership, project-scoped skills
- **Tasks** — CRUD, assign / start / complete lifecycle, paging & filters, comments, history
- **Skills catalog** — global skills; user and project skill links; skill-aware auto-assignment
- **AI task analysis** — multipart feedback (+ optional image) → structured task via Gemini or OpenAI; images on Cloudinary
- **WhatsApp intake** — Meta Cloud API webhook; project pick → feedback/image → AI task create
- **API docs** — Swagger UI at `/swagger`
- **Docker & CI** — compose stack, GitHub Actions build/test/push/deploy on `master`

## Tech stack

| Area | Choice |
|------|--------|
| Runtime | .NET 10 (`net10.0`) |
| API | ASP.NET Core, controllers, ProblemDetails, Swashbuckle |
| Data | EF Core 10 + SQL Server (LocalDB locally; SQL Server 2022 in Docker) |
| Auth | JWT Bearer + BCrypt passwords + hashed refresh tokens |
| AI | Google Gemini or OpenAI (toggle via env) |
| Images | Cloudinary |
| Messaging | WhatsApp Cloud API (Graph `v21.0`) |
| Tests | xUnit |
| Containers | Docker multi-stage (`sdk:10.0` → `aspnet:10.0`), image `yousaf007/etmp-api` |

## Architecture

Clean Architecture with dependency flow **API → Infrastructure / Persistence → Application → Domain**.

```
EnterpriseTaskManagement.slnx
├── EnterpriseTaskManagement.API            # Host, controllers, Swagger, DI, AI/agent.md
├── EnterpriseTaskManagement.Application    # Services, DTOs, interfaces, policies
├── EnterpriseTaskManagement.Domain         # Entities & enums (no NuGet deps)
├── EnterpriseTaskManagement.Infrastructure # Gemini/OpenAI, Cloudinary, WhatsApp client
├── EnterpriseTaskManagement.Persistence    # EF Core DbContext, repos, migrations
└── EnterpriseTaskManagement.Tests          # Unit tests (AI parse, skills, assignment)
```

### Domain model (high level)

| Entity | Role |
|--------|------|
| `User` / `Role` / `RefreshToken` | Identity & session |
| `Project` / `ProjectUser` | Projects & membership |
| `TaskItem` / `Comment` / `TaskHistory` | Work items, discussion, audit |
| `Skill` / `UserSkill` / `ProjectSkill` / `TaskSkill` | Skills graph |
| `WhatsAppContact` / `WhatsAppConversationSession` / `ProcessedWhatsAppMessage` | WhatsApp config, sessions, idempotency |

**Task status:** `Pending` → `InProgress` → `Completed` (start requires an assignee; complete requires `InProgress`).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or LocalDB (or Docker for the full stack)
- Optional: Gemini or OpenAI API key, Cloudinary account, Meta WhatsApp Cloud API app

## Quick start (local)

1. **Clone and restore**

   ```bash
   git clone <repo-url>
   cd EnterpriseTaskManagement
   dotnet restore EnterpriseTaskManagement.slnx
   ```

2. **Environment**

   ```bash
   cp .env.example .env
   ```

   Set at least `JWT_KEY` (32+ characters recommended). The API loads `.env` via DotNetEnv when not running in a container.

3. **Database**

   Default connection (LocalDB) is in `EnterpriseTaskManagement.API/appsettings.json`. Override with `ConnectionStrings__DefaultConnection` if needed.

   Migrations are **not** applied automatically at startup:

   ```bash
   dotnet ef database update \
     --project EnterpriseTaskManagement.Persistence \
     --startup-project EnterpriseTaskManagement.API
   ```

   (Requires the `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`.)

4. **Run**

   ```bash
   dotnet run --project EnterpriseTaskManagement.API --launch-profile https
   ```

   | Profile | URLs |
   |---------|------|
   | `http` | http://localhost:5299 |
   | `https` | https://localhost:7010 and http://localhost:5299 |

   - Swagger: https://localhost:7010/swagger  
   - CORS allows frontend origins `http://localhost:5173` and `https://projectmanagementfrontend.omnisoft.info`

5. **Tests**

   ```bash
   dotnet test EnterpriseTaskManagement.slnx
   ```

## Docker Compose

```bash
cp .env.example .env
# Set MSSQL_SA_PASSWORD and JWT_KEY (and optional AI_* / CLOUDINARY_*)
docker compose up -d
```

| Service | Details |
|---------|---------|
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest`, volume `etmp-sqlserver-data` |
| `api` | `yousaf007/etmp-api:latest` on **8080** |

API is reachable at http://localhost:8080 (Swagger at `/swagger`). Apply EF migrations against the container DB before first use. WhatsApp-related env vars are documented in `.env.example` but are **not** wired in `docker-compose.yml` by default—add them to the `api` service if you need WhatsApp in compose.

## Configuration

### `appsettings.json`

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `Jwt:Key` / `Issuer` / `Audience` / `ExpirationMinutes` | JWT signing & lifetime (default **60** minutes) |

Feature flags and third-party credentials come from **environment / `.env`**, not JSON.

### Environment variables (`.env.example`)

| Variable | Required when | Purpose |
|----------|---------------|---------|
| `MSSQL_SA_PASSWORD` | Docker SQL | SA password |
| `JWT_KEY` | Always (prod) | JWT signing key |
| `AI_ENABLED` | — | `true` / `false` (also `1` / `yes`) |
| `AI_PROVIDER` | AI on | `Gemini` (default) or `OpenAI` |
| `AI_API_KEY` / `AI_MODEL` | AI on | Provider credentials & model |
| `CLOUDINARY_CLOUD_NAME` / `API_KEY` / `API_SECRET` | AI on | Image storage |
| `WHATSAPP_ENABLED` | — | Requires `AI_ENABLED=true` |
| `WHATSAPP_VERIFY_TOKEN` / `APP_SECRET` / `ACCESS_TOKEN` | WhatsApp on | Meta Cloud API |
| `WHATSAPP_GRAPH_API_BASE_URL` | optional | Default `https://graph.facebook.com/v21.0` |
| `WHATSAPP_SEND_REPLIES` | optional | Default `true` |

Webhook callback (WhatsApp): `https://<public-host>/api/integrations/whatsapp/webhook`

When AI is enabled, startup fails fast if API key, model, `AI/agent.md`, or Cloudinary credentials are missing.

## Authentication

1. **Register** — `POST /api/users` (anonymous)
2. **Login** — `POST /api/users/login` → access token in body; refresh token in HttpOnly cookie `refreshToken` (`Secure`, `SameSite=Strict`, path `/api/users`)
3. **Refresh** — `POST /api/users/refresh` (cookie); rotates refresh token (~7-day expiry)
4. **Logout** — revokes refresh token and clears cookie

JWT claims: user id (`NameIdentifier`), `Email`, `Role`. Passwords are BCrypt-hashed.

Migration seeds role **Admin** (id `1`). Admin-gated areas include user update/delete, admin password reset, skill create/delete, and WhatsApp contact management.

## API overview

Base path: `/api/...`. Swagger lists full contracts.

| Area | Base route | Notes |
|------|------------|--------|
| Users | `/api/users` | CRUD, login/refresh/logout, profile, skills, password reset |
| Roles | `/api/Roles` | List/create roles |
| Projects | `/api/projects` | CRUD + members + skills (`[Authorize]`) |
| Tasks | `/api/Tasks` | CRUD, assign, start, complete, paging |
| AI analyze | `POST /api/tasks/analyze` | Multipart; authorized; 15 MB limit; 503 if AI off |
| History | `/api/tasks/{taskId}/history` | Task audit trail |
| Comments | `/api/Comments` | Create, list by task, delete |
| Skills | `/api/skills` | Catalog; create/delete Admin |
| WhatsApp contacts | `/api/whatsapp/contacts` | Admin; requires WhatsApp enabled |
| WhatsApp webhook | `/api/integrations/whatsapp/webhook` | GET verify, POST inbound (signature check) |

## AI task analysis

With `AI_ENABLED=true`:

1. Caller must be Admin or a project member
2. Optional image (JPEG/PNG/GIF/WebP) is validated and uploaded to Cloudinary
3. Model (prompt from `EnterpriseTaskManagement.API/AI/agent.md`) returns JSON: `title`, `description`, `requiredSkillIds`
4. Skill IDs are filtered to the project; a member is chosen by skill overlap (tie-break: lowest user id)
5. Task + skills + history are persisted; Cloudinary cleanup runs on failure

Limits: feedback/description ~4000 chars; title ~100; upload max ~15 MB.

## WhatsApp integration

Requires **both** `WHATSAPP_ENABLED=true` and `AI_ENABLED=true`.

- Meta verify challenge + `X-Hub-Signature-256` validation
- Admin registers business `phone_number_id` via `POST /api/whatsapp/contacts`
- User flow: menu (`hi` / `start`, …) → interactive project list → feedback text ± image → AI task create as system bot user
- Sessions TTL **60 minutes**; webhook retries are idempotent via `ProcessedWhatsAppMessage`
- With WhatsApp enabled, startup ensures bot user `whatsapp-bot@system.local` (Admin role)

## CI/CD

Workflow: [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml) — **Deploy ETMP** on push to `master`.

1. Restore / build / test (`.NET 10`)
2. Build & push `yousaf007/etmp-api:latest` and `:sha` to Docker Hub
3. SCP `docker-compose.yml` to the server and `docker compose pull && up -d`

Expected GitHub secrets: `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`, `SERVER_HOST`, `SERVER_PORT`, `SERVER_USER`, `SERVER_SSH_KEY`. Runtime secrets (DB password, JWT, AI, etc.) live on the server `.env`, not in the workflow.

## Project layout tips

| Path | Why it matters |
|------|----------------|
| `EnterpriseTaskManagement.API/Program.cs` | DI, auth, CORS, Swagger, feature registration |
| `EnterpriseTaskManagement.API/AI/agent.md` | System prompt for task analysis (copied to publish output) |
| `EnterpriseTaskManagement.Persistence/Migrations/` | EF schema history |
| `.env.example` | Canonical env documentation |
| `Dockerfile` / `docker-compose.yml` | Container build & local/prod compose |

## License

Proprietary / private — update this section if you publish or open-source the project.
