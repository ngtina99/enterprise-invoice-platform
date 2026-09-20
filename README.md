# Invoice Management Web Application

A full-stack invoice management portfolio project built with ASP.NET Core, Vue 3, Entity Framework Core and SQLite. Invoices and status changes are saved in SQLite and remain available after restarting the application.

## Overview

This is a small invoice management application that demonstrates a complete workflow from the frontend to the backend and database.

Users can:
- View supplier invoices.
- Create new invoices.
- Approve or reject invoices.
- See validation and error messages.
- Retrieve saved invoices after restarting the application.

This is an educational portfolio project, not a production financial system.

## Technology Stack

### Backend
- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQLite

### Frontend
- Vue 3
- JavaScript
- Vite
- Fetch API

## Architecture

```text
Vue 3 frontend
      |
      | HTTP / JSON
      v
ASP.NET Core REST API
      |
      | Entity Framework Core
      v
SQLite database
```

During local development, Vite proxies requests beginning with `/api` to the ASP.NET Core backend.

## Features

### Invoice creation

Users can create an invoice by providing:
- Invoice number
- Supplier name
- Amount
- Currency

New invoices start with the `Pending` status.

### Invoice listing

The frontend retrieves invoices from the REST API and displays them in a table.

### Status updates

Users can change an invoice's status to:
- Pending
- Approved
- Rejected

The current interface provides Approve and Reject buttons.

### Validation and error handling

The API uses DTO validation and returns appropriate HTTP responses for invalid requests and missing invoices.

The frontend displays loading, success, and error states.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/invoices` | Retrieve all invoices |
| GET | `/api/invoices/{id}` | Retrieve an invoice by ID |
| POST | `/api/invoices` | Create an invoice |
| PATCH | `/api/invoices/{id}/status` | Update invoice status |

## Running Locally

### Requirements

- .NET SDK compatible with the backend project
- Node.js and npm

### 1. Start the backend

```bash
cd backend/FinanceFlow.Api
dotnet restore
dotnet ef database update
dotnet run
```

The API is configured for local development at:

`http://localhost:5212`

The EF Core CLI must be installed to run `dotnet ef`.

### 2. Start the frontend

Open a second terminal:

```bash
cd frontend
npm install
npm run dev
```

Open the URL displayed by Vite, usually:

`http://localhost:5173`

Both applications must be running for the complete workflow.

### Frontend production build

```bash
cd frontend
npm run build
```

## Testing

The following workflows were tested manually:

- Invoice creation
- Invoice listing
- Invoice approval and rejection
- Persistence after backend restart
- Invalid input handling
- Missing invoice responses
- Backend connection errors
- Frontend production build

## Current Scope and Limitations

- SQLite is used.
- Authentication and user roles are not implemented.
- The application has no real SAP or other enterprise-system integration.
- Status changes use simplified business rules.
- Automated tests and production deployment are not yet implemented.

## Possible Future Improvements

- SQL Server integration
- Authentication and role-based authorization
- Automated backend and frontend tests
- Invoice search and filtering
- More detailed approval workflows
- Mock integrations with external enterprise systems

## Purpose

This project was developed to practice full-stack development, REST API design, database persistence, frontend/backend integration, validation and debugging.
