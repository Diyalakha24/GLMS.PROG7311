# GLMS — Global Logistics Management System

A full-stack logistics management system built with **ASP.NET Core 8**, designed to manage clients, contracts, and service requests through a secure REST API and MVC web application.

GLMS follows a **layered, API-driven architecture** where the frontend and backend are independently deployable. The MVC application communicates with the backend exclusively through HTTP requests, while the API is responsible for data access, authentication, validation, and business rules.

---

## ✨ Key Features

* 🔐 **JWT-based authentication** with protected API endpoints
* 👥 **Client management** — create, update, view, and delete client records
* 📄 **Contract management** with lifecycle and status-transition rules
* 📎 **Signed agreement PDF validation**
* 🛠️ **Service request management** linked to active contracts
* 💱 **Automatic USD → ZAR currency conversion** using a live exchange rate
* 🛡️ **Business-rule validation** handled by the API
* 📚 **Swagger/OpenAPI documentation**
* 🧪 **Unit and integration testing** with NUnit, Moq, and `WebApplicationFactory`
* 🐳 **Docker and Docker Compose** support
* ⚙️ **GitHub Actions CI pipeline**
* 🗄️ **Entity Framework Core** with SQL Server and migrations

---

## 🏗️ Architecture

GLMS is divided into independently deployable frontend, backend, and testing projects.

```text
┌──────────────┐
│    Browser   │
└──────┬───────┘
       │ HTTP
       ▼
┌─────────────────────────┐
│      GLMSwebApp         │
│   ASP.NET Core MVC      │
│                         │
│   Razor Views           │
│   MVC Controllers       │
│   ApiService            │
│   Session-based JWT     │
└───────────┬─────────────┘
            │ REST API
            │ JWT Bearer
            ▼
┌─────────────────────────┐
│        GLMS.API         │
│    ASP.NET Core Web API │
│                         │
│ Controllers             │
│ Services                │
│ Repositories            │
│ EF Core / DbContext     │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│       SQL Server        │
└─────────────────────────┘
```

### Project Structure

| Project            | Responsibility                                                            |
| ------------------ | ------------------------------------------------------------------------- |
| **GLMS.API**       | REST API, authentication, business logic, validation, and database access |
| **GLMSwebApp**     | MVC frontend, UI rendering, session management, and API communication     |
| **GLMS.Web.Tests** | Unit and integration tests for business rules and API functionality       |

The frontend **does not have direct access to SQL Server**. All data operations are handled by `GLMS.API`.

---

## 📦 Domain Model

### Client

Represents a customer using the logistics service.

A client contains:

* Name
* Contact information
* Region
* Associated contracts

### Contract

Represents an agreement between GLMS and a client.

Contracts contain:

* Start and end dates
* Service level
* Status
* Optional signed-agreement PDF
* Associated client

Contracts follow a controlled lifecycle:

```text
                 ┌──────────┐
                 │  Draft   │
                 └────┬─────┘
                      │
              ┌───────┴────────┐
              ▼                ▼
        ┌──────────┐      ┌──────────┐
        │  Active  │◄────►│ On Hold  │
        └────┬─────┘      └────┬─────┘
             │                 │
             └────────┬────────┘
                      ▼
                ┌──────────┐
                │ Expired  │
                └──────────┘
```

`Expired` is a terminal state and cannot transition back to another status.

### Service Request

Service requests are raised against contracts and contain a cost recorded in USD.

When a request is created:

1. The contract is validated.
2. The contract must have an `Active` status.
3. The USD cost is converted to ZAR.
4. The converted value is stored with the request.
5. If the live exchange-rate service is unavailable, a fallback exchange rate is used.

---

## 🛠️ Technology Stack

### Backend

* **.NET 8**
* **ASP.NET Core Web API**
* **Entity Framework Core 8**
* **SQL Server**
* **JWT Bearer Authentication**
* **Swagger / Swashbuckle**

### Frontend

* **ASP.NET Core MVC**
* **Razor Views**
* **HttpClient**
* **Session-based token storage**

