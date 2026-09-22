# Employee Management System — ASP.NET Core

[![.NET CI](https://github.com/jackypao103098/employee-management-system/actions/workflows/dotnet-ci.yml/badge.svg?branch=feat/dotnet-week-01)](https://github.com/jackypao103098/employee-management-system/actions/workflows/dotnet-ci.yml?query=branch%3Afeat%2Fdotnet-week-01)

以 ASP.NET Core 10、EF Core 與 PostgreSQL 實作的員工管理 REST API，並串接 React TypeScript 前端。重點放在 JWT 權限控制、密碼安全，以及用真實資料庫驗證的自動化測試。

[🌐 Live Demo](https://main.d1zjxnolctj65r.amplifyapp.com) · [☕ Spring Boot 版本](https://github.com/jackypao103098/employee-management-system/tree/springboot-interview-v1) · [📂 Repository](https://github.com/jackypao103098/employee-management-system)

> **展示模式：** 為了控制成本，線上網站不連接後端，登入與 CRUD 的資料只存在目前瀏覽器。Demo 帳號 `demo@jackypao.com`／`password` 僅適用於此模式。完整的 API 與 PostgreSQL 可依下方 Local Setup 在本機執行。

## Screenshots

| Login | Employee Dashboard |
|---|---|
| ![Login](../docs/login.png) | ![Dashboard](../docs/dashboard.png) |

## 技術亮點

- **角色與資源擁有權授權**：建立與刪除限 Admin（Role-based），修改資料限本人或 Admin（resource-based policy）。Role 與本人 ID 只取自簽章驗證過的 JWT，不信任 request body。
- **密碼安全**：PBKDF2-SHA512（210,000 iterations）搭配 fixed-time 比對；舊格式 hash 在登入成功後自動升級。API 與前端同步要求強密碼。
- **一致的錯誤回應**：以 `IExceptionHandler` 統一輸出 Problem Details，401／403／404／409 語意明確。
- **查詢**：名稱搜尋、部門篩選、依到職日排序與分頁，使用 `AsNoTracking` 與 projection，只查詢需要的欄位。
- **安全日誌**：登入失敗只記錄 Email 的 SHA-256 fingerprint，不記錄密碼、Token 或原始 Email。
- **測試與 CI**：38 個 xUnit 測試，包含以 Testcontainers 啟動真實 PostgreSQL 的整合測試；GitHub Actions 執行格式檢查與 analyzer，warning 一律視為錯誤。

## Tech Stack

| Backend | Frontend | Data & Testing |
|---|---|---|
| .NET 10、ASP.NET Core、JWT、OpenAPI | React 18、TypeScript、Vite、Chakra UI | PostgreSQL 17、EF Core、xUnit、Testcontainers、GitHub Actions |

## API

| Method | Endpoint | 說明 | 權限 |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | 登入並取得 JWT | 公開 |
| `GET` | `/api/v1/employees` | 搜尋、篩選、排序及分頁 | 已登入 |
| `GET` | `/api/v1/employees/{id}` | 查詢單一員工 | 已登入 |
| `POST` | `/api/v1/employees` | 建立員工 | Admin |
| `PUT` | `/api/v1/employees/{id}` | 更新員工 | 本人或 Admin |
| `DELETE` | `/api/v1/employees/{id}` | 刪除員工 | Admin |
| `GET` | `/health` | Health check | 公開 |

未登入回 `401`，已登入但權限不足回 `403`。

## 設計取捨

- **授權只在後端決定**：前端解碼 JWT 僅用來隱藏無權限的按鈕。`jwt-decode` 不驗證簽章，因此不能作為安全依據。
- **Email 唯一性雙重保護**：Service 先檢查重複，資料庫再以 unique index 擋下併發寫入，兩種情況都統一回 `409`。
- **不在 seed data 寫死 Admin 密碼**：首位 Admin 透過環境變數 bootstrap，資料庫只保存 hash。

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
dotnet format --verify-no-changes
dotnet build --configuration Release --warnaserror
dotnet test --configuration Release --no-build
```

PostgreSQL 整合測試使用 Testcontainers，執行時需要 Docker。

## Status

下一步：API container 化與 AWS 部署、profile image storage、observability。

技術筆記：[API 基礎](docs/week-01-notes.md) · [CRUD](docs/week-02-notes.md) · [EF Core](docs/week-03-notes.md) · [JWT](docs/week-04-notes.md)
