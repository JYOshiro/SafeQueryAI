---
layout: default
title: Testing
---

# Testing

This page describes the current testing strategy for SafeQueryAI and how to run tests locally.

## Test Strategy

SafeQueryAI uses a risk-focused testing approach:

- Backend unit tests focus on core session, storage, extraction, indexing, retrieval, and answer orchestration services.
- Frontend tests focus on user-critical flows: session initialization, upload behavior, question submission, API error handling, and streaming responses.

## Current Test Inventory

| Project | Framework | Test Count Basis |
|---|---|---|
| Backend | xUnit + Moq + FluentAssertions | `[Fact]` and `[Theory]` attributes |
| Frontend | Vitest + React Testing Library | `it(...)` cases |

Observed in repository at documentation update time:

- Backend test cases: 55
- Frontend test cases: 32

- [REVIEW REQUIRED: re-verify counts after any test refactor or suite expansion]

## Backend Testing

### Tooling

| Package | Version | Purpose |
|---|---|---|
| xunit | 2.5.3 | Test framework |
| Moq | 4.20.70 | Mocking dependencies |
| FluentAssertions | 6.12.0 | Readable assertions |
| coverlet.collector | 6.0.0 | Coverage collection |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.0 | Integration-test host support |

### Test Locations

```
backend.Tests/
  UnitTest1.cs
  Services/
    SessionServiceTests.cs
    FileStorageServiceTests.cs
    TextExtractionServiceTests.cs
    DocumentIndexingServiceTests.cs
    VectorStoreServiceTests.cs
    QuestionAnsweringServiceTests.cs
```

### Run Backend Tests

```bash
dotnet test backend.Tests/
```

With coverage:

```bash
dotnet test backend.Tests/ --collect:"XPlat Code Coverage"
```

## Frontend Testing

### Tooling

| Package | Purpose |
|---|---|
| vitest | Test runner |
| @testing-library/react | Component rendering and querying |
| @testing-library/user-event | User interaction simulation |
| @testing-library/jest-dom | Extended DOM assertions |
| jsdom | Browser-like test environment |
| @vitest/coverage-v8 | Coverage output |

### Test Locations

```
frontend/src/test/
  App.test.tsx
  FileUploadPanel.test.tsx
  QuestionForm.test.tsx
  api.test.ts
```

### Run Frontend Tests

```bash
cd frontend
npm test
```

Watch mode:

```bash
npm run test:watch
```

With coverage:

```bash
npm run test:coverage
```

## Coverage and Quality Gates

Current baseline gates:

- All tests must pass before merge.
- Coverage reports should be generated for backend and frontend in CI.

- [REVIEW REQUIRED: define explicit coverage thresholds for backend and frontend]

## What Is Covered

- Session lifecycle and expiry behavior.
- Temporary storage and file lifecycle handling.
- CSV/PDF extraction logic behavior.
- Vector retrieval and question-answer orchestration.
- Frontend upload validation, question form states, API error handling, and stream parsing.

## Related Pages

- [Architecture](architecture.md)
- [API Reference](api-documentation.md)
- [Roadmap](roadmap.md)
- [Development](development.md)
