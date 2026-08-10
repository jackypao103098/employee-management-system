# 第一週學習筆記：從 Spring Boot 到 ASP.NET Core

## Java／.NET 核心對照

| Java／Spring Boot | C#／ASP.NET Core |
|---|---|
| Maven multi-module | Solution／Project |
| `pom.xml` | `.csproj` |
| `@SpringBootApplication`＋`main` | `Program.cs`＋`WebApplication` |
| `@RestController` | `[ApiController]`＋`ControllerBase` |
| `@RequestMapping` | `[Route]` |
| `@GetMapping` | `[HttpGet]` |
| Spring DI | ASP.NET Core 內建 DI |
| `application.yml` | `appsettings.json` |
| Spring Profile | ASP.NET Core Environment |
| Spring Boot Actuator Health | ASP.NET Core Health Checks |
| springdoc／Swagger | `Microsoft.AspNetCore.OpenApi`＋Swagger UI |
| JUnit | xUnit |
| Java `CompletableFuture` | C# `Task`／async-await |
| getter／setter | C# Property |
| Java Interface | C# Interface |
| Java record／POJO DTO | C# record／class DTO |
| Maven `verify` | `dotnet test` |

兩個框架的目的相近：處理 HTTP request、解析 routing、建立物件、注入依賴、載入設定並執行 cross-cutting concerns。語法不同，不代表 REST、依賴反轉、設定外部化或測試隔離等工程原則不同。

## Solution 與 Project

`EmployeeManagement.sln` 是 IDE、CLI 與 CI 使用的容器，列出一起工作的 projects 與 build configurations。它本身不產生執行檔。

`EmployeeManagement.Api.csproj` 才是編譯單位，宣告 target framework、SDK 與 NuGet dependencies，編譯後產生 assembly。`EmployeeManagement.Api.Tests.csproj` 是另一個編譯單位，透過 `ProjectReference` 引用 API。

可以把 Solution 類比為 Maven aggregator／multi-module workspace，把每個 `.csproj` 類比為有自己 `pom.xml` 的 module。常見錯誤是認為一個 Solution 只能有一個可部署程式，或為每個資料夾都建立 Project，導致不必要的 assembly 邊界。

**面試說法：** Solution 組織多個 Project；Project 定義實際的編譯、相依與輸出。兩者分離後，API 與測試可以各自有明確依賴。

## `Program.cs` 的用途

.NET 10 Web API 範本使用 top-level statements。`Program.cs` 主要分成兩段：

1. `builder.Services...` 註冊 framework services、Options 與 application services，類似 Spring 的 component registration／`@Bean`。
2. `app...` 組合 HTTP middleware 與 endpoints，類似設定 Servlet filters、Spring MVC 與 Actuator endpoints。

`builder.Build()` 是邊界：之前配置 DI container，之後配置已建立的 application。常見錯誤是把所有商業邏輯放進 `Program.cs`；它應作為 composition root，而不是 service implementation。

## Middleware Pipeline

Middleware 是依序處理 `HttpContext` 的元件。每個 middleware 可以：

- 在下一個元件之前執行邏輯。
- 呼叫 next 將 request 往下傳。
- 在 response 回程執行邏輯。
- 提前結束 pipeline。

順序很重要。例如 exception handling 應包住可能失敗的後續元件；authentication 必須先建立 identity，authorization 才能判斷權限；HTTPS redirect 必須在 endpoint 執行前發生。

Spring Boot 可類比 Servlet Filter chain 或 HandlerInterceptor，但 ASP.NET Core middleware 是 request pipeline 的核心組合方式。

## Controller 如何被發現

`AddControllers()` 註冊 MVC controller services，`MapControllers()` 將 attribute-routed actions 加入 endpoint routing。符合 controller convention、繼承 `ControllerBase` 且具有 public actions 的型別會被 application part discovery 找到。

`[ApiController]` 提供 API-specific behavior，例如 attribute routing requirement、binding source inference 與自動 model validation response。它不等於 routing；`[Route]`／`[HttpGet]` 才定義 route template 與 HTTP method。

常見錯誤包括：忘記 `MapControllers()`、route 少一段、action 不是 public、把 DTO 換成匿名物件，或在 controller 組合所有商業資料。

## DI 註冊與 Constructor Injection

本專案註冊：

```csharp
builder.Services.AddSingleton<ISystemInformationService, SystemInformationService>();
```

Controller 只依賴 interface，runtime 由 DI container 建立 implementation 並傳入 constructor。這對照 Spring 的 constructor injection。兩邊都應避免 service locator，也不建議 field injection，因為 constructor 能明確表達必要依賴並方便單元測試。

