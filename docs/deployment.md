---
layout: default
title: Deployment
---

# Deployment

This page documents deployment guidance for the current SafeQueryAI implementation.

## Current Deployment Scope

Supported and documented in this release:

- Local development deployment.
- Single-machine self-host style operation.

Not currently implemented as first-class project artifacts:

- Containerized deployment files in repository.
- Managed cloud deployment templates.
- Built-in production authentication profile.

## Runtime Components

| Component | Default |
|---|---|
| Frontend | `http://localhost:5173` |
| Backend API | `http://localhost:5000/api` |
| Swagger (development) | `http://localhost:5000/swagger` |
| Ollama | `http://localhost:11434` |

## Local Deployment Steps

1. Start Ollama.
2. Pull required models.
3. Start backend API.
4. Start frontend development server.

Reference commands are in [Getting Started](getting-started.md).

## Configuration Baseline

From backend configuration:

- Session timeout: `60` minutes.
- Max file size: `20 MB`.
- Temporary storage path: `TempSessions`.
- Ollama endpoint: local loopback URL.

## Operational Notes

- Backend startup enforces local-only Ollama URL validation.
- Session expiry service removes inactive session data.
- Manual session clear removes files and in-memory indexing data immediately.
- Vite proxy handles local frontend-to-backend API routing.

## Security and Privacy in Deployment Context

- Keep Ollama bound to local machine unless risk-assessed.
- Do not expose temporary session storage beyond required local access.
- Treat local host security controls as part of the trust boundary.

See [Security & Privacy](security-privacy.md).

## Deployment Risks

| Risk | Impact | Control |
|---|---|---|
| Misconfigured backend URL | Frontend/API integration failure | Keep endpoint references standardized |
| Ollama unavailable | Question-answering degraded or unavailable | Pre-run model and service checks |
| Resource constraints | Slow indexing or responses | File size limits and session cleanup |
| Overstated production readiness | Stakeholder misunderstanding | Explicitly document current scope |

## Reviewer Notes

- [REVIEW REQUIRED: confirm deployment target conventions for assessor environment]
- [REVIEW REQUIRED: confirm whether HTTPS is required in demonstration environment]

## Future Deployment Enhancements (Roadmap)

Potential future work, not part of current release:

- Container packaging and compose workflow.
- Repeatable self-host scripts.
- Optional authenticated deployment profile.
- Deployment observability baseline (metrics + structured logs).

See [Roadmap](roadmap.md) for planning detail.
