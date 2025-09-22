[← Back to README](../../README.md)

# Installing Choruko
This guide will help you install Choruko on your local machine.

## Prerequisites
- [Docker](https://docs.docker.com/get-docker/)
- SSL setup

## Step 1 - Download the required files

Create a directory for Choruko and navigate into it:

```bash
mkdir ./choruko
cd ./choruko
```

Download the [docker-compose.yml](../../docker/compose/prod/docker-compose.yml) and [env.example](../../docker/compose/example.env) files by running the following commands:

```bash
# Download docker-compose.yml
wget -O docker-compose.yml https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/prod/docker-compose.yml
```

```bash
# Download env.example
wget -O env.example https://raw.githubusercontent.com/psampir/choruko/refs/heads/main/docker/compose/example.env
```

## Step 2 - Configure environment variables

Create a copy of the [env.example](../../docker/compose/example.env) file and rename it to `.env`. You can then edit the `.env` file to adjust any environment variables as needed.

```bash
cp env.example .env
```

## Step 3 - Start the containers

From the directory where you downloaded the `docker-compose.yml` file and created the `.env` file, run the following command to start the containers: 

```bash
docker compose up -d
```

## Step 4 - Set up SSL

Choruko needs to be served over HTTPS for the web application to be installable as PWA (Progressive Web App). You can set up SSL using a reverse proxy like Nginx or Caddy.