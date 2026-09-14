# 第三週學習筆記：EF Core 查詢與穩定分頁

## 完成範圍

- EF Core 10 與 Npgsql PostgreSQL provider
- `AppDbContext` 與 Employee Fluent API mapping
- Initial Migration 與第三週增量 Migration
- Async CRUD 與 `CancellationToken`
- Email unique index
- 姓名關鍵字搜尋
- 部門篩選
- 到職日期排序
- 穩定分頁
- `AsNoTracking()`
- 直接 Projection 成 DTO
- Demo Seed Data

## `AppDbContext`

`AppDbContext` 是 EF Core 的工作單元，透過 `DbSet<Employee>` 查詢及寫入 Employee。它以 Scoped lifetime 註冊，每個 HTTP request 使用一個 context：

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

Fluent API 集中定義 table、column、長度、required、enum conversion、default value 與 indexes，避免資料庫規則散落在 Controller 或 Service。

## Migration 策略

`InitialEmployeeSchema` 保留最初建立 employee table 的歷史；第三週的 `AddEmployeeQueryFieldsAndSeedData` 是增量 Migration，加入：

- `department`
- `hire_date`
- 部門查詢索引
- `(hire_date, id)` 排序索引
- 三筆 Demo Seed Data

不應修改已經套用到共享環境的舊 Migration。Model 改變後應新增下一個 Migration，讓每個環境依相同順序升級。

## 查詢契約

```http
GET /api/v1/employees
    ?name=ada
    &department=engineering
    &sortDirection=asc
    &page=1
    &pageSize=10
```

`EmployeeQueryRequest` 由 Model Binding 從 Query String 建立，並限制 `page` 至少為 1、`pageSize` 必須介於 1～100。

## 組合 `IQueryable`

EF Core query 在 `ToListAsync()` 前只是 expression tree，可以依條件逐步組合：

```csharp
var employees = dbContext.Employees.AsNoTracking();

if (!string.IsNullOrWhiteSpace(query.Name))
{
    employees = employees.Where(...);
}

if (!string.IsNullOrWhiteSpace(query.Department))
{
    employees = employees.Where(...);
}
```

最後呼叫 `ToListAsync()` 時才送出一條 SQL，而不是每加一個 `Where()` 就查一次資料庫。

## 穩定排序與分頁

只用到職日期排序並不穩定，因為多名員工可能有相同日期。PostgreSQL 對相同排序值沒有義務維持固定順序，可能造成資料跨頁重複或遺漏。

本專案加入唯一的 ID 作為第二排序鍵：

```csharp
employees
    .OrderBy(employee => employee.HireDate)
    .ThenBy(employee => employee.Id);
```

再套用：

```csharp
.Skip((query.Page - 1) * query.PageSize)
.Take(query.PageSize)
```

這是 offset pagination；在同一資料集下具有確定順序。若未來面對大量資料與頻繁新增，則可進一步改用 `(hireDate, id)` cursor 的 keyset pagination。

## `AsNoTracking()`

列表及單筆查詢只讀不改，因此使用 `AsNoTracking()`：

```csharp
dbContext.Employees.AsNoTracking();
```

EF Core 不需建立 change tracking snapshot，可減少記憶體與追蹤成本。Update／Delete 仍查詢 tracked Entity，才能由 `SaveChangesAsync()` 偵測修改或刪除。

## 直接 Projection 成 DTO

唯讀查詢在 SQL 階段直接選成 `EmployeeResponse`：

```csharp
.Select(employee => new EmployeeResponse(
    employee.Id,
    employee.Name,
    employee.Email,
    employee.Age,
    employee.Gender,
    employee.Department,
    employee.HireDate))
```

因此 SQL 不會讀取 Response 不需要的 `password` 欄位，也不會先 materialize 完整 Entity 再轉換，能兼顧安全與效率。

## Demo Seed Data

`HasData()` 定義 Ada Lovelace、Grace Hopper 與 Peter Drucker 三筆展示資料。Seed 使用固定負數 ID，避免占用 PostgreSQL 正數 identity sequence，也使 Migration 可重複部署並可在 Down migration 精確刪除。

Seed Data 屬於 schema-controlled reference/demo data；大量、動態或環境專屬測試資料則不適合全部放進 `HasData()`。

## `CancellationToken`

Controller 接收 request aborted token，Service 再傳給 `ToListAsync()`、`SingleOrDefaultAsync()` 與 `SaveChangesAsync()`。Client 中斷連線時，資料庫操作可以合作式取消，避免無意義地繼續占用資源。

## 驗收方式

```bash
dotnet build --configuration Release --warnaserror
dotnet test --configuration Release --no-build
```

啟動 PostgreSQL 並套用 Migration：

```bash
docker compose up -d --wait
dotnet tool run dotnet-ef database update \
  --project src/EmployeeManagement.Api \
  --startup-project src/EmployeeManagement.Api
```

驗證查詢：

```bash
curl 'http://localhost:5180/api/v1/employees?name=ada&department=engineering&sortDirection=asc&page=1&pageSize=10'
```
