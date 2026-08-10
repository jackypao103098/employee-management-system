# Employee Management API — ASP.NET Core

## 專案目的

這個目錄是既有 Spring Boot 員工管理系統的 ASP.NET Core 重構版本，用來學習 C#／.NET，並讓 Java 與 .NET 面試官能在同一個 Git repository 比較兩種實作。第一週只建立企業 Web API 的基礎，不實作 Employee CRUD、資料庫、JWT、Docker、AWS 或 React 串接。

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

## 必要環境

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- 選用：支援 C# 的 IDE，例如 JetBrains Rider、Visual Studio 或 VS Code
- 執行本週 .NET API 不需要 PostgreSQL、Docker 或 AWS 帳號

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

## 啟動 API

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
| `GET` | `/openapi/v1.json` | Development-only OpenAPI 文件 |
| `GET` | `/swagger` | Development-only Swagger UI |

呼叫範例：

```bash
curl --insecure https://localhost:7180/health
curl --insecure https://localhost:7180/api/v1/system/info
```

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

Java 版本已有完整 Employee、資料庫與驗證功能；.NET 版本目前刻意停在第一週基礎，不能將兩邊功能完成度誤述為相同。

## 設定與環境

共同設定放在 `appsettings.json`，環境差異放在 `appsettings.{Environment}.json`。Development profile 透過 `launchSettings.json` 設定 `ASPNETCORE_ENVIRONMENT=Development`。

ASP.NET Core 預設允許環境變數覆寫 JSON。例如：

```bash
Application__Name="Employee Management API - Local" \
dotnet run --project src/EmployeeManagement.Api --launch-profile http
```

雙底線 `__` 代表設定階層分隔符。不要將密碼、Token 或其他 Secret 提交到 appsettings；未來應由安全的環境變數或 AWS Secrets Manager 注入。

## 專案結構

```text
employee-management-dotnet/
├── EmployeeManagement.sln
├── src/
│   └── EmployeeManagement.Api/
│       ├── Controllers/
│       ├── Models/
│       ├── Options/
│       ├── Services/
│       ├── Program.cs
│       └── appsettings*.json
├── tests/
│   └── EmployeeManagement.Api.Tests/
├── docs/
│   └── week-01-notes.md
└── README.md
```

## CI 與 AWS 路線

本週只新增 .NET CI，不新增 CD 或 AWS 資源。CI 對 `main` 的相關 push／pull request 執行 restore、Release build 與 test，而且沒有 AWS credentials 或 deployment secrets。

未來完成 CRUD、PostgreSQL、JWT 與 Docker 後，再比較 Elastic Beanstalk 與 ECS Fargate。AWS CD 應優先使用 GitHub OIDC 取得短期 IAM credentials，不沿用長期 access key，也不由 workflow 回寫 image tag 到 Git。

## 後續週次預計功能

- Employee request／response DTO 與 Controller CRUD
- EF Core、PostgreSQL、migration 與資料存取
- 輸入驗證、例外處理與 integration tests
- JWT authentication／authorization
- React API 切換與 CORS
- Docker image、AWS 架構、IaC、CD 與監控

完整第一週概念與面試題請閱讀 [`docs/week-01-notes.md`](docs/week-01-notes.md)。
