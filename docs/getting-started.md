---
layout: default
title: Getting Started
---

# Getting Started

This guide covers a clean local setup for SafeQueryAI.

## Prerequisites

| Tool | Minimum Version | Purpose |
|---|---|---|
| .NET SDK | 8.0 | Backend API |
| Node.js | 18 | Frontend build and dev server |
| Ollama | Current stable | Local LLM runtime |

## Standard Local Endpoints

| Service | URL |
|---|---|
| Frontend | `http://localhost:5173` |
| Backend API | `http://localhost:5000/api` |
| Swagger UI | `http://localhost:5000/swagger` |
| Ollama | `http://localhost:11434` |

## 1. Clone the Repository

```bash
git clone https://github.com/JYOshiro/SafeQueryAI.git
cd SafeQueryAI
```

## 2. Start Ollama and Pull Models

```bash
ollama serve
```

In a second terminal:

```bash
ollama pull nomic-embed-text
ollama pull llama3.2
```

## 3. Start Backend

```bash
cd backend
dotnet restore
dotnet run
```

Backend should start on `http://localhost:5000`.

## 4. Start Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend should start on `http://localhost:5173`.

## 5. Validate End-to-End Flow

1. Open `http://localhost:5173`.
2. Create a session.
3. Upload a `.pdf` or `.csv` file (up to `20 MB`).
4. Ask a question and confirm an answer is returned.
5. Clear the session and confirm files are removed from the session list.

## Default Runtime Constraints

| Setting | Default |
|---|---|
| Session timeout | `60` minutes of inactivity |
| Supported file types | `.pdf`, `.csv` |
| Configured max upload size | `20 MB` |
| Request hard limit | `25 MB` |

## Troubleshooting

### Backend cannot reach Ollama

- Confirm Ollama is running: `ollama serve`.
- Confirm model availability: `ollama list`.
- Confirm `Ollama:BaseUrl` in backend config is `http://localhost:11434`.

### Frontend cannot reach backend

- Confirm backend is running on `http://localhost:5000`.
- Confirm frontend dev server is running on `http://localhost:5173`.
- Confirm Vite proxy target in `frontend/vite.config.ts` points to `http://localhost:5000`.

### Upload rejected

- Confirm file extension is `.pdf` or `.csv`.
- Confirm file size does not exceed `20 MB`.

### [REVIEW REQUIRED: verify backend URL consistency]

If your machine uses a different ASP.NET local URL, update local run settings and align references across this documentation set.

## Next Reading

- [Business Overview](business-overview.md)
- [Architecture](architecture.md)
- [Security & Privacy](security-privacy.md)
- [API Reference](api-documentation.md)
