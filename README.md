# 員工管理系統

這是一個全端員工管理系統，使用 Spring Boot、React 和 PostgreSQL 建置，並部署在 AWS 上（Elastic Beanstalk + Amplify + S3 + CloudFront）。

## 面試官快速入口

這個 repository 展示同一套員工管理需求的 Java／Spring Boot 原始實作，以及後續以 C#／ASP.NET Core 進行的重構：

- ☕ **Java／Spring Boot**：本頁即為主要版本，包含 Spring Security、PostgreSQL、Flyway、AWS 與 CI/CD。[查看 Backend 原始碼](backend/)
- 🔷 **C#／ASP.NET Core**：獨立重構版本，包含 EF Core、PostgreSQL、JWT、xUnit 與 React 串接。[查看 .NET Side Project](https://github.com/jackypao103098/employee-management-system/tree/dotnet-interview-v4/employee-management-dotnet)

.NET 連結指向已驗證的固定快照；日常開發仍會繼續在各分支進行。

> 本專案以 [Amigoscode Spring Boot Fullstack course](https://www.amigoscode.com) 為基礎，後續延伸加入 CI/CD 流程、AWS 雲端部署，以及實際部署時遇到的正式環境問題修復。

## 線上展示

🔗 https://main.d1zjxnolctj65r.amplifyapp.com

> **展示模式：** 為了控制 AWS 成本，線上網站預設不連接後端服務。
> 登入與員工 CRUD 皆可直接操作，資料只會儲存在目前瀏覽器，
> 不會寫入正式資料庫。真實 AWS 後端環境會在需要展示完整架構時啟動。

**Demo 帳號**（僅適用於上方的離線展示模式）
- Email：`demo@jackypao.com`
- Password：`password`

> 展示模式不會呼叫後端 API，此帳號只存在瀏覽器中。.NET 版本的 API 另有強密碼規則：新密碼須至少 12 字元，並包含英文大小寫、數字與特殊字元。

## 畫面截圖

![登入頁面](docs/login.png)
![Dashboard 頁面](docs/dashboard.png)

## 技術棧

**後端**
- Java 17 + Spring Boot 3.4
- Spring Security 6（以 JWT 為基礎的身分驗證）
- Spring Data JPA + JDBC
- PostgreSQL
- Flyway（資料庫 migration）
- AWS S3（員工大頭照儲存）
- Docker + AWS Elastic Beanstalk

**前端**
- React 18 + TypeScript + Vite
- Chakra UI
- Formik + Yup（表單驗證）
- React Router v6
- AWS Amplify + CloudFront

## 功能

- 員工 CRUD 操作（新增、查詢、更新、刪除）
- JWT 身分驗證與授權
- 透過 AWS S3 上傳員工大頭照
- 響應式 UI，使用綠色系主題
- 使用 GitHub Actions 建立 CI/CD 流程

## API 端點

| Method | Path | 說明 |
|--------|------|-------------|
| GET | `/api/v1/employees` | 取得所有員工 |
| GET | `/api/v1/employees/{id}` | 依 ID 取得員工 |
| POST | `/api/v1/employees` | 建立新員工 |
| PUT | `/api/v1/employees/{id}` | 更新員工資料 |
| DELETE | `/api/v1/employees/{id}` | 刪除員工 |
| POST | `/api/v1/employees/{id}/profile-image` | 上傳員工大頭照 |
| GET | `/api/v1/employees/{id}/profile-image` | 取得員工大頭照 |
| POST | `/api/v1/auth/login` | 登入 |

## 快速開始

### 前置需求
- Java 17+
- Node.js 18+
- Docker + Docker Compose
- Maven

### 本機執行

啟動資料庫：
```bash
docker compose up -d
```

啟動後端：
```bash
cd backend
mvn spring-boot:run
```

啟動前端：
```bash
cd frontend/react
npm install
npm run dev
```

### 前端展示模式

前端支援不連接後端、RDS 與 S3 的展示模式。登入、員工 CRUD
與小型頭像上傳會改用瀏覽器 `localStorage`，適合在 AWS 後端環境
關閉時持續提供面試展示。

`frontend/react/.env` 預設啟用展示模式：

```dotenv
VITE_API_MODE=demo
```

展示帳號：

```text
Email: demo@jackypao.com
Password: password
```

需要連接 Spring Boot 後端時，於建置環境設定：

```dotenv
VITE_API_MODE=real
VITE_API_BASE_URL=https://your-api.example.com
```

Amplify Console 中設定的環境變數會覆蓋 `.env`，因此可讓不同 branch
分別使用 `demo` 與 `real` 模式。
