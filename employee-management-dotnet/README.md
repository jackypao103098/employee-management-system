# Employee Management API — ASP.NET Core

## 專案目的

這個目錄是既有 Spring Boot 員工管理系統的 ASP.NET Core 重構版本，用來學習 C#／.NET，並讓 Java 與 .NET 面試官能在同一個 Git repository 比較兩種實作。第一週建立企業 Web API 基礎；第二週加入 Employee CRUD、驗證、CORS 與 React 串接；第三週深化 EF Core／PostgreSQL；第四週完成 JWT Authentication 與 Authorization。

## 第一週完成內容

- .NET 10 Solution、Web API Project 與 xUnit Test Project
- Controller 型 API 與 Attribute Routing
- ASP.NET Core 內建 Dependency Injection
- Options Pattern、設定繫結與啟動時驗證
- Development／Production 環境設定
- .NET 10 OpenAPI 與 Development-only Swagger UI
- 無外部相依的 Health Check
- `SystemInformationService` 單元測試
- GitHub Actions restore、Release build、test CI
- Java／Spring Boot 與 C#／ASP.NET Core 學習對照

## 第二週完成內容

- `ControllerBase`、`[ApiController]`、Attribute Routing 與 Model Binding
- Create／Update request DTO 與 response DTO，不直接暴露 Entity 或密碼
- Data Annotations Model Validation 與自動 `400 Bad Request`
- Scoped `EmployeeService` 與 Scoped EF Core `AppDbContext`
- PostgreSQL persistence、initial migration 與 Email unique index
- Employee REST CRUD 與 `200`、`201`、`204`、`400`、`404`、`409`
- `CreatedAtAction` 產生新資源的 `Location` header
- Development CORS allowlist，不使用 `AllowAnyOrigin`
- Service 單元測試與完整 HTTP integration tests
- React 透過 `VITE_API_BASE_URL` 操作 .NET API 的新增、查詢、修改與刪除

## 第三週完成內容

- 使用 Npgsql 的 Scoped `AppDbContext` 與完整 Employee Fluent API mapping
- 保留 Initial Migration，新增部門、到職日期、查詢索引及 Demo Seed Data Migration
- 全部 CRUD 使用 Async EF Core API 並傳遞 `CancellationToken`
- 唯讀查詢使用 `AsNoTracking()` 並直接 Projection 成 `EmployeeResponse`
- 姓名關鍵字搜尋與不分大小寫的部門篩選
- 到職日期升冪／降冪排序，以 Employee ID 作為穩定排序 tie-breaker
- `page`／`pageSize` 分頁與輸入範圍驗證
- 三筆可重複部署的 Demo Seed Data，使用負數 ID 避免干擾正式 identity sequence

## 第四週完成內容

- `POST /api/v1/auth/login` 登入 API 與成功／失敗安全稽核紀錄
- PBKDF2-SHA512 密碼雜湊、固定時間比較及舊雜湊登入後升級
- JWT HMAC-SHA256 簽章，以及 Issuer、Audience、Expiration 完整驗證
- JWT 包含 `sub`（Employee ID）、`email`、`role` claims
- Role Authorization：只有 Admin 可以刪除 Employee
- Policy Authorization：Employee 只能修改自己，Admin 可以修改所有人
- 未登入／Token 無效回傳 401；已登入但權限不足回傳 403
- JWT Secret 僅從環境變數或 .NET User Secrets 注入
- 無預設 Admin 密碼；以一次性環境變數安全 bootstrap 既有 Employee
- React 登入、Bearer Token interceptor 與依權限顯示操作按鈕

## 必要環境

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Docker Desktop 或相容的 Docker Engine（本機 PostgreSQL）
- 選用：支援 C# 的 IDE，例如 JetBrains Rider、Visual Studio 或 VS Code
- 執行第二週 CRUD 不需要 AWS 帳號

確認 SDK：

```bash
dotnet --version
```

## 還原、編譯與測試

以下指令都從本目錄 `employee-management-dotnet/` 執行：

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

CI 會以 Release configuration 執行 build，並透過 `--warnaserror` 將編譯警告視為失敗。

## 啟動 PostgreSQL 與套用 Migration

本專案使用獨立的 PostgreSQL container 與 port `5433`，不會占用既有 Java 專案的 port `5332`：

```bash
docker compose up -d
dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project src/EmployeeManagement.Api \
  --startup-project src/EmployeeManagement.Api
```

