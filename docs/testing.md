---
layout: default
title: Testing Guide
---

# Testing Guide

This page documents the unit testing strategy, tooling, and how to run tests for both the backend (.NET) and frontend (React/TypeScript) projects.

---

## Overview

SafeQueryAI uses a risk-based unit testing approach focused on the core service and component layers. Tests were introduced as a quality gate before implementing infrastructure improvements from the [Implementation Plan](implementation-plan.md).

| Project | Framework | Total Tests |
|---------|-----------|-------------|
| Backend (ASP.NET Core 8) | xUnit + Moq + FluentAssertions | 54 |
| Frontend (React / TypeScript) | Vitest + React Testing Library | 32 |
| **Total** | | **86** |

---

## Backend Tests

### Tooling

| Package | Version | Purpose |
|---------|---------|---------|
| xUnit | 2.5.3 | Test runner and assertions |
| Moq | 4.20.70 | Mocking service interfaces |
| FluentAssertions | 6.12.0 | Readable assertion syntax |
| coverlet.collector | 6.0.0 | Code coverage collection |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Full integration test support |

### Test project location

```
backend.Tests/
  SafeQueryAI.Tests.csproj
  Services/
    SessionServiceTests.cs          (12 tests)
    VectorStoreServiceTests.cs      (10 tests)
    QuestionAnsweringServiceTests.cs (9 tests)
    DocumentIndexingServiceTests.cs  (8 tests)
    FileStorageServiceTests.cs       (9 tests)
    TextExtractionServiceTests.cs    (9 tests)
```

### Running backend tests

```bash
# Run all tests
dotnet test backend.Tests/

# Run with verbose output
dotnet test backend.Tests/ --logger "console;verbosity=normal"

# Collect code coverage
dotnet test backend.Tests/ --collect:"XPlat Code Coverage"
```

### What is tested

| Test Class | Coverage Focus |
|------------|---------------|
| `SessionServiceTests` | Session creation (unique IDs, timestamps), retrieval, file association, clearing, expiry detection |
| `VectorStoreServiceTests` | Cosine similarity ranking, top-k selection, session isolation, file/session removal, edge cases (zero vectors, mismatched dimensions) |
| `QuestionAnsweringServiceTests` | RAG path (Ollama + vector lookup), evidence population, graceful fallback on HTTP errors, streaming token/final-chunk sequencing |
| `DocumentIndexingServiceTests` | Chunking, embedding calls, empty-document handling, cancellation propagation, removal delegation |
| `FileStorageServiceTests` | File persistence with session scoping, extension preservation, deletion, missing-directory safety |
| `TextExtractionServiceTests` | CSV parsing (headers, rows, extra/missing columns, whitespace trimming), empty and non-existent file handling |

---

## Frontend Tests

### Tooling

| Package | Purpose |
|---------|---------|
| Vitest | Fast, Vite-native test runner |
| @testing-library/react | Component rendering and querying |
| @testing-library/user-event | Realistic user interaction simulation |
| @testing-library/jest-dom | Extended DOM matchers |
| jsdom | Browser DOM environment |
| @vitest/coverage-v8 | V8-based coverage reporting |

### Test file locations

```
frontend/src/test/
  setup.ts                    — jest-dom global matchers
  QuestionForm.test.tsx       (9 tests)
  FileUploadPanel.test.tsx    (9 tests)
  api.test.ts                 (9 tests)
  App.test.tsx                (5 tests)
```

### Running frontend tests

```bash
cd frontend

# Run all tests once
npm test

# Watch mode (re-runs on file change)
npm run test:watch

# Collect coverage report
npm run test:coverage
```

Coverage output is written to `frontend/coverage/` with a text summary and an `lcov.info` file for CI integration.

### What is tested

| Test File | Coverage Focus |
|-----------|---------------|
| `QuestionForm.test.tsx` | Textarea and button rendering, disabled states (no files, asking, empty input), trimmed submission, whitespace-only guard, button label transitions |
| `FileUploadPanel.test.tsx` | Drop zone rendering, uploading/analysing phase text, error message display, file type validation (rejects unsupported formats), file size enforcement, successful PDF and CSV upload callbacks |
| `api.test.ts` | `request()` happy path, non-2xx error extraction (error field, detail field, status text fallback), SSE stream token delivery, done-event metadata resolution, premature stream end error, malformed event resilience |
| `App.test.tsx` | Loading state before session init, successful session render, backend-unreachable error banner, session ID display, clear-session → fresh-session flow |

---

## Quality Gates

| Gate | Target |
|------|--------|
| All tests must pass (CI) | 100% green |
| Backend service layer coverage | ≥ 80% line coverage |
| Frontend component + API coverage | ≥ 70% line coverage |
| No regressions on merge to `main` | Gate on PR workflow |

---

## Coverage Commands

```bash
# Backend — generates coverage XML in TestResults/
dotnet test backend.Tests/ --collect:"XPlat Code Coverage"

# Frontend — generates lcov.info and text summary
cd frontend && npm run test:coverage
```

---

## Related Documents

- [Implementation Plan](implementation-plan.md) — delivery phases and acceptance criteria
- [Development Guide](development.md) — build and contribution workflow
- [Architecture](architecture.md) — system design and service dependency map
