# Production Environment Configuration

## Environment Variables

### Required Production Variables
```bash
# Database Configuration
CONNECTIONSTRINGS__DEFAULTCONNECTION="Server=your-prod-server;Database=POSSystemDB;User Id=your-user;Password=your-secure-password;TrustServerCertificate=false;MultipleActiveResultSets=true"

# Application Settings
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# Security
ASPNETCORE_HTTPS_PORT=443
ASPNETCORE_Kestrel__Certificates__Default__Password=your-cert-password
ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certificates/aspnetapp.pfx

# Logging
Logging__LogLevel__Default=Warning
Logging__LogLevel__Microsoft=Warning
Logging__LogLevel__Microsoft.Hosting.Lifetime=Information

# CORS (if needed)
CORS__AllowedOrigins__0=https://your-frontend-domain.com
CORS__AllowedOrigins__1=https://your-admin-panel.com
```

### Azure Key Vault Integration
```bash
# Azure Key Vault
AZURE_CLIENT_ID=your-service-principal-id
AZURE_CLIENT_SECRET=your-service-principal-secret
AZURE_TENANT_ID=your-tenant-id
KEYVAULT_URL=https://your-keyvault.vault.azure.net/
```

### Docker Secrets
```bash
# For Docker Swarm/Kubernetes
/run/secrets/db_password
/run/secrets/jwt_secret
/run/secrets/encryption_key
```

## GitHub Secrets Configuration

### Repository Secrets (Settings > Secrets and variables > Actions)

#### Database Secrets
- `PROD_DB_CONNECTION_STRING`: Production database connection string
- `STAGING_DB_CONNECTION_STRING`: Staging database connection string

#### Azure Secrets
- `AZURE_CLIENT_ID`: Service principal ID for Azure resources
- `AZURE_CLIENT_SECRET`: Service principal secret
- `AZURE_TENANT_ID`: Azure tenant ID
- `AZURE_SUBSCRIPTION_ID`: Azure subscription ID

#### Container Registry Secrets
- `CONTAINER_REGISTRY_USERNAME`: Docker registry username
- `CONTAINER_REGISTRY_PASSWORD`: Docker registry password

#### SSL/TLS Certificates
- `SSL_CERTIFICATE`: Base64 encoded SSL certificate
- `SSL_PRIVATE_KEY`: Base64 encoded private key
- `SSL_CERTIFICATE_PASSWORD`: Certificate password

#### Application Secrets
- `JWT_SECRET_KEY`: JWT token signing key
- `ENCRYPTION_KEY`: Data encryption key
- `API_KEY`: External API keys

#### Monitoring & Logging
- `APPLICATION_INSIGHTS_KEY`: Azure Application Insights key
- `SENTRY_DSN`: Sentry error tracking DSN
- `DATADOG_API_KEY`: DataDog monitoring API key

### Environment Variables (Settings > Secrets and variables > Actions > Variables)

#### Environment Configuration
- `ENVIRONMENT_NAME`: `production` | `staging` | `development`
- `API_BASE_URL`: Base URL for the API
- `FRONTEND_URL`: Frontend application URL

#### Feature Flags
- `ENABLE_SWAGGER`: `false` (disabled in production)
- `ENABLE_DETAILED_ERRORS`: `false`
- `ENABLE_CORS`: `true` | `false`

#### Performance Settings
- `MAX_REQUEST_SIZE`: `10MB`
- `CONNECTION_TIMEOUT`: `30`
- `COMMAND_TIMEOUT`: `120`

## Kubernetes Configuration

### Namespace
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: pos-system
```

### ConfigMap
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: pos-system-config
  namespace: pos-system
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Logging__LogLevel__Default: "Warning"
  Logging__LogLevel__Microsoft: "Warning"
```

### Secret
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: pos-system-secrets
  namespace: pos-system
type: Opaque
stringData:
  ConnectionStrings__DefaultConnection: "Server=sql-server;Database=POSSystemDB;User Id=sa;Password=SecurePassword123!;TrustServerCertificate=true"
  JwtSettings__SecretKey: "your-super-secret-jwt-key-here"
```

## Azure App Service Configuration

### Application Settings
```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "WEBSITES_ENABLE_APP_SERVICE_STORAGE": "false",
  "WEBSITE_HTTPLOGGING_RETENTION_DAYS": "3",
  "APPINSIGHTS_INSTRUMENTATIONKEY": "your-app-insights-key"
}
```

### Connection Strings
```json
{
  "DefaultConnection": {
    "value": "Server=your-azure-sql-server.database.windows.net;Database=POSSystemDB;User Id=your-user;Password=your-password;",
    "type": "SQLAzure"
  }
}
```

## Docker Production Configuration

### Environment File (.env.production)
```bash
# Database
POSTGRES_USER=posuser
POSTGRES_PASSWORD_FILE=/run/secrets/db_password
POSTGRES_DB=possystemdb

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# Security
JWT_SECRET_FILE=/run/secrets/jwt_secret
ENCRYPTION_KEY_FILE=/run/secrets/encryption_key
```

### Docker Compose Override (docker-compose.prod.yml)
```yaml
version: '3.8'

services:
  pos-api:
    image: pos-system-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    secrets:
      - db_password
      - jwt_secret
      - ssl_certificate
    ports:
      - "443:443"
      - "80:80"
    volumes:
      - ssl_certs:/app/certificates
    deploy:
      replicas: 3
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
      resources:
        limits:
          memory: 512M
          cpus: '0.5'

secrets:
  db_password:
    external: true
  jwt_secret:
    external: true
  ssl_certificate:
    external: true

volumes:
  ssl_certs:
    external: true
```

## Security Best Practices

### 1. Environment Separation
- Use different secrets for each environment
- Never share production secrets in development
- Rotate secrets regularly

### 2. Access Control
- Use service principals with minimal permissions
- Enable Azure AD authentication where possible
- Implement IP restrictions for databases

### 3. Monitoring
- Enable Application Insights for telemetry
- Set up alerts for failed authentications
- Monitor database performance

### 4. Backup & Recovery
- Automated database backups
- Test restore procedures regularly
- Document recovery procedures

## Deployment Checklist

- [ ] Update connection strings
- [ ] Configure SSL certificates
- [ ] Set up monitoring and alerting
- [ ] Configure backup policies
- [ ] Update DNS records
- [ ] Test health checks
- [ ] Verify security configurations
- [ ] Run database migrations
- [ ] Perform smoke tests
- [ ] Update documentation
