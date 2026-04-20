# Employee Management System

A full-stack employee management system built with Spring Boot, React, and PostgreSQL, deployed on AWS (Elastic Beanstalk + Amplify + S3 + CloudFront).

## Tech Stack

**Backend**
- Java 17 + Spring Boot 3
- Spring Security 6 (JWT-based authentication)
- Spring Data JPA + JDBC
- PostgreSQL
- Flyway (database migrations)
- AWS S3 (profile image storage)
- Docker + AWS Elastic Beanstalk

**Frontend**
- React 18 + Vite
- Chakra UI
- Formik + Yup (form validation)
- React Router v6
- AWS Amplify + CloudFront

## Features

- Employee CRUD operations (create, read, update, delete)
- JWT authentication and authorization
- Profile image upload via AWS S3
- Responsive UI with green color theme
- CI/CD via GitHub Actions

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/v1/employees` | List all employees |
| GET | `/api/v1/employees/{id}` | Get employee by ID |
| POST | `/api/v1/employees` | Create new employee |
| PUT | `/api/v1/employees/{id}` | Update employee |
| DELETE | `/api/v1/employees/{id}` | Delete employee |
| POST | `/api/v1/employees/{id}/profile-image` | Upload profile image |
| GET | `/api/v1/employees/{id}/profile-image` | Get profile image |
| POST | `/api/v1/auth/login` | Login |

## Getting Started

### Prerequisites
- Java 17+
- Node.js 18+
- Docker + Docker Compose
- Maven

### Running Locally

Start the database:
```bash
docker compose up -d
```

Start the backend:
```bash
cd backend
mvn spring-boot:run
```

Start the frontend:
```bash
cd frontend/react
npm install
npm run dev
```