### Testing

* **NUnit**
* **Moq**
* **WebApplicationFactory**
* Unit testing
* API integration testing

### DevOps

* **Docker**
* **Docker Compose**
* **GitHub Actions**
* Automated build and test pipeline

---

## 🔐 Authentication & Security

The API uses **JWT Bearer authentication** to protect sensitive operations.

The authentication flow is:

```text
User
 │
 │ Login
 ▼
GLMSwebApp
 │
 │ POST /api/auth/login
 ▼
GLMS.API
 │
 │ JWT Token
 ▼
GLMSwebApp
 │
 │ Stores token in session
 ▼
Protected API Requests
 │
 │ Authorization: Bearer <token>
 ▼
GLMS.API
```

The MVC frontend automatically attaches the authenticated user's JWT to protected API requests.

> **Demo authentication:** The current implementation uses hardcoded credentials for coursework/demo purposes. A production deployment should replace this with ASP.NET Core Identity or another secure credential-management solution.

---

## 🌐 API Overview

All API endpoints are available under `/api`.

| Resource             | Endpoint                           | Description                          |
| -------------------- | ---------------------------------- | ------------------------------------ |
| **Authentication**   | `POST /api/auth/login`             | Authenticate a user and return a JWT |
| **Clients**          | `GET /api/clients`                 | Retrieve clients                     |
|                      | `GET /api/clients/{id}`            | Retrieve a specific client           |
|                      | `POST /api/clients`                | Create a client                      |
|                      | `PUT /api/clients/{id}`            | Update a client                      |
|                      | `PATCH /api/clients/{id}`          | Partially update a client            |
|                      | `DELETE /api/clients/{id}`         | Delete a client                      |
| **Contracts**        | `GET /api/contracts`               | Retrieve contracts                   |
|                      | `GET /api/contracts/{id}`          | Retrieve a specific contract         |
|                      | `POST /api/contracts`              | Create a contract                    |
|                      | `PUT /api/contracts/{id}`          | Update a contract                    |
|                      | `PATCH /api/contracts/{id}/status` | Change contract status               |
|                      | `DELETE /api/contracts/{id}`       | Delete a contract                    |
| **Service Requests** | `GET /api/servicerequests`         | Retrieve service requests            |
|                      | `GET /api/servicerequests/{id}`    | Retrieve a specific request          |
|                      | `POST /api/servicerequests`        | Create a service request             |
|                      | `DELETE /api/servicerequests/{id}` | Delete a service request             |

Contract listings can also be filtered by:

* `status`
* `startDate`
* `endDate`

### Swagger

When the API is running, interactive API documentation is available through Swagger:

```text
http://localhost:5000/swagger
```

---

## 💱 Currency Conversion

Service-request costs are entered in **USD** and converted to **ZAR** when the request is created.

The currency service:

* Retrieves a live exchange rate
* Converts the USD amount to ZAR
* Stores the converted value
* Uses a predefined fallback rate if the external exchange-rate service is unavailable

This approach demonstrates handling of **external API dependencies and graceful failure scenarios**.

---

## 🧪 Testing

The solution includes both **unit tests and API integration tests**.

Tests cover core business rules including:

* Currency conversion
* PDF validation
* Service-request eligibility
* Contract validation
* API endpoint behaviour

Integration tests use `WebApplicationFactory` to test the API in a realistic application environment.

Run the complete test suite with:

```bash
dotnet test GLMS.Web.sln
```

---

## 🐳 Running with Docker

Docker Compose can be used to run the complete application stack.

### Prerequisites

* Docker Desktop
* Docker Compose

### Start the application

```bash
docker compose up --build
```

This starts:

| Service       | URL                             |
| ------------- | ------------------------------- |
| MVC Frontend  | `http://localhost:5002`         |
| API / Swagger | `http://localhost:5000/swagger` |
| SQL Server    | `localhost:1433`                |

Database migrations are automatically applied when the API container starts.

---

## 💻 Running Locally

### 1. Configure SQL Server

