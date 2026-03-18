---
layout: default
title: Configuration
---

# Configuration Guide

This guide covers environment variables, settings, and customization options for both backend and frontend.

## Backend Configuration

Backend settings are managed in `backend/appsettings.json` (development) and `appsettings.Production.json` (production).

### Core Settings

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OllamaSettings": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text",
    "GenerationModel": "llama3.2",
    "EmbeddingDimension": 768,
    "RequestTimeout": 120
  },
  "FileStorageSettings": {
    "TempFolderPath": "./TempSessions",
    "MaxFileSizeBytes": 52428800,
    "AllowedFileTypes": ["pdf", "csv"]
  },
  "SessionSettings": {
    "SessionTimeoutMinutes": 60,
    "CleanupIntervalSeconds": 300,
    "MaxSessionsPerDay": 100
  },
  "RagSettings": {
    "ChunkSize": 1000,
    "ChunkOverlap": 200,
    "TopKResults": 5,
    "SimilarityThreshold": 0.3
  }
}
```

### Environment Variables

You can override settings with environment variables using the format `SectionName__Key`:

```bash
# Ollama
export OllamaSettings__BaseUrl=http://ollama-prod.example.com:11434
export OllamaSettings__GenerationModel=llama3.1

# File Storage
export FileStorageSettings__MaxFileSizeBytes=104857600

# Sessions
export SessionSettings__SessionTimeoutMinutes=120
```

### Setting Descriptions

| Setting | Default | Description |
|---------|---------|-------------|
| `OllamaSettings.BaseUrl` | `http://localhost:11434` | Ollama service endpoint |
| `OllamaSettings.EmbeddingModel` | `nomic-embed-text` | Model for embeddings |
| `OllamaSettings.GenerationModel` | `llama3.2` | Model for answer generation |
| `OllamaSettings.RequestTimeout` | `120` (seconds) | Timeout for API calls |
| `FileStorageSettings.TempFolderPath` | `./TempSessions` | Temporary file directory |
| `FileStorageSettings.MaxFileSizeBytes` | `52428800` (50 MB) | Max file size |
| `FileStorageSettings.AllowedFileTypes` | `["pdf", "csv"]` | Accepted file types |
| `SessionSettings.SessionTimeoutMinutes` | `60` | Session inactivity timeout |
| `SessionSettings.CleanupIntervalSeconds` | `300` | Session cleanup interval |
| `RagSettings.ChunkSize` | `1000` | Document chunk size |
| `RagSettings.ChunkOverlap` | `200` | Overlap between chunks |
| `RagSettings.TopKResults` | `5` | Retrieved context chunks |
| `RagSettings.SimilarityThreshold` | `0.3` | Embedding similarity cutoff |

### CORS Configuration

Edit `Program.cs` to allow specific origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// Later in the pipeline
app.UseCors("AllowFrontend");
```

### Logging Configuration

Control logging level and output:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "SafeQueryAI": "Debug"
    }
  }
}
```

### Tuning RAG Parameters

**For faster responses** (sacrifice accuracy):
```json
{
  "RagSettings": {
    "ChunkSize": 500,
    "TopKResults": 3,
    "SimilarityThreshold": 0.5
  }
}
```

**For better accuracy** (slower responses):
```json
{
  "RagSettings": {
    "ChunkSize": 2000,
    "ChunkOverlap": 400,
    "TopKResults": 10,
    "SimilarityThreshold": 0.1
  }
}
```

**For large documents**:
```json
{
  "RagSettings": {
    "ChunkSize": 1500,
    "ChunkOverlap": 300,
    "TopKResults": 7
  }
}
```

## Frontend Configuration

Frontend settings are in `frontend/.env` files and Vite config.

### Environment Variables

Create `.env.local` (gitignored) for local overrides:

```env
# Development
VITE_API_URL=http://localhost:7180/api

# Production
# VITE_API_URL=https://api.yourdomain.com/api
```

### Vite Configuration

Edit `vite.config.ts` for build optimization:

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:7180',
        changeOrigin: true
      }
    }
  },
  build: {
    outDir: 'dist',
    sourcemap: false, // Set true for debugging
    minify: 'terser'
  }
})
```

### Feature Flags

Add to `App.tsx` for feature management:

```typescript
const FEATURES = {
  STREAMING_ANSWERS: true,
  CSV_SUPPORT: true,
  SESSION_PERSISTENCE: false
};
```

## Advanced Configuration

### Custom Embedding Model

To use a different embedding model:

1. Pull the model in Ollama: `ollama pull your-model`
2. Update `appsettings.json`:
   ```json
   {
     "OllamaSettings": {
       "EmbeddingModel": "your-model",
       "EmbeddingDimension": 1024
     }
   }
   ```
3. Restart backend

### Custom LLM Generation Model

To use a different language model:

1. Pull model in Ollama: `ollama pull your-model`
2. Update `appsettings.json`:
   ```json
   {
     "OllamaSettings": {
       "GenerationModel": "your-model"
     }
   }
   ```
3. Restart backend
4. Frontend works unchanged (model switch is transparent)

### Database Integration

For production, consider adding persistent storage:

1. **Enable entity logging**:
   ```csharp
   var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
       .UseSqlServer(connectionString)
       .EnableSensitiveDataLogging();
   ```

2. **Add Entity Framework migrations**:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### Authentication & Authorization

**Add JWT Authentication**:

```csharp
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://auth.example.com";
        options.Audience = "safequeryai-api";
    });

app.UseAuthentication();
app.UseAuthorization();
```

**Protect endpoints**:

```csharp
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase { }
```

## Performance Tuning

### Backend Performance

- **Increase timeout** for large documents: `OllamaSettings.RequestTimeout`
- **Optimize chunks**: Adjust `RagSettings.ChunkSize` based on document type
- **Enable compression**: Add gzip middleware in `Program.cs`

### Frontend Performance

```typescript
// In vite.config.ts
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor': ['react', 'react-dom']
        }
      }
    },
    chunkSizeWarningLimit: 1000
  }
})
```

## Health & Monitoring

### Backend Health Endpoint

```
GET /health
```

Returns:
```json
{
  "status": "healthy",
  "services": {
    "ollama": "connected",
    "storage": "available",
    "sessions": "active"
  }
}
```

### Add Metrics

Integrate OpenTelemetry for observability:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());
```

## Reset to Defaults

### Backend
```bash
git checkout backend/appsettings.json
```

### Frontend
```bash
git checkout frontend/.env
rm frontend/.env.local
```

## Troubleshooting Configuration

| Issue | Solution |
|-------|----------|
| Ollama not found | Check `OllamaSettings.BaseUrl` |
| Files not uploading | Increase `MaxFileSizeBytes` |
| Questions timeout | Increase `RequestTimeout` |
| Sessions expire too quickly | Increase `SessionTimeoutMinutes` |
| Answer quality poor | Adjust `RagSettings` parameters |

For more help, see the [Troubleshooting](troubleshooting.md) section.
