# Employee Management System — ASP.NET Core

以 **ASP.NET Core、React、PostgreSQL** 打造的全端員工管理 side project，實作 Employee CRUD、JWT 登入、角色授權、EF Core persistence 與自動化測試。

[🌐 Live Demo](https://main.d1zjxnolctj65r.amplifyapp.com) · [☕ Spring Boot 版本](https://github.com/jackypao103098/employee-management-system/tree/springboot-interview-v1) · [📂 Repository](https://github.com/jackypao103098/employee-management-system)

> Live Demo 使用共用 React 前端的 browser demo mode，資料保存在瀏覽器，不會呼叫 .NET API；完整 .NET API 可依下方步驟在本機執行。

**Demo 帳號：** `demo@jackypao.com`／`password`

## Screenshots

| Login | Employee Dashboard |
|---|---|
| ![Login](../docs/login.png) | ![Dashboard](../docs/dashboard.png) |

## 技術亮點

- ASP.NET Core REST API 與 React TypeScript 前端
- JWT Authentication、Admin／Employee 角色與 Owner-based Authorization
- EF Core、PostgreSQL、Migration、unique index 與 demo seed data
- 搜尋、部門篩選、排序與分頁
- DTO validation、Problem Details 與一致的 HTTP status code
- xUnit service tests 與 HTTP integration tests

## Tech Stack

| Backend | Frontend | Data & Testing |
|---|---|---|
| .NET 10、ASP.NET Core、JWT、OpenAPI | React 18、TypeScript、Vite、Chakra UI | PostgreSQL 17、EF Core、xUnit |

## Architecture

```text
React + TypeScript
        │ HTTP / Bearer JWT
        ▼
ASP.NET Core Controllers
        ▼
Application Services + Authorization Policies
        ▼
EF Core / Npgsql → PostgreSQL
```

## API

| Method | Endpoint | 說明 |
|---|---|---|
| `POST` | `/api/v1/auth/login` | 登入並取得 JWT |
| `GET` | `/api/v1/employees` | 搜尋、篩選及分頁 |
| `POST` | `/api/v1/employees` | 建立員工 |
| `PUT` | `/api/v1/employees/{id}` | 更新員工 |
| `DELETE` | `/api/v1/employees/{id}` | 刪除員工 |
| `GET` | `/health` | Health check |

## Local Setup

需求：.NET 10 SDK、Node.js 18+、Docker。

在 `employee-management-dotnet/` 執行：

```bash
docker compose up -d

dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project src/EmployeeManagement.Api \
  --startup-project src/EmployeeManagement.Api

dotnet user-secrets set "Jwt:Secret" "$(openssl rand -hex 32)" \
  --project src/EmployeeManagement.Api

AdminBootstrap__Email='ada.lovelace@example.com' \
AdminBootstrap__Password='ChangeMe-Strong123!' \
dotnet run --project src/EmployeeManagement.Api --launch-profile http
```

- API：<http://localhost:5180>
- Swagger：<http://localhost:5180/swagger>

啟動 React 的 .NET 模式：

```bash
cd ../frontend/react
npm install
npm run dev:dotnet
```

## Tests

```bash
dotnet build --configuration Release --warnaserror
dotnet test --configuration Release --no-build
```

測試涵蓋 service、authentication、authorization、CRUD 與 API status codes。

## Status

已完成可在本機完整操作的 API、JWT 權限控制、React 串接與測試。下一步是 profile image storage、container deployment 與 observability。

技術筆記：[API 基礎](docs/week-01-notes.md) · [CRUD](docs/week-02-notes.md) · [EF Core](docs/week-03-notes.md) · [JWT](docs/week-04-notes.md)
