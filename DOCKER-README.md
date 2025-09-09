# Docker Development Environment

## Overview

This document describes the Docker-based development environment for the BonusSystem project.

## Environment Components

### API Container

- **Image**: .NET 9 ASP.NET Core
- **Ports**: 5001 (HTTP), 5002 (HTTPS)
- **Configuration**: Environment variables for API settings
- **Volume Mounts**: Source code is mounted for hot reloading

### PostgreSQL Container

- **Image**: PostgreSQL 14 Alpine
- **Port**: 5432
- **Credentials**: Defined in .env file
- **Volume**: Persistent volume for data storage
- **Init Scripts**: Database schema and seed data

### pgAdmin Container

- **Image**: pgAdmin 4
- **Port**: 5050
- **Credentials**: Defined in .env file
- **Purpose**: Web-based database administration

## Getting Started

### Starting the Environment

```bash
docker-compose up -d
```

### Stopping the Environment

```bash
docker-compose down
```

### Viewing Logs

```bash
# All containers
docker-compose logs -f

# Specific container
docker-compose logs -f api
```

## Common Tasks

### Accessing the API Container Shell

```bash
docker-compose exec api /bin/bash
```

### Rebuilding the API Container

```bash
docker-compose build api
```

### Resetting the Database

```bash
docker-compose down -v
docker-compose up -d
```

## Connecting to Services

### PostgreSQL

- **Host**: localhost
- **Port**: 5432
- **Username**: postgres (or as defined in .env)
- **Password**: postgres (or as defined in .env)
- **Database**: bonussystem

### pgAdmin

- **URL**: http://localhost:5050
- **Email**: admin@bonussystem.com (or as defined in .env)
- **Password**: admin (or as defined in .env)

## Troubleshooting

### Container Not Starting

Check logs for errors:

```bash
docker-compose logs <service-name>
```

### Database Connection Issues

Ensure PostgreSQL is running:

```bash
docker-compose ps
```

Verify connection settings in .env file match the application settings.

### API Not Connecting to Database

Ensure the connection string in the API environment variables matches your PostgreSQL settings.

## Advanced Usage

### Customizing Environment Variables

Edit the `.env` file to customize ports, credentials, and other settings.

### Adding New Services

Edit `docker-compose.yml` to add new services as needed.

### Modifying Database Initialization

Edit or add scripts in the `scripts/init-db/` directory.
