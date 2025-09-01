#!/bin/bash

echo "Setting up POS System Development Environment..."

# Create necessary directories
mkdir -p /home/vscode/.nuget
mkdir -p /app/TestResults

# Install additional tools
echo "Installing additional development tools..."
sudo apt-get update
sudo apt-get install -y curl wget jq tree

# Install Entity Framework CLI tools globally
echo "Installing Entity Framework CLI tools..."
dotnet tool install --global dotnet-ef

# Install additional .NET tools
echo "Installing additional .NET tools..."
dotnet tool install --global dotnet-format
dotnet tool install --global dotnet-outdated-tool
dotnet tool install --global dotnet-reportgenerator-globaltool

# Ensure tools are in PATH
echo 'export PATH="$PATH:/home/vscode/.dotnet/tools"' >> /home/vscode/.bashrc

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
max_attempts=30
attempt=0

while [ $attempt -lt $max_attempts ]; do
    if sqlcmd -S sqlserver,1433 -U sa -P "DevPassword123!" -C -Q "SELECT 1" > /dev/null 2>&1; then
        echo "SQL Server is ready!"
        break
    fi
    echo "SQL Server not ready yet (attempt $((attempt + 1))/$max_attempts)..."
    sleep 2
    attempt=$((attempt + 1))
done

if [ $attempt -eq $max_attempts ]; then
    echo "ERROR: SQL Server failed to start within expected time"
    exit 1
fi

# Run database migrations
echo "Running database migrations..."
cd /app
if [ -f "POSSystem.Migrator/POSSystem.Migrator.csproj" ]; then
    dotnet run --project POSSystem.Migrator/POSSystem.Migrator.csproj -- "Server=sqlserver,1433;Database=POSSystemDB;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;MultipleActiveResultSets=true"
    echo "Database migrations completed!"
else
    echo "WARNING: No migrator project found"
fi

# Set up Git if not already configured
if ! git config --global user.name > /dev/null 2>&1; then
    echo "Setting up Git configuration..."
    echo "Please configure Git with your details:"
    echo "git config --global user.name 'Your Name'"
    echo "git config --global user.email 'your.email@example.com'"
fi

# Create useful aliases
echo "Setting up development aliases..."
cat >> /home/vscode/.bashrc << 'EOF'

# POS System Development Aliases
alias pos-build='dotnet build POSSystem.csproj'
alias pos-run='dotnet run --project POSSystem.csproj'
alias pos-test='dotnet test POSSystem.Tests/POSSystem.Tests.csproj'
alias pos-migrate='dotnet run --project POSSystem.Migrator/POSSystem.Migrator.csproj'
alias pos-watch='dotnet watch run --project POSSystem.csproj'
alias pos-clean='dotnet clean && rm -rf bin obj'
alias pos-restore='dotnet restore'
alias pos-format='dotnet format'
alias pos-logs='docker-compose logs -f'
alias pos-restart='docker-compose restart pos-api'

# Docker shortcuts
alias dc='docker-compose'
alias dcu='docker-compose up'
alias dcd='docker-compose down'
alias dcr='docker-compose restart'
alias dcl='docker-compose logs'

echo "POS System Development Environment Ready!"
echo ""
echo "Available commands:"
echo "  pos-build    - Build the application"
echo "  pos-run      - Run the application"
echo "  pos-test     - Run tests"
echo "  pos-migrate  - Run database migrations"
echo "  pos-watch    - Run with hot reload"
echo "  pos-clean    - Clean build artifacts"
echo "  pos-format   - Format code"
echo ""
echo "Docker shortcuts:"
echo "  dc, dcu, dcd, dcr, dcl"
echo ""
echo "Application will be available at: http://localhost:5122"
echo "Swagger UI: http://localhost:5122/swagger"
echo ""
EOF

echo "Development environment setup completed!"
echo "Restart your terminal or run 'source ~/.bashrc' to load aliases"
