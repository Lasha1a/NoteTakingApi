📝 NoteTaking API

A Note Taking REST API built with .NET 8/9, Minimal APIs, and Vertical Slice Architecture.
The project supports authentication with JWT, note management with tags, soft deletion, logging, and health checks.

🚀 Tech Stack

.NET 8 / .NET 9

Minimal APIs

Vertical Slice Architecture

PostgreSQL

Entity Framework Core

JWT Authentication + Refresh Tokens

Serilog (Console + File logging)

Docker & Docker Compose

Scalar (OpenAPI UI)

🏗 Architecture Overview

This project follows Vertical Slice Architecture, where:

Each feature lives in its own file

One endpoint = one file = one responsibility

No controllers

No bloated service layers


🐳 Running the Project (Docker)
1️⃣ Start PostgreSQL with Docker

From the solution root (where docker-compose.yml is):

docker compose up -d


This starts PostgreSQL in a container.

⚠️ If PostgreSQL is installed locally, make sure the Windows PostgreSQL service is stopped to avoid port conflicts.

2️⃣ Apply Database Migrations

After the database container is running:

dotnet ef database update


This will:

Create all tables

Apply relationships

Prepare the database for the application

3️⃣ Run the API
dotnet run


The API will start and expose:

Scalar UI
👉 https://localhost:{port}/scalar/v1
