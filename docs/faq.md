---
layout: default
title: FAQ
---

# FAQ

## Setup and Environment

### Do I need internet access after initial setup?

You need internet access only to install dependencies and pull Ollama models. After that, SafeQueryAI can operate locally.

### What software is required?

- .NET SDK 8.0+
- Node.js 18+
- Ollama

### Which URLs should I expect in local development?

- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:5000/api`
- Swagger: `http://localhost:5000/swagger`
- Ollama: `http://localhost:11434`

## Product Behavior

### What is SafeQueryAI used for?

SafeQueryAI supports document question-answering on uploaded PDF and CSV files using session-based processing.

### Which file types are supported?

- `.pdf`
- `.csv`

### What is the upload limit?

Configured upload limit is `20 MB` per file, with a `25 MB` request ceiling.

### How long does a session last?

Sessions expire after `60 minutes` of inactivity by default.

### Can a user clear data before timeout?

Yes. The session clear action removes temporary files and in-memory index data immediately.

## Security and Privacy

### Where is document data processed?

Document processing occurs locally in the running environment through the backend and local LLM runtime.

### Is document content sent to cloud services?

No cloud upload flow is implemented in the current architecture.

### Is document data stored permanently?

No. SafeQueryAI uses temporary storage and in-memory session data that are removed on clear or expiry.

### Is telemetry enabled?

No external telemetry integration is documented in the current implementation.

### Does the app enforce local model inference?

Yes. Backend startup validates that the Ollama URL is a loopback address.

## API and Development

### Where can I find endpoint details?

See [API Reference](api-documentation.md).

### Is there a streaming endpoint?

Yes. Use `POST /api/sessions/{sessionId}/questions/stream` with `text/event-stream`.

### Are there tests?

Yes. Backend and frontend test suites are documented in [Testing](testing.md).

## Limits and Known Constraints

### Does SafeQueryAI support OCR for scanned PDFs?

Not in the current release.

### Is authentication built in?

Not in the current release.

### Is this documented as a multi-tenant production platform?

No. Current documentation describes a local, privacy-first architecture with session-based processing.

## Roadmap Questions

### Are cloud deployment templates included today?

Not in the current release scope.

### What improvements are planned next?

See [Roadmap](roadmap.md) for planned reliability, retrieval, and operational improvements.

## Reviewer Notes

- [REVIEW REQUIRED: verify whether any local environment uses a non-default backend port]