Update the connection string in:

```text
GLMS.API/appsettings.Development.json
```

or:

```text
GLMS.API/appsettings.json
```

### 2. Apply database migrations

```bash
cd GLMS.API
dotnet ef database update
```

### 3. Start the API

```bash
dotnet run
```

### 4. Start the MVC frontend

Open a second terminal:

```bash
cd GLMSwebApp
dotnet run
```

Configure `ApiBaseUrl` so that the MVC application points to the running API.

The application URL will be displayed in the terminal when the frontend starts.

---

## ⚙️ Configuration

The main configuration values are:

| Setting                               | Purpose                               |
| ------------------------------------- | ------------------------------------- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string          |
| `Jwt:Key`                             | JWT signing key                       |
| `Jwt:Issuer`                          | JWT token issuer                      |
| `Jwt:Audience`                        | JWT token audience                    |
| `ApiBaseUrl`                          | API base URL used by the MVC frontend |

When using Docker Compose, these values are supplied through environment variables.

> **Security note:** Demo JWT keys and SQL Server credentials should be replaced with secure secrets before any production deployment.

---

## 🔄 CI/CD

The project includes a **GitHub Actions** workflow located at:

```text
.github/workflows/dotnet.yml
```

The pipeline runs when changes are pushed or pull requests are opened against:

* `master`
* `Part-3`

### Pipeline checks

```text
Code Push / Pull Request
          │
          ▼
       Restore
          │
          ▼
        Build
          │
          ▼
     Run Tests
          │
          ▼
 Docker Image Validation
    (Part-3 branch)
```

This provides automated verification that the application builds successfully, tests pass, and the Docker images can be created.

---

## 📁 Project Structure

```text
GLMS/
│
├── GLMS.API/
│   ├── Controllers/
│   │   ├── AuthController
│   │   ├── ClientsController
│   │   ├── ContractsController
│   │   └── ServiceRequestsController
│   │
│   ├── Data/
│   │   └── DbContext
│   │
│   ├── Migrations/
│   │
│   ├── Models/
│   │   ├── Entities
│   │   └── DTOs
│   │
│   ├── Repositories/
│   │
│   └── Services/
│       ├── ContractValidationService
│       └── CurrencyService
│
├── GLMSwebApp/
│   ├── Controllers/
│   ├── Views/
│   │   ├── Auth
│   │   ├── Clients
│   │   ├── Contracts
│   │   ├── ServiceRequests
│   │   └── Home
│   │
│   ├── Services/
│   │   └── ApiService
│   │
│   └── wwwroot/
│
├── GLMS.Web.Tests/
│
├── .github/
│   └── workflows/
│       └── dotnet.yml
│
├── docker-compose.yml
└── GLMS.Web.sln
```

---

## 🎯 Engineering Highlights

GLMS demonstrates practical software-engineering concepts beyond basic CRUD functionality:

* **API-first architecture** with clear separation between frontend and backend
* **Separation of concerns** using controllers, services, repositories, and DTOs
* **RESTful API design**
* **JWT authentication and authorization**
* **Entity Framework Core and relational database design**
* **Business-rule enforcement at the API layer**
* **External API integration**
* **Fallback handling for external-service failures**
* **Automated testing**
* **Integration testing**
* **Containerisation with Docker**
* **CI/CD with GitHub Actions**
* **Database migrations**
* **Configuration through environment variables**

---

## 🚀 Future Improvements

Potential improvements for a production-ready version include:

* Replace demo authentication with **ASP.NET Core Identity**
* Implement role-based authorization
* Store secrets using a secure secret-management service
* Add structured application logging
* Add centralized exception handling
* Add refresh-token support
* Expand API test coverage
* Add automated deployment to a cloud environment
* Introduce monitoring and health checks

---

## 👤 Author

**Diya Lakha**

Bachelor of Computer and Information Sciences
Application Development

This project was developed as part of my application-development coursework and demonstrates full-stack development, REST API design, database integration, authentication, testing, containerisation, and CI/CD practices.
