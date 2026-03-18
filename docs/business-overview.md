---
layout: default
title: Business Overview
---

# Business Overview

## Executive Summary

SafeQueryAI is a privacy-first architecture for document question-answering. It enables users to upload PDF and CSV files, ask natural-language questions, and receive grounded answers through a local LLM runtime.

The product is designed for scenarios where document confidentiality is critical and cloud processing is not acceptable.

## Problem Statement

Teams often need fast access to insights locked in documents, but many AI services require sending content to external infrastructure. This creates adoption barriers in privacy-sensitive environments.

SafeQueryAI addresses this by keeping document processing local, using session-based processing, temporary storage, and automatic cleanup.

## Target Users

- Students and assessors evaluating practical AI architecture and software engineering quality.
- Recruiters and portfolio reviewers assessing implementation maturity and product thinking.
- Developers who need a local reference implementation of document question-answering.
- Semi-technical stakeholders who need confidence in privacy-first architecture decisions.

## Functional Scope (Current)

- Create and manage temporary sessions.
- Upload PDF and CSV files to a session.
- Extract and index document text for retrieval.
- Ask natural-language questions against uploaded session documents.
- Return grounded answers with evidence snippets.
- Clear session data manually or through session timeout.

## Non-Functional Requirements (Current)

- Privacy-first architecture: no cloud upload of document content.
- Session-based processing with temporary storage lifecycle.
- Local LLM runtime enforced through loopback-only Ollama URL validation.
- Readable API and UI behavior suitable for demonstration and evaluation.
- Maintainable code structure across frontend and backend service layers.

## Business Benefits

- Reduces privacy risk for document AI use cases.
- Improves information retrieval speed compared with manual document scanning.
- Demonstrates end-to-end capability across API, frontend, and local AI runtime.
- Provides auditable behavior for temporary data handling.

## Assumptions

- Users can run Ollama locally and pull required models.
- Users accept that sessions are temporary and not persisted.
- File inputs are primarily text-based PDFs and standard CSV files.

## Constraints

- No persistent database for documents or conversation history.
- No built-in authentication/authorization in the current release.
- No OCR support for scanned image-only PDFs.
- Runtime behavior depends on local machine resources and model performance.

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Ollama unavailable locally | Question-answering quality or availability decreases | Fallback behavior and clear setup guidance |
| Large files or many chunks | Slower indexing and response times | Upload limits, session timeout, and roadmap optimization |
| Inconsistent environment setup | Demo failures and onboarding friction | Standardized endpoint and setup documentation |
| Misunderstood privacy boundaries | Reduced stakeholder trust | Dedicated Security & Privacy documentation |

## Success Criteria

- Users can complete local setup and first question-answer flow without cloud dependencies.
- Documentation remains consistent across setup, API, architecture, and FAQ pages.
- Privacy-first architecture is explicit, understandable, and technically credible.
- Testing guidance supports repeatable validation by reviewers.

## Out of Scope (Current Release)

- Cloud-hosted inference.
- Enterprise identity and access control.
- Persistent multi-user session management.
- Compliance certification claims.

## Related Pages

- [Getting Started](getting-started.md)
- [Architecture](architecture.md)
- [Security & Privacy](security-privacy.md)
- [Roadmap](roadmap.md)
