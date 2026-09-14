# 第二週學習筆記：Employee CRUD 與 PostgreSQL

## Request 從 React 到 PostgreSQL 的流程

```text
React
  → EmployeesController
  → IEmployeeService / EmployeeService
  → AppDbContext / EF Core
  → PostgreSQL
```

`EmployeesController` 負責 HTTP：route、model binding、status code 與 response。`EmployeeService` 負責 Email 正規化、重複檢查、密碼雜湊及找不到員工等應用規則。`AppDbContext` 負責 Entity mapping、查詢追蹤與資料庫寫入；Entity 則描述持久化資料。

## `ControllerBase` 與 `[ApiController]`

`ControllerBase` 提供 `Ok()`、`CreatedAtAction()`、`NoContent()` 等 Web API helper，但不包含 MVC View 功能。`[ApiController]` 啟用 API conventions，包括 binding source inference 與 ModelState 無效時自動回傳 400。

本專案使用 Attribute Routing：Controller 的 `[Route("api/v1/employees")]` 是共同 prefix，action 再以 `[HttpGet]`、`[HttpGet("{id:int}")]` 等 attributes 定義 HTTP method 與路徑。`{id:int}` 同時限制 route value 必須可解析為整數。

## Model Binding 與 Model Validation

ASP.NET Core 會把 JSON body 綁定成 `CreateEmployeeRequest` 或 `UpdateEmployeeRequest`，把 route 中的 `id` 綁定成 action parameter。DTO 上的 `Required`、`EmailAddress`、`StringLength`、`Range` 與 `EnumDataType` 會在 Controller 執行前驗證。

驗證失敗時 `[ApiController]` 自動回傳 `400 ValidationProblemDetails`。例如 Email 格式錯誤、年齡小於 16、密碼少於 8 字元，以及無法解析的 Gender 都不會進入 Service。

## DTO 為何不直接使用 Entity

Entity 是資料庫模型，包含 `PasswordHash` 等持久化細節；API 契約則只應包含 caller 能輸入或讀取的欄位。若直接輸出 Entity，容易意外暴露密碼雜湊，也會讓資料表調整直接破壞前端契約。

- `CreateEmployeeRequest`：建立所需欄位，包含原始密碼。
- `UpdateEmployeeRequest`：本週可修改 name、email、age。
- `EmployeeResponse`：回傳安全的員工資料，不包含密碼。
- `Employee`：只在 Service 與 EF Core 資料層內使用。

## DI Lifetime

`EmployeeService` 與 `AppDbContext` 都註冊為 Scoped，也就是每個 HTTP request 取得一組 instances。EF Core `DbContext` 會追蹤一次工作單元中的 Entity，且本身不是 thread-safe，因此很適合 request scope，不應註冊為 Singleton。

- Transient：每次解析都建立，適合輕量、無狀態服務。
- Scoped：每個 HTTP request 一份，適合 `DbContext` 與 request transaction。
- Singleton：整個 application 共用，必須 thread-safe，也不能捕捉 Scoped dependency。

## HTTP Status Code 與 `CreatedAtAction`

| Status | 本專案使用時機 |
|---|---|
| 200 OK | GET 成功、PUT 成功並回傳更新結果 |
| 201 Created | POST 成功建立 Employee |
| 204 No Content | DELETE 成功且不需 response body |
| 400 Bad Request | JSON、型別或 Data Annotations 驗證失敗 |
| 404 Not Found | 指定 Employee id 不存在 |
| 409 Conflict | Email 已被其他 Employee 使用 |

POST 使用 `CreatedAtAction(nameof(GetById), ...)`，除了回傳 201，也會產生指向新資源的 `Location` header。這比只回傳 200 更精確地表達「新資源已建立」。

## EF Core 與 PostgreSQL

`AppDbContext` 透過 Npgsql provider 操作 PostgreSQL。Email 在寫入前會 trim 並轉為小寫，資料庫另有 unique index 防止 concurrency race。Service 先做可讀性較好的存在性檢查，仍會攔截 PostgreSQL unique violation 並轉成 409。

查詢使用 async API 並傳遞 request 的 `CancellationToken`。純讀取查詢使用 `AsNoTracking()`，避免不必要的 change tracking。

Migration 是可版本控制的 schema 變更。本週的 `InitialEmployeeSchema` 建立 employee table 與唯一索引；本機可用以下指令套用：

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project src/EmployeeManagement.Api \
  --startup-project src/EmployeeManagement.Api
```

## CORS

CORS 是瀏覽器對跨來源 request 的安全機制。React 的 `http://localhost:5173` 與 API 的 `http://localhost:5180` port 不同，因此是不同 origin，需要 API 明確允許。

Development 設定只允許 `http://localhost:5173`，Production 預設沒有允許來源。正式環境應列出實際前端 domain，不應任意使用 `AllowAnyOrigin()`，尤其當未來加入 credentials 或敏感資料後。

## OpenAPI

Controller actions、DTO 與 `ProducesResponseType` 會成為 OpenAPI 文件的一部分。Development 可在 `/swagger` 操作 API，並從 `/openapi/v1.json` 查看 machine-readable contract。

## 週驗收回答重點

1. Controller 管理 HTTP 契約，Service 管理應用規則，DbContext 管理資料存取。
2. DTO 隔離外部契約與資料庫 Entity，避免 over-posting 與敏感欄位外洩。
3. Scoped 對應一次 request 的工作單元，符合 `DbContext` 的生命週期與 thread-safety 限制。
4. 201 表示建立資源、204 表示成功但沒有 body、409 表示 request 與現有資源狀態衝突。
5. CORS 是瀏覽器執行的跨來源限制；允許來源應採 allowlist。
6. React 的 `.env.dotnet` 以 `VITE_API_BASE_URL` 指向 .NET API，CRUD 寫入 PostgreSQL，不使用瀏覽器記憶體假資料。
