#!/bin/bash

echo "Initializing development environment..."

# Check if Docker is running
if ! docker info >/dev/null 2>&1; then
    echo "ERROR: Docker is not running. Please start Docker and try again."
    exit 1
fi

# Build and start services
if [ -f "docker-compose.dev.yml" ]; then
    echo "Building Docker images..."
    docker-compose -f docker-compose.dev.yml up --build -d
fi

echo "Environment initialization completed!"
