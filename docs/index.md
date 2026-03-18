---
layout: default
title: SafeQueryAI Documentation
---

# SafeQueryAI Documentation

SafeQueryAI is a privacy-first architecture for document question-answering.
It lets users upload PDF and CSV files, ask natural-language questions, and receive grounded answers from a local LLM runtime.

## Product Summary

SafeQueryAI addresses a common business need: extracting actionable information from private documents without exposing that data to external services.

The product enforces session-based processing:

- Documents are processed only within the active session.
- Data is held in temporary storage and in-memory indexes.
- Session cleanup removes files and indexes after timeout or manual clear.
- Ollama is restricted to a local loopback URL to prevent remote inference endpoints.

## Business Value

- Reduces privacy risk for teams working with sensitive internal documents.
- Improves speed-to-insight by enabling direct natural-language querying of uploaded files.
- Provides a demonstrable implementation of privacy-first architecture using modern web and API stacks.
- Supports portfolio and assessment scenarios with traceable architecture, testing, and documentation.

## High-Level Architecture

1. Frontend receives uploads and question input.
2. Backend stores files in temporary storage per session.
3. Text extraction and chunking prepare data for retrieval.
4. Embeddings and retrieval run through the local LLM runtime (Ollama).
5. Answers are generated using only session content and returned to the user.
6. Session timeout or manual clear removes temporary files and in-memory vectors.

See [Architecture](architecture.md) and [Security & Privacy](security-privacy.md) for full detail.

## Environment Reference

| Item | Default Value |
|---|---|
| Frontend URL | `http://localhost:5173` |
| Backend API | `http://localhost:5000/api` |
| Swagger UI (development) | `http://localhost:5000/swagger` |
| Ollama URL | `http://localhost:11434` |
| Supported file types | `.pdf`, `.csv` |
| Session timeout | `60` minutes |
| Max file size | `20` MB (`25` MB absolute request ceiling) |

## Audience Paths

- Product and non-technical stakeholders: [Business Overview](business-overview.md)
- Technical reviewers and assessors: [Architecture](architecture.md), [Security & Privacy](security-privacy.md), [Testing](testing.md)
- Developers: [Getting Started](getting-started.md), [API Reference](api-documentation.md), [Frontend Guide](frontend-guide.md), [Deployment](deployment.md)
- Recruiters and portfolio reviewers: [Business Overview](business-overview.md), [Roadmap](roadmap.md), [FAQ](faq.md)

## Documentation Map

| Section | Focus |
|---|---|
| [Business Overview](business-overview.md) | Problem statement, users, scope, risks, success criteria |
| [Getting Started](getting-started.md) | Local setup and first run |
| [Architecture](architecture.md) | System flow, components, constraints |
| [Security & Privacy](security-privacy.md) | Data handling model and trust assumptions |
| [API Reference](api-documentation.md) | Endpoint contract and examples |
| [Frontend Guide](frontend-guide.md) | UI structure and frontend integration points |
| [Testing](testing.md) | Test strategy, suites, and execution commands |
| [Deployment](deployment.md) | Current deployment approach and operational notes |
| [Roadmap](roadmap.md) | Planned improvements and delivery priorities |
| [FAQ](faq.md) | Common operational and setup questions |

---

Last updated: March 2026
