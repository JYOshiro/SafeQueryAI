---
layout: default
title: Architecture
---

# Architecture

SafeQueryAI is built around a Retrieval-Augmented Generation (RAG) pipeline that processes documents and answers questions without storing data permanently.

## System Overview

```
┌─────────────────────────────────────────────────────────┐
│                    React Frontend                       │
│              (TypeScript, Vite, Responsive)             │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP/WebSocket
                         ▼
┌─────────────────────────────────────────────────────────┐
│              ASP.NET Core 8 Web API                      │
│         (Controllers, Services, Dependency Injection)   │
└────────────────────────┬────────────────────────────────┘
                    ┌────┴────┬────────────┬──────────────┐
                    ▼         ▼            ▼              ▼
            ┌─────────────┐ ┌──────────┐ ┌─────────────┐ ┌──────┐
            │   Session   │ │   File   │ │  Embeddings │ │Local │
            │  Management │ │ Storage  │ │ & Vector    │ │ Temp │
            └─────────────┘ └──────────┘ │   Store     │ │Files │
                                         └─────────────┘ └──────┘
                    ┌─────────────────────────┬──────────────┐
                    ▼                         ▼              ▼
            ┌──────────────────┐    ┌────────────────┐    ┌────────┐
            │   Ollama Local   │    │  LLM Inference │    │ Memory │
            │   Embedding      │    │    & RAG       │    │ Vector │
            │  (nomic-embed)   │    │   (llama3.2)   │    │ Store  │
            └──────────────────┘    └────────────────┘    └────────┘
```

## RAG Pipeline

### 1. **Document Upload**
- User uploads PDF or CSV file
- File is saved into a temporary session folder
- Session ID is generated for file tracking

### 2. **Text Extraction**
- **PDF files**: `PdfPig` library extracts text with layout awareness
- **CSV files**: Parsed as structured data, formatted for context
- Text is cleaned and normalized

### 3. **Chunking & Embedding**
- Text is split into overlapping chunks (configurable window size and stride)
- Each chunk is embedded using `nomic-embed-text` model via Ollama
- Embeddings are stored in an in-memory vector store

### 4. **Question Processing**
- User question is embedded using the same embedding model
- Cosine similarity search finds the K most relevant chunks
- Retrieved chunks provide context for answer generation

### 5. **Answer Generation**
- Context chunks + question are sent to `llama3.2` via Ollama
- Model generates a grounded answer based on document content
- Answer is streamed back to the frontend

### 6. **Session Cleanup**
- Session expires after configurable inactivity period (default: 60 minutes)
- All files, embeddings, and session data are permanently deleted
- **No persistent storage** — privacy is guaranteed

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

- **FileStorageService**: Manages temporary file storage
- **TextExtractionService**: Extracts text from PDFs/CSVs
- **DocumentIndexingService**: Chunks and embeds documents
- **VectorStoreService**: In-memory vector search
- **OllamaService**: Communication with Ollama APIs
- **QuestionAnsweringService**: RAG orchestration
- **SessionService**: Session lifecycle management
- **SessionExpiryService**: Automatic cleanup background process

### Frontend Structure

| Component | Purpose |
|-----------|---------|
| **App.tsx** | Main application component |
| **QuestionForm** | User input for questions |
| **FileUploadPanel** | Document upload interface |
| **AnswerPanel** | Streaming answer display |
| **SessionInfo** | Session details and management |
| **UploadedFileList** | List of files in session |

## Data Flow

```
Upload PDF
    ↓
Text Extraction (PdfPig)
    ↓
Chunk Text
    ↓
Embed Chunks (Ollama → nomic-embed-text)
    ↓
Store in Vector Store (In-Memory)
    ↓
User Asks Question
    ↓
Embed Question (Ollama → nomic-embed-text)
    ↓
Retrieve Top-K Chunks
    ↓
Generate Answer (Ollama → llama3.2)
    ↓
Stream Response to Frontend
```

## Failover & Resilience

- **Ollama Offline**: System falls back to keyword matching for document browsing
- **Model Load Failure**: User receives a feedback message
- **Session Expiry**: Automatic cleanup prevents disk space exhaustion
- **API Errors**: Graceful error handling with user-friendly messages

## Security & Privacy

- **No Data Persistence**: All data deleted after session expires
- **Local Only**: No external API calls or cloud uploads
- **Temporary Storage**: Files stored in isolated session directories
- **No Database**: All state is in-memory or temporary
- **No Tracking**: No telemetry or user analytics

## Performance Considerations

- Embedding lookup: ~milliseconds (in-memory)
- Chunk retrieval: ~milliseconds (cosine similarity)
- Answer generation: ~seconds (depends on LLM model)
- Vector store: O(n) search without indexing (fine for typical document sizes)

## Scalability Notes

Current design is optimized for single-user, single-machine deployment. For production scaling:
- Implement persistent vector store (e.g., Pinecone, Weaviate)
- Use distributed session management
- Add load balancing for multiple backend instances
- Consider model quantization for inference speed