`appsettings.Development.json` 內的帳密只供本機 compose 開發環境使用。正式環境必須以 `ConnectionStrings__EmployeeDatabase` 注入真正的連線字串，不應提交正式密碼。

## 啟動 API

先在本機 User Secrets 產生 JWT signing secret；值儲存在 Repository 外：

```bash
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -hex 32)" \
  --project src/EmployeeManagement.Api
```

使用 HTTPS Development profile：

```bash
dotnet run --project src/EmployeeManagement.Api --launch-profile https
```

Development URLs：

- Swagger UI：<https://localhost:7180/swagger>
- OpenAPI JSON：<https://localhost:7180/openapi/v1.json>
- HTTP：`http://localhost:5180`

若本機尚未信任開發憑證，可執行 `dotnet dev-certs https --trust`，或測試時使用 `curl --insecure`。

## API 端點

| Method | Endpoint | 用途 |
|---|---|---|
| `GET` | `/health` | 回傳 API 存活狀態，不檢查資料庫 |
| `GET` | `/api/v1/system/info` | 回傳名稱、環境、版本及 UTC 時間 |
| `POST` | `/api/v1/auth/login` | Email／密碼登入，成功回傳 JWT |
| `GET` | `/api/v1/employees` | 需登入；搜尋、篩選、排序並分頁取得員工 |
| `GET` | `/api/v1/employees/{id}` | 需登入；取得指定員工，找不到回傳 404 |
| `POST` | `/api/v1/employees` | 公開註冊 Employee role，成功回傳 201 |
| `PUT` | `/api/v1/employees/{id}` | 本人或 Admin 可更新 |
| `DELETE` | `/api/v1/employees/{id}` | 只有 Admin 可刪除，成功回傳 204 |
| `GET` | `/openapi/v1.json` | Development-only OpenAPI 文件 |
| `GET` | `/swagger` | Development-only Swagger UI |

呼叫範例：

```bash
curl --insecure https://localhost:7180/health
curl --insecure https://localhost:7180/api/v1/system/info
```

登入並呼叫受保護 API：

```bash
login_response=$(curl -s http://localhost:5180/api/v1/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"your-email@example.com","password":"your-password"}')

access_token=$(printf '%s' "$login_response" | jq -r '.accessToken')

curl http://localhost:5180/api/v1/employees \
  -H "Authorization: Bearer $access_token"
```

員工列表支援的 Query Parameters：

| Parameter | 預設值 | 用途 |
|---|---:|---|
| `name` | 無 | 姓名關鍵字搜尋，不分大小寫 |
| `department` | 無 | 部門完整名稱篩選，不分大小寫 |
| `sortDirection` | `desc` | 到職日期排序，可用 `asc`／`desc` |
| `page` | `1` | 頁碼，最小值為 1 |
| `pageSize` | `20` | 每頁筆數，範圍 1～100 |

```bash
curl 'http://localhost:5180/api/v1/employees?name=ada&department=engineering&sortDirection=asc&page=1&pageSize=10' \
  -H "Authorization: Bearer $access_token"
```

排序永遠附加 Employee ID 作為第二排序鍵，因此到職日期相同時仍有確定順序，避免同一資料集在不同頁之間隨機移動。

建立員工範例：

```bash
curl -i http://localhost:5180/api/v1/employees \
  -H 'Content-Type: application/json' \
  -d '{"name":"王小明","email":"ming@example.com","age":30,"gender":"MALE","password":"password123","department":"Engineering","hireDate":"2026-09-01"}'
```

## React 串接 .NET API

先啟動 PostgreSQL 與 .NET API，再從 `frontend/react` 執行：

```bash
npm run dev:dotnet
```

`.env.dotnet` 將 `VITE_API_BASE_URL` 設成 `http://localhost:5180` 並啟用 Authentication。登入後 Token 保存在目前分頁的 `sessionStorage`，Axios interceptor 統一附加 Bearer Token；Admin 才顯示新增／刪除，Employee 只會看到自己的修改按鈕。前端條件顯示只是 UX，後端仍會獨立驗證每個 request。圖片 API 尚未實作，因此 .NET 模式會關閉圖片上傳。

也可以使用 [`EmployeeManagement.Api.http`](src/EmployeeManagement.Api/EmployeeManagement.Api.http) 從 IDE 發送請求。

