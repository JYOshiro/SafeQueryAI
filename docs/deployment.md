---
layout: default
title: Deployment
---

# Deployment Guide

SafeQueryAI can be deployed to various environments. This guide covers common deployment scenarios.

## Local Deployment

For development and testing on your local machine. See [Getting Started](getting-started.md) for instructions.

## Docker Deployment

### Build Docker Image

Create a `Dockerfile` in the root directory:

```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY backend/ .
RUN dotnet publish -c Release -o out

FROM node:18 AS frontend-build
WORKDIR /app
COPY frontend/ .
RUN npm install && npm run build

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy backend
COPY --from=backend-build /app/out .

# Copy frontend built files to wwwroot
COPY --from=frontend-build /app/dist ./wwwroot

EXPOSE 80 443
ENTRYPOINT ["dotnet", "SafeQueryAI.Api.dll"]
```

### Build and Run

```bash
# Build image
docker build -t safequeryai:latest .

# Run container (with local Ollama)
docker run -d \
  --name safequeryai \
  -p 8080:80 \
  -e OllamaSettings__BaseUrl=http://host.docker.internal:11434 \
  safequeryai:latest
```

Access at `http://localhost:8080`

## Docker Compose

For local development with all services:

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  ollama:
    image: ollama/ollama:latest
    ports:
      - "11434:11434"
    volumes:
      - ollama_data:/root/.ollama
    environment:
      - OLLAMA_HOST=0.0.0.0:11434

  safequeryai:
    build: .
    ports:
      - "8080:80"
    environment:
      - OllamaSettings__BaseUrl=http://ollama:11434
    depends_on:
      - ollama
    volumes:
      - ./backend/TempSessions:/app/TempSessions

volumes:
  ollama_data:
```

Run with:

```bash
docker-compose up -d
```

## Linux Server Deployment

### Prerequisites

```bash
# Install .NET 8 SDK
wget https://dot.net/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Install Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Install Ollama
curl https://ollama.ai/install.sh | sh
```

### Setup Application

```bash
# Clone repository
git clone https://github.com/yourusername/SafeQueryAI.git
cd SafeQueryAI

# Build backend
cd backend
dotnet publish -c Release -o ../publish/backend
cd ..

# Build frontend
cd frontend
npm install
npm run build
cp -r dist ../publish/frontend
cd ..
```

### Systemd Service

Create `/etc/systemd/system/safequeryai.service`:

```ini
[Unit]
Description=SafeQueryAI
After=network.target

[Service]
Type=simple
User=safequeryai
WorkingDirectory=/opt/safequeryai
ExecStart=/home/username/.dotnet/dotnet SafeQueryAI.Api.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl enable safequeryai
sudo systemctl start safequeryai
sudo systemctl status safequeryai
```

## Cloud Deployment

### Microsoft Azure

#### App Service Deployment

```bash
# Create resource group
az group create --name safequeryai-rg --location eastus

# Create App Service plan
az appservice plan create \
  --name safequeryai-plan \
  --resource-group safequeryai-rg \
  --sku B2 \
  --is-linux

# Create web app
az webapp create \
  --resource-group safequeryai-rg \
  --plan safequeryai-plan \
  --name safequeryai-app \
  --runtime "DOTNETCORE|8.0"

# Deploy from GitHub
az webapp deployment source config-zip \
  --resource-group safequeryai-rg \
  --name safequeryai-app \
  --src-path publish.zip
```

### AWS (Elastic Beanstalk)

```bash
# Install EB CLI
pip install awsebcli

# Initialize environment
eb init -p "Docker running on 64bit Amazon Linux 2" safequeryai

# Create environment
eb create safequeryai-env

# Deploy
eb deploy
```

### Heroku

```bash
# Login
heroku login

# Create app
heroku create safequeryai

# Deploy
git push heroku main

# View logs
heroku logs --tail
```

## Production Considerations

### HTTPS/TLS

Enable certificate with Let's Encrypt:

```bash
# Using Certbot
sudo apt-get install certbot python3-certbot-nginx
sudo certbot certonly --standalone -d yourdomain.com
```

Update Kestrel config in `appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://+:443",
        "Certificate": {
          "Path": "/etc/letsencrypt/live/yourdomain.com/fullchain.pem",
          "KeyPath": "/etc/letsencrypt/live/yourdomain.com/privkey.pem"
        }
      }
    }
  }
}
```

### Reverse Proxy (Nginx)

Create `/etc/nginx/sites-available/safequeryai`:

```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable site:

```bash
sudo ln -s /etc/nginx/sites-available/safequeryai /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### Authentication

Add authentication for production. See [Configuration](configuration.md#authentication--authorization).

### Monitoring & Logging

Use centralized logging:

```csharp
// In Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddSerilog(); // For persistent logging
});
```

Install Serilog:

```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
```

### Environment Variables

Set in your hosting environment:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:5000
export OllamaSettings__BaseUrl=http://ollama-prod:11434
export SessionSettings__SessionTimeoutMinutes=120
```

### Backup Strategy

Back up important files:

```bash
# Backup temporary files (if persistence added)
tar -czf backup-sessions-$(date +%Y%m%d).tar.gz backend/TempSessions/

# Backup database (if using one)
mysqldump -u user -p database > backup-$(date +%Y%m%d).sql
```

### Scaling Considerations

For production scale:

1. **Stateful Storage**: Move from in-memory to persistent database
2. **Vector Store**: Use managed service (Pinecone, Weaviate)
3. **Load Balancing**: Add load balancer in front of multiple app instances
4. **Caching**: Add Redis for session management
5. **Model Serving**: Use dedicated inference servers for Ollama
6. **CDN**: Serve frontend assets through CDN

## Troubleshooting Deployment

| Issue | Solution |
|-------|----------|
| Port already in use | Change port in `appsettings.json` or firewall |
| Ollama connection failed | Verify network connectivity and BaseUrl |
| Files not persisting | Check disk permissions and TempSessions path |
| High memory usage | Reduce `SessionSettings.MaxSessionsPerDay` |
| Slow responses | Enable response caching, check Ollama load |

## Health Checks

Monitor health endpoint in production:

```bash
# Check every 30 seconds
while true; do
  curl -s http://localhost:5000/health | jq .
  sleep 30
done
```

Use monitoring tools:
- **Datadog** for metrics
- **Prometheus** for monitoring
- **ELK Stack** for logging
- **New Relic** for APM

## Security Hardening

- [ ] Enable HTTPS/TLS
- [ ] Set up authentication/authorization
- [ ] Enable CORS with specific origins
- [ ] Rate limit API endpoints
- [ ] Validate file uploads
- [ ] Use secrets management (Azure Key Vault, AWS Secrets)
- [ ] Regular security updates
- [ ] Firewall rules for Ollama port (11434)
- [ ] Audit logging enabled
- [ ] HTTPS-only cookies

## Zero-Downtime Deployment

For rolling updates:

```bash
# Build new version
dotnet publish -c Release

# Stop old instance
systemctl stop safequeryai

# Start new instance
systemctl start safequeryai

# Verify
curl http://localhost:5000/health
```

For blue-green deployment, use two app instances with load balancer.
