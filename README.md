# TalentIQ Platform

TalentIQ is an AI-powered Talent Intelligence Platform designed to help organizations manage their workforce, discover employee skills, recommend the best talent for projects, and support workforce planning through AI-driven insights.

The platform follows a Hybrid Multi-Tenancy Architecture that supports both Enterprise customers with dedicated databases and Starter/Business customers using a shared database with logical tenant isolation.

---

## Features

- Hybrid Multi-Tenancy Architecture
- Organization Management
- Identity & Access Management
- Talent Management
- Project Management
- Integration with external systems
- AI-powered Skill Extraction & Recommendations
- Internal Mobility & Workforce Planning

---

## Architecture

TalentIQ follows **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** principles.

```
Presentation
│
├── API
│
Application
│
Domain
│
Infrastructure
│
Persistence
```

The application is divided into seven independent bounded contexts (modules).

---

## Modules

### Organization

- Organizations
- Departments
- Teams

### Identity & Access

- Users
- Roles
- Permissions
- Authentication & Authorization

### Talent Management

- Employees
- Talent Profiles
- Skills
- Employee Skills
- Skill Evidence
- Certifications

### Project Management

- Projects
- Tasks
- Project Assignments

### Integration

- Jira
- Azure DevOps
- GitHub
- Asana
- Trello
- CSV Import

### AI & Recommendations

- Skill Extraction
- Recommendation Engine
- Team Formation
- Skill Gap Analysis
- Hidden Talent Discovery

### Internal Mobility

- Internal Opportunities
- Mobility Matching
- Succession Planning
- Resource Allocation

---

## Hybrid Multi-Tenancy

### Enterprise Tier

- Dedicated PostgreSQL Database
- Physical Tenant Isolation
- Independent Backup & Restore
- Dedicated Resources

### Starter / Business Tier

- Shared PostgreSQL Cluster
- Logical Isolation using `org_id`
- Row-Level Security (Optional)
- Shared Infrastructure

---

## Tech Stack

### Backend

- ASP.NET Core
- .NET 9
- Entity Framework Core
- PostgreSQL
- MediatR
- FluentValidation
- AutoMapper
- JWT Authentication

### Architecture

- Clean Architecture
- CQRS
- Repository Pattern
- Unit of Work
- Domain-Driven Design (DDD)

### Database

- PostgreSQL 16

### AI

- Recommendation Engine
- Skill Extraction
- AI Copilot

---

## Database

- 7 Modules
- 37 Tables
- Global Organization Registry
- PostgreSQL 16

---

## Project Structure

```
src
│
├── Organization Module
├── Identity Module
├── Talent Management Module
├── Project Management Module
├── WorkFit.Infrastructure
├── WorkFit.SharedKernel
└── WorkFit.Host
```

---

## Getting Started

### Clone the repository

```bash
git clone https://github.com/your-username/TalentIQ.git
```

### Restore packages

```bash
dotnet restore
```

### Build

```bash
dotnet build
```

### Apply Migrations

```bash
dotnet ef database update
```

### Run

```bash
dotnet run
```

---

## Design Principles

- Clean Architecture
- SOLID Principles
- Dependency Injection
- CQRS
- MediatR
- Domain Events
- Modular Monolith
- Hybrid Multi-Tenancy

---

## Future Enhancements

- AI Copilot
- Advanced Workforce Analytics
- Predictive Skill Gap Analysis
- Smart Team Formation
- Real-time Integrations
- AI-powered Career Path Recommendations

---

## Authors

TalentIQ Development Team

---

## License

This project is intended for educational and enterprise research purposes.
