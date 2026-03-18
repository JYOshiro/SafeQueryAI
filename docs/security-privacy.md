---
layout: default
title: Security & Privacy
---

# Security & Privacy

This page defines the current security and privacy model for SafeQueryAI.

## Privacy Model Summary

SafeQueryAI uses privacy-first architecture:

- Document question-answering is session-based.
- Processing is local to the running environment.
- Storage is temporary and cleaned automatically.
- No external telemetry is implemented.

## What Data Is Processed

During an active session, the system processes:

- Uploaded file binaries (`.pdf`, `.csv`).
- Extracted document text.
- Chunked text segments.
- Embedding vectors.
- User-submitted question text.
- Generated answers and evidence snippets.

## Where Data Is Processed

- Frontend: browser session at `http://localhost:5173`.
- Backend: ASP.NET Core API process at `http://localhost:5000`.
- Local LLM runtime: Ollama at `http://localhost:11434`.

## What Is Stored Temporarily

- Uploaded files are written to session-scoped folders under temporary storage.
- Extracted text and vectors are held in application memory for retrieval.
- Session metadata is held in memory for session lifecycle management.

## Data Deletion Lifecycle

Data is removed by either of these events:

1. Manual clear through session delete endpoint.
2. Automatic expiry after 60 minutes of inactivity.

Cleanup actions include:

- Deleting session temporary files.
- Removing session vectors and indexed chunks.
- Removing session metadata from memory.

## What Is Not Sent Externally

SafeQueryAI is designed to avoid external document exfiltration:

- No cloud upload workflow is implemented.
- No external telemetry or analytics service is integrated.
- No external database is used for document retention.

The backend enforces a loopback-only Ollama base URL at startup, preventing non-local Ollama endpoints.

## Dependency and Trust Assumptions

SafeQueryAI depends on:

- Local machine security posture.
- Correct local Ollama installation and model provenance.
- Correct repository and dependency integrity.

Trust boundaries to communicate to stakeholders:

- Local processing does not remove endpoint security risk.
- Local users with machine access can still access temporary files during active sessions.
- Third-party model behavior remains dependent on selected local models.

## Known Limitations

- No built-in authentication and authorization controls in current release.
- No encryption-at-rest controls specific to temporary storage in current release.
- No OCR for scanned image-only PDFs.
- No formal compliance certification claims are made.

## Operational Controls (Current)

- Session timeout default: 60 minutes.
- File type restriction: `.pdf`, `.csv`.
- File size control: 20 MB configured, 25 MB request hard ceiling.
- CORS restricted to local development origins in backend configuration.

## Reviewer Notes

- [REVIEW REQUIRED: verify whether deployment environment overrides session timeout or file size defaults]
- [REVIEW REQUIRED: confirm whether infrastructure-level disk encryption is applied in target environments]

## Related Pages

- [Business Overview](business-overview.md)
- [Architecture](architecture.md)
- [Getting Started](getting-started.md)
- [FAQ](faq.md)