## Java／.NET 雙軌展示

兩個後端位於同一個 repository，但彼此獨立：

```text
employee-management-system/
├── backend/                       # Java 17 / Spring Boot，預設 port 8080
├── frontend/react/                # 既有 React
└── employee-management-dotnet/    # C# / ASP.NET Core，port 5180 / 7180
```

若原 Java 專案所需的 PostgreSQL、環境變數與本機 profile 已備妥，可分別開兩個 Terminal：

```bash
# Terminal A：Spring Boot（repository 根目錄執行）
cd backend
mvn spring-boot:run
```

```bash
# Terminal B：ASP.NET Core（repository 根目錄執行）
cd employee-management-dotnet
dotnet run --project src/EmployeeManagement.Api --launch-profile https
```

建議面試展示順序：

1. 先說明 `pom.xml` 與 `.csproj`、Maven module 與 Solution／Project 的對照。
2. 比較 Java `Main.java` 與 .NET [`Program.cs`](src/EmployeeManagement.Api/Program.cs)。
3. 呼叫 Spring Boot `/ping`、Actuator health，再呼叫 .NET `/health` 與 `/api/v1/system/info`。
4. 比較 Spring annotation DI 與 .NET [`SystemInformationController`](src/EmployeeManagement.Api/Controllers/SystemInformationController.cs) 的 constructor injection。
5. 展示 Maven CI 與 `.github/workflows/dotnet-ci.yml` 的 restore／build／test quality gate。

Java 與 .NET 版本目前都有 JWT；.NET 版本尚未實作圖片 API，不能將兩邊功能完成度誤述為完全相同。

## 設定與環境

共同設定放在 `appsettings.json`，環境差異放在 `appsettings.{Environment}.json`。Development profile 透過 `launchSettings.json` 設定 `ASPNETCORE_ENVIRONMENT=Development`。

ASP.NET Core 預設允許環境變數覆寫 JSON。例如：

```bash
Application__Name="Employee Management API - Local" \
dotnet run --project src/EmployeeManagement.Api --launch-profile http
```

雙底線 `__` 代表設定階層分隔符。`Jwt__Secret` 是必要設定，Repository 內沒有預設值；不要將正式密碼、Token 或其他 Secret 提交到 appsettings。本機使用 User Secrets，部署環境應由安全的環境變數或 Secrets Manager 注入。

首次建立 Admin 時，可只在該次啟動注入既有 Employee 與自行選擇的強密碼：

```bash
AdminBootstrap__Email='existing-employee@example.com' \
AdminBootstrap__Password='choose-a-strong-password' \
dotnet run --project src/EmployeeManagement.Api --launch-profile http
```

成功後資料庫會保存雜湊並將該 Employee 設為 Admin；環境變數不需保留。若已是 Admin，後續啟動不會重設密碼。

## 專案結構

```text
employee-management-dotnet/
├── EmployeeManagement.sln
├── src/
│   └── EmployeeManagement.Api/
│       ├── Controllers/
│       ├── Contracts/Auth/
│       ├── Contracts/Employees/
│       ├── Data/Migrations/
│       ├── Entities/
│       ├── Errors/
│       ├── Models/
│       ├── Options/
│       ├── Security/
│       ├── Services/
│       ├── Program.cs
│       └── appsettings*.json
├── tests/
│   └── EmployeeManagement.Api.Tests/
├── docs/
│   ├── week-01-notes.md
│   ├── week-02-notes.md
│   ├── week-03-notes.md
│   └── week-04-notes.md
├── compose.yml
└── README.md
```

## CI 與 AWS 路線

本週只新增 .NET CI，不新增 CD 或 AWS 資源。CI 對 `main` 的相關 push／pull request 執行 restore、Release build 與 test，而且沒有 AWS credentials 或 deployment secrets。

未來完成圖片與 Docker 後，再比較 Elastic Beanstalk 與 ECS Fargate。AWS CD 應優先使用 GitHub OIDC 取得短期 IAM credentials，不沿用長期 access key，也不由 workflow 回寫 image tag 到 Git。

## 後續週次預計功能

- Profile image storage
- Docker image、AWS 架構、IaC、CD 與監控

學習筆記：[`第一週`](docs/week-01-notes.md)／[`第二週`](docs/week-02-notes.md)／[`第三週`](docs/week-03-notes.md)／[`第四週`](docs/week-04-notes.md)。
