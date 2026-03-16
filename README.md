# PrivateDoc AI — Phase 1 Scaffold

> **Portfolio project** · Privacy-first document Q&A web application

A responsive web application where users upload PDFs and CSV files and ask natural-language questions about their content. Answers are grounded **only** in the files uploaded during the current temporary session — nothing is stored permanently.

---

## Problem Solved

Users often need to query information from private documents without sending those documents to a cloud service. PrivateDoc AI enables session-scoped document analysis: upload, ask, get answers, then clear — all locally, without a database.

---

## Phase 1 Scope

| ✅ Included | ❌ Out of Scope |
|---|---|
| Responsive React + TypeScript UI | Authentication |
| In-memory session management | OCR / scanned PDFs |
| PDF text extraction (text-based PDFs) | XLSX support |
| CSV parsing and text extraction | Vector databases |
| Basic keyword-match Q&A engine | Cloud deployment |
| Evidence snippets in answers | LLM API integration (hook exists) |
| Clear session + file cleanup | Multi-user persistence |

---

## Stack

| Layer | Technology |
|---|---|
| Frontend | React 19, TypeScript, Vite |
| Backend | ASP.NET Core Web API, .NET 8 |
| PDF extraction | [PdfPig](https://github.com/UglyToad/PdfPig) |
| State management | React `useState` / `useEffect` |
| Storage | In-memory + local temp files |

---

## Project Structure

```
SafeQueryAI/
├── frontend/                  # React + TypeScript + Vite
│   ├── src/
│   │   ├── components/        # UI components
│   │   ├── services/api.ts    # API client layer
│   │   ├── types/api.ts       # Shared TypeScript types
│   │   ├── styles/app.css     # Application styles
│   │   ├── App.tsx            # App shell + state management
│   │   └── main.tsx           # Entry point
│   ├── vite.config.ts
│   └── package.json
│
└── backend/                   # ASP.NET Core Web API
    ├── Controllers/           # HTTP endpoints
    ├── Services/              # Business logic
    │   └── Interfaces/        # Service contracts
    ├── Models/                # Internal models
    ├── Contracts/             # API request/response DTOs
    ├── Program.cs
    └── PrivateDoc.Api.csproj
```

---

## How to Run Locally

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm

### 1. Start the Backend

```bash
cd backend
dotnet run
# API runs on http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### 2. Start the Frontend

```bash
cd frontend
npm install
npm run dev
# App runs on http://localhost:5173
```

Open [http://localhost:5173](http://localhost:5173) in your browser.

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `POST` | `/api/sessions` | Create a new session |
| `GET` | `/api/sessions/{id}` | Get session info |
| `DELETE` | `/api/sessions/{id}` | Clear session + delete files |
| `POST` | `/api/sessions/{id}/files` | Upload a PDF or CSV file |
| `GET` | `/api/sessions/{id}/files` | List uploaded files |
| `POST` | `/api/sessions/{id}/questions` | Ask a question |
| `GET` | `/api/health` | Health check |

---

## Privacy-First Design Notes

- **No database** — session data is held in process memory only
- **Temporary files** — uploads are stored in a local `TempSessions/` folder, scoped per session
- **Manual clear** — the user explicitly triggers file and session removal
- **No telemetry** — no analytics, tracking, or external calls
- **Extracted text is server-side only** — raw file content is never returned to the client
- **Session IDs are random UUIDs** — not tied to any user identity

---

## Current Limitations

- Text-layer PDFs only (no OCR for scanned/image PDFs)
- Answering uses keyword matching, not semantic search
- Sessions do not persist across server restarts
- No authentication — designed for single-user local use only
- No automatic session expiry (manual clear required)

---

## Future Evolution Ideas

- **Phase 2:** Add semantic search using a local embedding model (e.g., sentence-transformers via Python sidecar)
- **Phase 3:** Integrate an optional LLM (behind the existing `IQuestionAnsweringService` interface)
- **Phase 4:** Add user authentication and optional persistent session history
- **Phase 5:** Container deployment (Docker Compose for frontend + backend)
- Add automatic session expiry and cleanup background job
- Support XLSX files
- Add OCR support for scanned PDFs

---

## Portfolio Notes

### Why React + C#?

I chose React + TypeScript for the frontend because it produces a strongly-typed, component-based UI that is easy to reason about and extend. Vite gives fast local development without complex configuration.

For the backend I chose ASP.NET Core (.NET 8) because:
- It's my primary professional stack
- It provides excellent built-in dependency injection, middleware, and hosting
- C# gives strong typing across the full backend surface
- It produces a single deployable binary — easy to explain and demo

I deliberately avoided Python and microservices for Phase 1. A single React + C# monolith is far easier to run, explain to interviewers, and evolve incrementally.

### What I Wanted to Validate

- Can I build a working privacy-first document Q&A pipeline without a vector database?
- Does a simple keyword-matching approach produce usable answers for a Phase 1 demo?
- Can the architecture cleanly support swapping in an LLM later?

### How This Scaffold Supports Future Evolution

The `IQuestionAnsweringService` interface means the keyword-matching implementation can be replaced with an LLM-backed version in Phase 2 without changing any controllers. The `ITextExtractionService` interface similarly allows future OCR support to be added transparently. The frontend's `services/api.ts` layer means the base URL and auth headers can be updated in one place.

