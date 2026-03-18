---
layout: default
title: Architecture
---

# Architecture

SafeQueryAI implements document question-answering using a local Retrieval-Augmented Generation (RAG) pipeline and session-based processing.

## Architecture Goals

- Keep document processing local to the machine.
- Enforce temporary storage and automatic cleanup.
- Ground answers in uploaded session documents.
- Keep implementation understandable for maintainers and assessors.

## System Overview

```
Browser (React + TypeScript)
    |
    | HTTP
    v
ASP.NET Core API
    |
    +--> SessionService (session lifecycle)
    +--> FileStorageService (temporary storage)
    +--> TextExtractionService (PDF/CSV extraction)
    +--> DocumentIndexingService (chunk + embed)
    +--> VectorStoreService (in-memory retrieval)
    +--> QuestionAnsweringService (RAG orchestration)
    |
    v
Ollama local LLM runtime (loopback URL only)
```

## RAG Pipeline

1. User uploads a PDF or CSV file to the active session.
2. Backend stores the file in a session folder under temporary storage.
3. Text extraction reads PDF/CSV content.
4. Document text is chunked and embedded through Ollama (`nomic-embed-text`).
5. Embeddings are stored in an in-memory vector store.
6. On question submission, the question is embedded and top-matching chunks are retrieved.
7. Backend generates an answer through Ollama (`llama3.2`) using retrieved context.
8. If embedding/generation is unavailable, the system falls back to keyword matching.
9. Session expiry or manual clear removes temporary files and index data.

## Components

### Backend Structure

| Component | Responsibility |
|-----------|-----------------|
| **Controllers** | HTTP endpoints for files, questions, sessions, health |
| **Services** | Business logic for indexing, storage, RAG, expiry |
| **Contracts** | Request/response DTOs |
| **Models** | Domain entities (SessionInfo, DocumentChunk, etc.) |
| **Interfaces** | Service abstractions for dependency injection |

### Key Services

- FileStorageService: session-scoped temporary storage.
- TextExtractionService: PDF and CSV text extraction.
- DocumentIndexingService: chunking and embedding orchestration.
- VectorStoreService: in-memory cosine similarity retrieval.
- OllamaService: local HTTP integration with Ollama.
- QuestionAnsweringService: answer generation and fallback orchestration.
- SessionService: session state tracking.
- SessionExpiryService: background cleanup for expired sessions.

### Frontend Structure

| Component | Purpose |
|-----------|---------|
| **App.tsx** | Main application component |
| **QuestionForm** | User input for questions |
| **FileUploadPanel** | Document upload interface |
| **AnswerPanel** | Streaming answer display |
| **SessionInfo** | Session details and management |
| **UploadedFileList** | List of files in session |

## Operational Characteristics

| Characteristic | Current Implementation |
|---|---|
| Storage model | Temporary storage + in-memory session state |
| Session timeout | 60 minutes inactivity |
| Supported file types | PDF, CSV |
| Upload size policy | 20 MB configured limit, 25 MB request ceiling |
| LLM runtime | Local Ollama only |
| API style | REST + SSE stream endpoint |

## Failover & Resilience

- Ollama unavailable: file upload can still succeed; answer path can use keyword fallback.
- Invalid session IDs: API returns explicit not found responses.
- Background expiry: stale sessions are cleared automatically.
- Local-only guardrail: non-loopback Ollama URL blocks startup.

## Constraints

- No persistent database in current design.
- No built-in authentication in current design.
- No OCR pipeline for scanned/image-only PDFs.
- Single-machine scope is the primary operating model.

## Assumptions

- Ollama is available locally with required models pulled.
- Session load remains within in-memory processing capacity.
- Users accept temporary-session behavior and non-persistent history.

## Related Pages

- [Business Overview](business-overview.md)
- [Security & Privacy](security-privacy.md)
- [API Reference](api-documentation.md)
- [Roadmap](roadmap.md)
