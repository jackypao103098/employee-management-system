# 第四週學習筆記：JWT Authentication 與 Authorization

## 完成範圍

- 登入 API
- PBKDF2-SHA512 密碼雜湊與驗證
- JWT 產生、簽章及驗證
- Employee ID、Email、Role Claims
- Role Authorization 與 Policy Authorization
- Employee 只能修改自己
- 401／403 語意
- 登入成功與失敗紀錄
- JWT Secret 外部注入
- React 登入、Token 傳遞及依權限顯示 UI

## Authentication 與 Authorization

Authentication 回答「你是誰」，Authorization 回答「你可以做什麼」。登入成功只代表身分已驗證，不代表可以操作所有 Employee。

```text
Email + Password
  → 驗證 PBKDF2 hash
  → 簽發 JWT
  → React 傳送 Authorization: Bearer <token>
  → API 驗證簽章、Issuer、Audience、Expiration
  → Role／Policy 判斷是否允許操作
```

## JWT Claims

Token 只放授權判斷需要的資料：

- `sub`：資料庫中的 Employee ID
- `email`：登入帳號
- `role`：由資料庫讀取的 `ADMIN` 或 `EMPLOYEE`
- `jti`：Token 唯一識別碼
- `exp`、`nbf`、`iss`、`aud`：有效期及簽發範圍

API 不接受 Create／Update request 傳入 Role，也不拿 request body 的 Employee ID 判斷本人。Role 與本人 ID 都來自簽章驗證成功的 JWT。

## 權限矩陣

| 操作 | 未登入 | Employee | Admin |
|---|---:|---:|---:|
| 登入 | 允許 | 允許 | 允許 |
| 建立 Employee | 401 | 403 | 允許 |
| 查看 Employee | 401 | 允許 | 允許 |
| 修改自己 | 401 | 允許 | 允許 |
| 修改別人 | 401 | 403 | 允許 |
| 刪除 Employee | 401 | 403 | 允許 |

`[Authorize(Roles = "ADMIN")]` 是 Role Authorization。修改 Employee 則是 resource-based Policy Authorization：handler 同時收到已驗證的 User claims 與路由 Employee ID，再判斷 `sub == employeeId` 或是否為 Admin。

## 401 與 403

- `401 Unauthorized`：沒有 Token、Token 無效／過期，或登入密碼錯誤。實際語意是尚未通過 Authentication。
- `403 Forbidden`：Token 有效、身分已知，但 Role 或資源擁有權不足。

如果 Employee 修改別人時回 404 或由前端自行判斷，容易產生不一致或被繞過。本專案由 API Policy 固定回 403。

## 密碼安全

新密碼至少 12 字元，且必須同時包含英文大小寫、數字與特殊字元。雜湊使用隨機 16-byte salt、PBKDF2-HMAC-SHA512、210,000 iterations 與 32-byte derived key。驗證使用 `CryptographicOperations.FixedTimeEquals()`。既有 PBKDF2-SHA256 hash 仍可登入，但成功後會自動換成目前格式。

資料庫只保存 `PasswordHash`。DTO、API response 與 Log 都不輸出密碼或 hash。

## JWT 驗證底線

JWT Bearer middleware 明確啟用：

- `ValidateIssuerSigningKey`
- `ValidateIssuer`
- `ValidateAudience`
- `ValidateLifetime`
- `RequireExpirationTime`
- `RequireSignedTokens`

演算法使用 HMAC-SHA256。JWT payload 可以被使用者讀取，所以不能放密碼或秘密；安全性來自簽章防竄改，不是內容加密。

## Secret 與 Admin Bootstrap

`appsettings*.json` 只包含非秘密的 Issuer、Audience 與有效時間。API 沒有 `Jwt__Secret` 或長度少於 32 字元時會拒絕啟動。

本機設定：

```bash
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -hex 32)" \
  --project src/EmployeeManagement.Api
```

Seed Data 不包含可預測的 Admin 密碼。bootstrap Admin 時，由環境變數提供既有 Employee Email 與符合強度規則的密碼；API 只保存 hash。已是 Admin 時會重設密碼，可用來替換舊弱密碼。完成後立刻移除 bootstrap 變數，避免下次啟動再次重設。

## 安全日誌

成功登入只記錄 Employee ID 與 Role。失敗登入只記錄 Email 的短 SHA-256 fingerprint，不記錄原始 Email、密碼、完整 Token 或資料庫 hash。ASP.NET Core 與 EF Core 的 sensitive data logging 沒有開啟。

## React

真實 API client 使用 Axios request interceptor 集中附加 Bearer Token，避免每個 API function 重複處理。Token 保存在 `sessionStorage`，關閉分頁後即消失；API 回 401 時會清除 Token 並回登入頁。

React 解碼 JWT 只用於顯示 Email、Role，以及隱藏無權限的按鈕。`jwt-decode` 不驗證簽章，所以前端資訊不能作為安全依據；所有授權仍由後端執行。

## 驗收重點

```bash
dotnet test --configuration Release
cd ../frontend/react
npm run typecheck
npm run build -- --mode dotnet
```

必須確認：

- 正確登入為 200，錯誤密碼為 401
- 無 Token／偽造／過期／錯誤 Audience Token 為 401
- Employee 修改自己為 200，修改他人為 403
- Employee 刪除為 403，Admin 刪除為 204
- Repository 搜尋不到 JWT Secret
- Database password 欄位全部為 PBKDF2 格式
- 登入 Log 沒有密碼、完整 Token 或原始 Email