### DI Lifetime

| Lifetime | 建立時機與範圍 | 常見用途 |
|---|---|---|
| Transient | 每次解析都建立新 instance | 輕量、無狀態且不需共享的 service |
| Scoped | 每個 HTTP request 一個 instance | `DbContext`、request transaction、request-scoped service |
| Singleton | application lifetime 只有一個 instance | thread-safe cache、固定設定、無狀態 shared service |

`SystemInformationService` 使用 Singleton，因為它沒有 mutable request state，依賴的 `IOptions<ApplicationOptions>` 與 `IHostEnvironment` 都適合 application lifetime。UTC 時間是在每次方法呼叫時產生，不儲存在 singleton field。

常見錯誤是 Singleton 捕捉 Scoped service，例如未來把 EF Core `DbContext` 注入 Singleton。這會造成 lifetime mismatch、跨 request 共用非 thread-safe state 等問題。加入資料庫後，Employee service 通常會改用 Scoped。

**面試說法：** Lifetime 不是依照名稱猜，而是依 state ownership、thread safety 及依賴 lifetime 決定；長生命週期 service 不應直接依賴較短生命週期 service。

## `Task`、async／await 與 Thread

`Task` 表示非同步操作最終完成的承諾，不代表一定建立新 Thread。I/O 等待時，runtime 可以把 thread 還給 thread pool，完成訊號到達後再排程 continuation。

本週 service 沒有資料庫或網路 I/O，因此以 `Task.FromResult` 回傳已完成的 Task；Controller 使用 `await` 示範 Task-based API。這不會讓 CPU 工作變快，也不應為了看起來「非同步」而使用 `Task.Run`。

常見錯誤：

- ASP.NET Core action 使用 `async void`，導致 caller 無法等待或正確處理 exception。
- 呼叫 `.Result`／`.Wait()`，造成 thread blocking，並可能導致 deadlock 或 thread pool starvation。
- 每個方法都加 `async`，即使沒有任何可等待的操作。
- 把 `Task` 誤解為 Java Thread。

面試時可說：C# `Task` 比較接近 `CompletableFuture` 的 completion abstraction；async／await 是撰寫 continuation 的語法，不是 thread 建立語法。

## `CancellationToken`

ASP.NET Core 能將 request aborted signal 綁定到 action 的 `CancellationToken`。Controller 再把同一個 token 傳給 service；未來也應傳給 EF Core、HTTP client 或其他支援取消的非同步 API。

取消是 cooperative cancellation，不會強制殺掉 thread。被呼叫端必須觀察 token、呼叫 `ThrowIfCancellationRequested()`，或交給支援 token 的 API。

常見錯誤是 action 接受 token 後沒有往下傳，或把 `CancellationToken.None` 傳給資料庫，使 client 斷線後昂貴工作仍持續。

## Options Pattern 與設定載入

`WebApplication.CreateBuilder(args)` 依預設順序載入多個 configuration providers，後載入的來源可覆寫先前來源，常見順序包含：

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Development user secrets（若設定）
4. 環境變數
5. command-line arguments

本專案將 `Application` section 綁定成 `ApplicationOptions`。`ValidateDataAnnotations()` 驗證必填欄位，`ValidateOnStart()` 讓錯誤在啟動時發生，而不是第一次 request 才發生。

這類似 Spring Boot `@ConfigurationProperties` 搭配 Bean Validation。Options 的優點是避免散落 magic strings，並讓 service 依賴具型別設定。

環境變數用雙底線表示階層，例如 `Application__Name` 對應 `Application:Name`。Secret 不應提交到 JSON；未來可由 CI/CD、AWS Secrets Manager 或安全環境變數注入。

## Development 與 Production

`ASPNETCORE_ENVIRONMENT` 決定環境名稱。Development profile 會載入 `appsettings.Development.json`，Production 則載入 `appsettings.Production.json`；若未設定，ASP.NET Core 預設為 Production。

本專案只在 `app.Environment.IsDevelopment()` 時映射 OpenAPI JSON 與 Swagger UI，避免 Production 無條件暴露 API metadata。Production logging 也比 Development 保守。

`launchSettings.json` 是本機開發工具設定，不會自動部署到正式環境成為 production configuration。常見錯誤是依賴 launch profile 裡的環境變數，部署後才發現正式環境沒有設定。

對照 Spring：`application-dev.yml` 類似 `appsettings.Development.json`，Spring profile 類似 ASP.NET Core Environment；兩者命名與啟用方式不同，但都提供 environment-specific override。

## OpenAPI／Swagger

OpenAPI 是描述 HTTP API 的標準格式；Swagger UI 是讀取 OpenAPI 文件並提供互動頁面的工具，兩者不是同一件事。

