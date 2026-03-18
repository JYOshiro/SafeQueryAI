# SafeQueryAI

> **Portfolio project** · Privacy-first document Q&A with local RAG

A responsive web application where users upload PDFs and CSV files and ask natural-language questions about their content. Answers are grounded **only** in the files uploaded during the current temporary session — nothing is stored permanently and no data ever leaves your machine.

---

## Problem Solved

Users often need to query private documents without sending them to a cloud service. SafeQueryAI runs entirely on your local machine: upload, ask, get answers, then clear — powered by a local LLM through Ollama, with no database and no external API calls.

---

## 📖 Documentation

Full documentation is available at **[https://jyoshiro.github.io/SafeQueryAI/](https://jyoshiro.github.io/SafeQueryAI/)**

Quick links:
- [Getting Started](https://jyoshiro.github.io/SafeQueryAI/getting-started.html)
- [Implementation Plan](https://jyoshiro.github.io/SafeQueryAI/implementation-plan.html)
- [Architecture](https://jyoshiro.github.io/SafeQueryAI/architecture.html)
- [API Reference](https://jyoshiro.github.io/SafeQueryAI/api-documentation.html)
- [Deployment Guide](https://jyoshiro.github.io/SafeQueryAI/deployment.html)
- [FAQ](https://jyoshiro.github.io/SafeQueryAI/faq.html)

---

## How It Works (RAG Pipeline)

1. **Upload** — PDF or CSV files are saved to a temporary session folder.
2. **Extract** — Text is extracted from the file immediately after upload.
3. **Chunk & Embed** — The extracted text is split into overlapping chunks and each chunk is embedded using the local Ollama embedding model (`nomic-embed-text`).
4. **Ask** — When a question is submitted, it is embedded and the most similar chunks are retrieved via cosine similarity from the in-memory vector store.
5. **Generate** — The retrieved chunks are sent as context to the local Ollama generation model (`llama3.2`), which produces a grounded answer.
6. **Clear** — When the session ends (manually or after 60 minutes of inactivity), all files, chunks, and embeddings are deleted.

If Ollama is offline when a file is uploaded, the system falls back to keyword matching so the application remains usable.

---

## Stack

| Layer | Technology |
|---|---|
| Frontend | React 19, TypeScript, Vite |
| Backend | ASP.NET Core Web API, .NET 8 |
| Local LLM runtime | [Ollama](https://ollama.com) |
| Embedding model | `nomic-embed-text` (via Ollama) |
| Generation model | `llama3.2` (via Ollama) |
| PDF extraction | [PdfPig](https://github.com/UglyToad/PdfPig) |
| Vector store | In-memory (session-scoped) |
| File storage | Local temp folder (session-scoped) |

---

## Prerequisites

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download |
| Node.js | 18+ | https://nodejs.org |
| Ollama | latest | https://ollama.com |

---

## How to Run Locally

### Step 1 — Install and start Ollama

Download and install Ollama from https://ollama.com, then start the local server:

```bash
ollama serve
```

Ollama runs on `http://localhost:11434` by default. Leave this terminal open.

### Step 2 — Pull the required models

Open a new terminal and pull both models (one-time download):

```bash
ollama pull nomic-embed-text
ollama pull llama3.2
```

> `nomic-embed-text` is ~274 MB. `llama3.2` is ~2 GB. Both are stored locally by Ollama and never sent anywhere.

### Step 3 — Start the backend

```bash
cd backend
dotnet run
```

The API starts on `http://localhost:5000`.  
Swagger UI is available at `http://localhost:5000/swagger`.

> On first run, .NET will restore NuGet packages automatically.  
> The backend will fail to start if `Ollama:BaseUrl` is set to a non-local address — this is intentional to prevent accidental data exfiltration.

### Step 4 — Start the frontend

Open a second terminal:

```bash
cd frontend
npm install
npm run dev
```

The app opens at `http://localhost:5173`.

> Vite proxies all `/api` requests to `http://localhost:5000`, so no CORS setup is needed.

### All three components running

| Component | URL |
|---|---|
| Frontend | http://localhost:5173 |
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Ollama | http://localhost:11434 |

---

## Configuration

All backend configuration is in [backend/appsettings.json](backend/appsettings.json):

```json
{
  "SafeQueryAI": {
    "TempStoragePath": "TempSessions",
    "MaxFileSizeMb": 20,
    "SessionTimeoutMinutes": 60
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text",
    "GenerationModel": "llama3.2"
  }
}
```

To swap models, change `EmbeddingModel` or `GenerationModel` to any model you have pulled with `ollama pull`. For example:

```bash
ollama pull mistral
```

Then set `"GenerationModel": "mistral"` in `appsettings.json` and restart the backend.

> **Privacy guardrail:** `Ollama:BaseUrl` must be a loopback address (`localhost` / `127.0.0.1`). The application will refuse to start if a remote URL is configured.

---

## Project Structure

```
SafeQueryAI/
├── frontend/                      # React + TypeScript + Vite
│   ├── src/
│   │   ├── components/            # UI components
│   │   ├── services/api.ts        # API client layer
│   │   ├── types/api.ts           # Shared TypeScript types
│   │   ├── App.tsx                # App shell + state management
│   │   └── main.tsx               # Entry point
│   └── vite.config.ts             # Dev server + API proxy
│
└── backend/                       # ASP.NET Core Web API
    ├── Controllers/               # HTTP endpoints
    ├── Services/
    │   ├── OllamaService.cs       # Embedding + generation via local Ollama
    │   ├── DocumentIndexingService.cs  # Chunking → embed → vector store
    │   ├── VectorStoreService.cs  # In-memory cosine similarity search
    │   ├── QuestionAnsweringService.cs # RAG pipeline + keyword fallback
    │   ├── SessionService.cs      # In-memory session state
    │   ├── SessionExpiryService.cs     # Background expiry + cleanup
    │   ├── FileStorageService.cs  # Temp file save/delete
    │   ├── TextExtractionService.cs    # PDF + CSV text extraction
    │   └── Interfaces/            # Service contracts
    ├── Models/                    # Internal models (DocumentChunk, SessionInfo, etc.)
    ├── Contracts/                 # API request/response DTOs
    ├── appsettings.json
    └── Program.cs
```

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/sessions` | Create a new session |
| `GET` | `/api/sessions/{id}` | Get session metadata |
| `DELETE` | `/api/sessions/{id}` | Clear session, delete files, purge RAG index |
| `POST` | `/api/sessions/{id}/files` | Upload a PDF or CSV (triggers embedding) |
| `GET` | `/api/sessions/{id}/files` | List uploaded files (metadata only) |
| `POST` | `/api/sessions/{id}/questions` | Ask a question (RAG answer) |
| `GET` | `/api/health` | Liveness check |

---

## Privacy-First Design

- **No database** — session data is held in process memory only
- **Local LLM only** — Ollama runs on your machine; the `BaseUrl` is validated to be a loopback address at startup
- **Temporary files** — uploads are stored in `TempSessions/`, scoped per session, and deleted when the session clears
- **Automatic expiry** — sessions inactive for 60 minutes are automatically expired and all associated files, chunks, and embeddings are removed
- **Startup cleanup** — the `TempSessions/` directory is wiped when the backend starts, removing any files left by a previous crashed process
- **No content in logs** — logs include only filenames and counts, never file content or extracted text
- **Extracted text is server-side only** — raw file content is never returned to the client
- **No telemetry** — no analytics, tracking, or external calls of any kind

---

## Current Limitations

- Text-layer PDFs only (no OCR for scanned/image-based PDFs)
- Sessions do not persist across server restarts (by design)
- No authentication — designed for single-user local use only
- Ollama models must be pulled manually before first use
- Large documents with many chunks may slow down the embedding step depending on hardware

---

## Future Ideas

- Docker Compose setup to start backend + frontend together
- OCR support for scanned PDFs
- XLSX file support
- Optional streaming responses from the LLM
- UI indicator showing whether Ollama is online

