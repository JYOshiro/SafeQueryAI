---
layout: default
title: API Documentation
---

# API Documentation

SafeQueryAI provides a RESTful API for all operations. The backend is an ASP.NET Core Web API running on `https://localhost:7180` (or as configured).

## Base URL

```
https://localhost:7180/api
```

## Endpoints Overview

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `POST` | `/sessions/create` | Create a new session |
| `DELETE` | `/sessions/{sessionId}` | Delete a session |
| `GET` | `/sessions/{sessionId}` | Get session info |
| `POST` | `/files/upload` | Upload a file |
| `GET` | `/files/{sessionId}` | List files in session |
| `POST` | `/questions/ask` | Ask a question |
| `GET` | `/health` | Health check |

## Detailed Endpoints

### Sessions

#### Create Session
```
POST /sessions/create
```

**Response**:
```json
{
  "sessionId": "uuid-string",
  "createdAt": "2024-03-18T12:30:00Z",
  "expiresAt": "2024-03-18T13:30:00Z"
}
```

#### Get Session Info
```
GET /sessions/{sessionId}
```

**Response**:
```json
{
  "sessionId": "uuid-string",
  "createdAt": "2024-03-18T12:30:00Z",
  "expiresAt": "2024-03-18T13:30:00Z",
  "fileCount": 2,
  "documentChunkCount": 150
}
```

#### Delete Session
```
DELETE /sessions/{sessionId}
```

**Response**:
```json
{
  "message": "Session deleted successfully"
}
```

### Files

#### Upload File
```
POST /files/upload
Content-Type: multipart/form-data

Parameters:
- sessionId (required): Session UUID
- file (required): File to upload (PDF or CSV)
```

**Response**:
```json
{
  "fileName": "document.pdf",
  "fileSize": 102400,
  "fileType": "application/pdf",
  "uploadedAt": "2024-03-18T12:31:00Z",
  "status": "processing"
}
```

#### List Session Files
```
GET /files/{sessionId}
```

**Response**:
```json
{
  "files": [
    {
      "fileName": "document1.pdf",
      "fileSize": 102400,
      "uploadedAt": "2024-03-18T12:31:00Z"
    },
    {
      "fileName": "data.csv",
      "fileSize": 51200,
      "uploadedAt": "2024-03-18T12:32:00Z"
    }
  ]
}
```

### Questions

#### Ask Question
```
POST /questions/ask
Content-Type: application/json

{
  "sessionId": "uuid-string",
  "question": "What does the document say about...?"
}
```

**Response** (Server-Sent Events stream):
```
data: {"chunk":"The document"}
data: {"chunk":" says "}
data: {"chunk":"that..."}
data: {"status":"complete","totalTokens":245}
```

Client-side example:

```javascript
const response = await fetch('/api/questions/ask', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ sessionId, question })
});

const reader = response.body.getReader();
while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  const line = new TextDecoder().decode(value);
  // Process chunk
}
```

### Health

#### Health Check
```
GET /health
```

**Response**:
```json
{
  "status": "healthy",
  "timestamp": "2024-03-18T12:30:00Z",
  "services": {
    "ollama": "connected",
    "storage": "available"
  }
}
```

## Error Responses

All errors follow this format:

```json
{
  "error": "Error type",
  "message": "Human-readable error description",
  "details": "Additional context (if available)"
}
```

### Common Error Codes

| Code | Message | Solution |
|------|---------|----------|
| 400 | Bad Request | Check request format and parameters |
| 404 | Session Not Found | Create a new session first |
| 422 | File Processing Failed | Check file format (PDF/CSV only) |
| 500 | Ollama Connection Failed | Ensure Ollama service is running |
| 503 | Service Unavailable | Backend is starting, retry after a moment |

## Request/Response Models

### AskQuestionRequest
```csharp
{
  "sessionId": "string (UUID)",
  "question": "string (1-2000 chars)"
}
```

### AnswerStreamChunk
```csharp
{
  "chunk": "string (portion of answer)",
  "status": "generating" | "complete" | "error"
}
```

### FileUploadResponse
```csharp
{
  "fileName": "string",
  "fileSize": "long (bytes)",
  "fileType": "string (MIME type)",
  "uploadedAt": "DateTime",
  "status": "processing" | "indexed" | "failed"
}
```

### SessionInfo
```csharp
{
  "sessionId": "Guid",
  "createdAt": "DateTime",
  "expiresAt": "DateTime",
  "fileCount": "int",
  "documentChunkCount": "int"
}
```

## Authentication

Currently, SafeQueryAI has no authentication layer. For production deployments, consider adding:

- JWT token authentication
- API key validation
- Role-based access control (RBAC)
- Rate limiting per session

See [Deployment](deployment.md) for production recommendations.

## Rate Limiting

Default limits (per session):
- File uploads: 10 per session
- Questions: 100 per hour
- File size: 50 MB per file

Modify in `appsettings.json` as needed.

## CORS Configuration

By default, CORS is configured to accept requests from `http://localhost:*`. For production, update `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader());
});
```

## API Examples

### JavaScript/Fetch

```javascript
// Create session
const sessionRes = await fetch('/api/sessions/create', { method: 'POST' });
const { sessionId } = await sessionRes.json();

// Upload file
const formData = new FormData();
formData.append('sessionId', sessionId);
formData.append('file', fileInput.files[0]);
await fetch('/api/files/upload', { method: 'POST', body: formData });

// Ask question
const questionRes = await fetch('/api/questions/ask', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ sessionId, question: 'What is...' })
});
```

### cURL

```bash
# Create session
curl -X POST https://localhost:7180/api/sessions/create

# Get session info
curl https://localhost:7180/api/sessions/YOUR_SESSION_ID

# Ask question
curl -X POST https://localhost:7180/api/questions/ask \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"YOUR_SESSION_ID","question":"What is..."}'
```

## Troubleshooting

- **CORS Errors**: Ensure frontend URL is in allowed origins
- **Session Expired**: Sessions expire after 60 minutes of no activity
- **File Upload Fails**: Check file size and format (PDF/CSV only)
- **Question Returns Error**: Ensure Ollama is running and models are loaded
