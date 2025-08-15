[← Back to README](../../README.md)

# Setting up development environment
This guide will help you set up your development environment for Choruko.

> **Note:** The commands in this guide use forward slashes (/) which work in standard terminal for Linux and macOS. Windows users should use PowerShell which also supports forward slashes, rather than Command Prompt.

## Prerequisites
- [Docker](https://docs.docker.com/get-docker/)
- [Node.js](https://nodejs.org/en/download/)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Python 3](https://www.python.org/downloads/)
- [Git](https://git-scm.com/downloads)

## Getting Started
1. Clone the repository to your local machine:
    
    ```bash
    git clone https://github.com/psampir/choruko.git
    cd choruko
    ```

## Backend Setup
The backend is an ASP.NET Core application.

1. **Configure launch settings:**
  Navigate to the `backend/ChorukoAPI/Properties` directory.
  Create a copy of `launchSettings.example.json` and rename it to `launchSettings.json`. Adjust environment variables as needed.

2. **Database Migrations:**
  The development database runs in a Docker container, defined in `docker/compose/dev/docker-compose.yml`. Before you can run the backend, you need to apply the database migrations.

    First, start the database container:
      
    ```bash
    docker-compose -f docker/compose/dev/docker-compose.yml up -d
    ```

    Then, install `dotnet-ef` tools:

    ```bash
    dotnet tool restore --tool-manifest backend/ChorukoAPI/.config/dotnet-tools.json
    ```

    Finally, apply the migrations:

    ```bash
    python3 backend/update-db.py
    ```

    > Keeping migrations up to date:
    > - After any change to your EF Core models (entities/DbContext), you must create a new migration and apply it.
    > - Create a migration (replace DescriptiveName):
    >   ```bash
    >   dotnet ef migrations add DescriptiveName --project backend/ChorukoAPI --startup-project backend/ChorukoAPI
    >   ```
    > - Then apply it:
    >   ```bash
    >   python3 backend/update-db.py
    >   ```
    > - If you only edited a migration file that has NOT yet been applied to your dev DB, just rerun `python3 backend/update-db.py`.
    > - If a migration was already applied and needs changes, add a new migration instead of editing the old one.

## Frontend Setup
The frontend is a React application built with Vite.

1. **Install dependencies:**
  Install the required npm packages:

    ```bash
    npm install --prefix frontend/ChorukoWeb
    ```

## Running the Application
Once you have completed the backend and frontend setup, you can run the application.

1. **Backend:**
  Open a new terminal window and navigate to the `backend/ChorukoAPI` directory.
  Run the following command to start the backend server:

    ```bash
    dotnet run
    ```
    
    The API will be available at `http://localhost:5180`.

2. **Frontend:**
  Open another terminal window and navigate to the `frontend/ChorukoWeb` directory.
  Run the following command to start the frontend development server:
  
    ```bash
    npm run dev
    ```
  
    The web application will be available at `http://localhost:3000`.

## What's Next?
Now that you have your development environment set up, you might want to explore the following:
- **API documentation:** Once the backend is running, you can access the Swagger UI at `http://localhost:5180/swagger` to explore the API endpoints.
- **Project architecture:** Learn more about the project's structure and design by reading the documents in the `/docs/UML` directory.