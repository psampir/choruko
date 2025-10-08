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
- [Git LFS](https://git-lfs.github.com/) (for handling PNG images)

## Getting Started

### Install Git LFS

This project uses Git LFS (Large File Storage) to manage PNG images. You need to install and configure Git LFS before cloning the repository.

1. **Install Git LFS:**

   - **macOS:** `brew install git-lfs`
   - **Windows:** Download and run the installer from [git-lfs.github.com](https://git-lfs.github.com/)
   - **Linux (Debian/Ubuntu):** `sudo apt-get install git-lfs`
   - **Linux (Fedora):** `sudo dnf install git-lfs`

2. **Initialize Git LFS:**

   After installation, run the following command once to set up Git LFS for your user account:

   ```bash
   git lfs install
   ```

### Clone the Repository

Clone the repository to your local machine and navigate into the project directory:

```bash
git clone https://github.com/psampir/choruko.git
cd choruko
```

Git LFS will automatically download the PNG images during the clone process. If you cloned the repository before installing Git LFS, run `git lfs pull` to fetch the image files.

## Backend Setup
The backend is an ASP.NET Core application.

1. **Configure launch settings:**
  Navigate to the `backend/ChorukoAPI/Properties` directory.
  Create a copy of `launchSettings.example.json` and rename it to `launchSettings.json`. Adjust environment variables as needed.

2. **Database Migrations:**
  The development database runs in a Docker container, defined in `docker/compose/dev/docker-compose.yml`. Before you can run the backend, you need to apply the database migrations.

    First, start the database container:
      
    ```bash
    docker compose -f docker/compose/dev/docker-compose.yml up -d
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

> PWA installability requirements:
> - The app must be served over HTTPS, or from a local development environment using localhost or 127.0.0.1 — with or without a port number.
> - The PWA manifest and service worker are generated at build time by vite-plugin-pwa; run a production build before testing installation.

1. **Install dependencies:**
  Install the required npm packages:

    ```bash
    npm install --prefix frontend/ChorukoWeb
    ```

2. **Build (generates PWA manifest and service worker):**
    ```bash
    npm run build --prefix frontend/ChorukoWeb
    ```

## Running the Application
Once you have completed the backend and frontend setup, you can run the application.

1. **Backend:**
  Open a new terminal window and navigate to the `backend/ChorukoAPI` directory.
  Run the following command to start the backend server:

    ```bash
    cd backend/ChorukoAPI
    dotnet run
    ```
    
    The API will be available at `http://localhost:5180`.

2. **Frontend:**

    **a)** Without PWA features (development mode):

    Open another terminal window and navigate to the `frontend/ChorukoWeb` directory.

      ```bash
      cd frontend/ChorukoWeb
      ```

      To start the frontend development server, run the following command:
    
      ```bash
      npm run dev
      ```
    
      The web application will be available at `http://localhost:3000`.

    ---

    **b)** With PWA features (production mode):

    Open another terminal window and navigate to the `frontend/ChorukoWeb` directory.

      ```bash
      cd frontend/ChorukoWeb
      ```

    In order to enable PWA, you need to build the production version and serve it locally (PWA requires manifest and service worker generated at build time):

    ```bash
    npm run build
    npm run preview
    ```

    The PWA will be available at `http://localhost:3000`.

    To test PWA features locally, follow the guide: [Testing PWA Mobile](test-pwa-mobile.md).

## What's Next?
Now that you have your development environment set up, you might want to explore the following:
- **API documentation:** Once the backend is running, you can access the Swagger UI at [http://localhost:5180/swagger](http://localhost:5180/swagger) to explore the API endpoints.
- **Project architecture:** Learn more about the project's structure and design by reading the documents in the [/docs/UML](../UML/) directory.