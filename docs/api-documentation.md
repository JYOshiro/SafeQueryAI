---
layout: default
title: API Reference
---

# API Reference

SafeQueryAI exposes REST endpoints for session lifecycle, file handling, and document question-answering.

## Base URL

```
http://localhost:5000/api
```

## Endpoint Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `POST` | `/sessions` | Create a new session |
| `GET` | `/sessions/{sessionId}` | Get session metadata |
| `DELETE` | `/sessions/{sessionId}` | Delete a session |
| `GET` | `/sessions/{sessionId}/files` | List session files |
| `POST` | `/sessions/{sessionId}/files` | Upload PDF or CSV file |
| `POST` | `/sessions/{sessionId}/questions` | Ask question (single response) |
| `POST` | `/sessions/{sessionId}/questions/stream` | Ask question (SSE stream) |
| `GET` | `/health` | Liveness check |

## Sessions

### Create Session
```
POST /sessions
```

Response example:
```json
{
  "sessionId": "uuid-string",
  "createdAt": "2026-03-18T12:30:00Z"
}
```

### Get Session
```
GET /sessions/{sessionId}
```

Response example:
```json
{
  "sessionId": "uuid-string",
  "createdAt": "2026-03-18T12:30:00Z"
}
```

### Delete Session
```
DELETE /sessions/{sessionId}
```

Response example:
```json
{
  "sessionId": "uuid-string",
  "cleared": true,
  "message": "Session cleared. All uploaded files have been removed."
}
```

## Files

### List Files in Session
```
GET /sessions/{sessionId}/files
```

Response example:
```json
{
  "sessionId": "uuid-string",
  "files": [
    {
      "fileId": "file-id",
      "originalFileName": "document.pdf",
      "fileType": "pdf",
      "fileSizeBytes": 102400,
      "uploadedAt": "2026-03-18T12:31:00Z"
    }
  ]
}
```

### Upload File
```
POST /sessions/{sessionId}/files
Content-Type: multipart/form-data

Form field:
- file (required)
```

Constraints:

- Allowed types: `.pdf`, `.csv`
- Configured file size limit: `20 MB`
- Request hard limit: `25 MB`

Response example:
```json
{
  "fileId": "file-id",
  "fileName": "document.pdf",
  "fileType": "pdf",
  "fileSizeBytes": 102400,
  "uploadedAt": "2026-03-18T12:31:00Z"
}
```

## Questions

### Ask Question (single response)
```
POST /sessions/{sessionId}/questions
Content-Type: application/json

{
  "question": "What does the document say about...?"
}
```

Response example:

```json
{
  "question": "What does the document say about...?",
  "answer": "...",
  "hasConfidentAnswer": true,
  "evidence": [
    {
      "fileName": "document.pdf",
      "snippet": "..."
    }
  ]
}
```

### Ask Question (stream)

```
POST /sessions/{sessionId}/questions/stream
Content-Type: application/json
Accept: text/event-stream

{
  "question": "What does the document say about...?"
}
```

SSE event types:

- token event: `{"type":"token","content":"..."}`
- done event: `{"type":"done","question":"...","hasConfidentAnswer":true,"evidence":[...]}`

Frontend consumption example:

```javascript
const response = await fetch(`/api/sessions/${sessionId}/questions/stream`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json', Accept: 'text/event-stream' },
  body: JSON.stringify({ question })
});

const reader = response.body.getReader();
while (true) {
  const { done, value } = await reader.read();
  if (done) break;
  const text = new TextDecoder().decode(value);
  // Parse data: lines
}
```

## Health

### Health Check
```
GET /health
```

Response example:
```json
{
  "status": "healthy",
  "timestamp": "2026-03-18T12:30:00Z"
}
```

## Error Responses

Error payloads generally use:

```json
{
  "error": "Error summary",
  "detail": "Optional additional detail"
}
```

Common cases:

| Code | Message | Solution |
|---|---|---|
| 400 | Invalid request | Validate payload and required fields |
| 404 | Session not found | Create a new session and retry |
| 400 | Unsupported file type | Use PDF or CSV |
| 400 | File too large | Keep file at or under configured limit |

## Local Development Integration

Frontend development server proxies `/api` to backend:

- Frontend: `http://localhost:5173`
- Proxy target: `http://localhost:5000`

This removes the need for manual CORS configuration during local development.

## Model and Privacy Assumptions

- Ollama endpoint must remain local loopback.
- Uploaded document text is processed only for active session operations.
- Session cleanup removes temporary files and in-memory indexing state.

## Reviewer Notes

- [REVIEW REQUIRED: confirm whether API contracts have changed after this documentation update]

## Related Pages

- [Architecture](architecture.md)
- [Security & Privacy](security-privacy.md)
- [Getting Started](getting-started.md)

<!-- Historical model snippets retained below only if needed for quick reference. -->

### AskQuestionRequest
```csharp
{
  "question": "string (1-2000 chars)"
}
```
