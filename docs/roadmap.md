---
layout: default
title: Roadmap
---

# Roadmap

This roadmap captures the planned evolution of SafeQueryAI while preserving the privacy-first architecture.

## Planning Principles

- Maintain session-based processing and temporary storage.
- Improve reliability and answer quality before adding breadth features.
- Prioritize changes that reduce user risk and operational ambiguity.

## Current State

- Local document question-answering for PDF and CSV files.
- ASP.NET Core backend with session-scoped temporary storage.
- React + TypeScript frontend.
- Local Ollama runtime for embeddings and answer generation.
- Unit testing implemented for backend services and frontend behavior.

## Delivery Phases

## Phase 1: Reliability and Consistency

- Standardize API error contracts across sync and stream endpoints.
- Improve edge-case handling around session lifecycle and cleanup.
- Tighten documentation alignment with runtime configuration.

Success indicators:

- Fewer setup and integration support issues.
- Consistent frontend error handling behavior.
- Reduced ambiguity in technical review.

## Phase 2: Retrieval and UX Improvements

- Improve chunking and retrieval tuning for better answer relevance.
- Strengthen confidence handling for fallback answers.
- Improve streaming UX controls and cancellation behavior.

Success indicators:

- Improved answer relevance in manual scenario tests.
- Clearer user feedback for uncertain answers.

## Phase 3: Operational Maturity

- Add richer observability for indexing and question flows.
- Improve performance behavior for larger session datasets.
- Prepare optional self-host operational guidance for broader reviewer scenarios.

Success indicators:

- More stable behavior under heavier document workloads.
- Better diagnostics during demos and assessments.

## Backlog Candidates (Future)

These items are not in the current release scope:

- OCR support for scanned/image-only PDFs.
- Additional file types such as XLSX.
- Optional authenticated deployment profile.
- Optional persistent deployment mode for enterprise scenarios.

## Scope Guardrails

Roadmap planning does not change current product constraints:

- No cloud document processing in current implementation.
- No compliance certification claims.
- No implied multi-tenant production readiness.

## Related Pages

- [Business Overview](business-overview.md)
- [Architecture](architecture.md)
- [Testing](testing.md)
- [Deployment](deployment.md)