.NET 10 使用 `AddOpenApi()` 與 `MapOpenApi()` 產生 `/openapi/v1.json`。Swagger UI 只提供 UI assets，設定它讀取該 JSON。這不同於舊教學常見的 `AddSwaggerGen()`／`UseSwagger()` 全套設定。

`/api/v1/system/info` 來自 Controller discovery；`/health` 則由 Health Check endpoint middleware 提供。Health Check 目前只代表 process 能處理 request，不代表資料庫健康，因為第一週尚未加入資料庫。

## GitHub Actions CI 與未來 AWS

.NET workflow 只在相關 path 變更時執行：

1. restore dependencies
2. Release build，warning 視為 error
3. run xUnit tests

這對照 Java workflow 的 `mvn verify`。CI 是每次變更的品質閘門；CD 才是把通過的 artifact 發布到環境。

第一週不建立 CD、Docker 或 AWS。程式先具備未來部署需要的 `/health`、Production config、環境變數 override 與無狀態 service。未來 AWS CD 應以 GitHub OIDC 取得短期 IAM credentials，避免長期 access key，並在估算成本及選定 Elastic Beanstalk／ECS 後再建立 IaC。

## 面試題與簡短參考答案

### 1. ASP.NET Core 的 DI Lifetime 有哪些？

Transient、Scoped、Singleton。Transient 每次解析建立、Scoped 通常每個 request 一個、Singleton 整個 application lifetime 共用。

### 2. 為什麼 Singleton 不能任意注入 Scoped service？

Singleton 可能把 Scoped instance 保留超過 request 範圍，造成跨 request 共用、資源未正確釋放及 thread-safety 問題。

### 3. Controller 與 Minimal API 的差異是什麼？

Controller 使用 MVC、attributes、filters 與 convention，適合較大型或規範化 API；Minimal API 以 route handlers 快速定義端點。兩者可共存，但本專案主要 API 使用 Controller。

### 4. Middleware 的執行順序為什麼重要？

Request 按註冊順序前進、response 反向返回；錯誤處理、安全性與 routing 元件放錯位置會讓功能失效或留下風險。

### 5. `async` 方法為什麼不應使用 `async void`？

呼叫端無法 await、取得結果或正常觀察 exception。事件處理器以外應回傳 `Task` 或 `Task<T>`。

### 6. `Task` 是否代表建立新 Thread？

不是。Task 表示操作完成狀態；非同步 I/O 可以在等待期間不占用 thread，只有特定排程方式才會使用額外 thread。

### 7. `appsettings.Development.json` 何時生效？

當 environment name 是 `Development` 時載入，並覆寫 base `appsettings.json` 的同名 keys。

### 8. `ValidateOnStart()` 的價值是什麼？

讓設定錯誤在 application startup 就失敗，符合 fail-fast，而不是等到第一次解析 Options 或處理 request 才出錯。

### 9. `[ApiController]` 提供什麼？

提供 API conventions，例如 attribute routing requirement、binding source inference 與自動 model validation error response；它本身不定義 endpoint path。

### 10. `CancellationToken` 如何在 Web API 中使用？

由 action 接收 request-aborted token，逐層傳給 service、database 或 HTTP calls；被呼叫端需 cooperative 地觀察取消訊號。

### 11. Solution 與 Project 有何差異？

Solution 組織 projects 與 build configuration；Project 才定義 framework、dependencies、source compilation 與 assembly output。

### 12. OpenAPI 與 Swagger UI 有何差異？

OpenAPI 是 machine-readable API specification；Swagger UI 是呈現並呼叫 OpenAPI operations 的互動工具。

### 13. Health Check 回傳 200 代表什麼？

只代表已註冊的 checks 目前為 Healthy。本週沒有 dependency checks，所以只能證明 API process 能回應，不能宣稱 PostgreSQL 或 AWS 正常。

### 14. CI 與 CD 的差異是什麼？

CI 自動驗證每次變更能 restore、build、test；CD 將已驗證 artifact 發布或部署到目標環境。第一週只實作 CI。

## 第一週面試總結說法

> 我保留原 Spring Boot 系統，另外建立 .NET 10 Controller Web API。Solution 內分成 API 與 xUnit projects，透過 ASP.NET Core 內建 DI 注入無狀態 Singleton service，使用 Options Pattern 驗證 application metadata，並提供 Controller endpoint、Health Check 與 Development-only OpenAPI／Swagger。GitHub Actions 會做 Release build 與 test，warnings 直接讓 CI 失敗；AWS deployment 則等資料庫、驗證與 container artifact 完成後再設計。
