---
layout: default
title: SafeQueryAI Documentation
---

# SafeQueryAI Documentation

Welcome to the SafeQueryAI documentation site. SafeQueryAI is a privacy-first, local document Q&A application powered by Retrieval-Augmented Generation (RAG).

## 🎯 Quick Start

- **New to SafeQueryAI?** Start with [Getting Started](getting-started.md)
- **Want to understand the system?** Read about the [Architecture](architecture.md)
- **Need delivery roadmap?** Review the [Implementation Plan](implementation-plan.md)
- **Building an integration?** Check the [API Documentation](api-documentation.md)
- **Customizing the UI?** See the [Frontend Guide](frontend-guide.md)

## 📚 Documentation Sections

| Section | Purpose |
|---------|---------|
| [Getting Started](getting-started.md) | Installation, prerequisites, running locally |
| [Implementation Plan](implementation-plan.md) | Business roadmap, delivery phases, acceptance criteria |
| [Architecture](architecture.md) | System design, RAG pipeline, components overview |
| [API Documentation](api-documentation.md) | Backend endpoints, request/response formats |
| [Frontend Guide](frontend-guide.md) | React components, styling, state management |
| [Configuration](configuration.md) | Environment variables, settings, customization |
| [Deployment](deployment.md) | Production setup, Docker, cloud hosting options |
| [Development](development.md) | Contributing, build process, project structure |
| [FAQ](faq.md) | Common questions and troubleshooting |

## 🔐 Key Features

- **Privacy-First**: No data collection or cloud uploads
- **Local LLM**: Powered by Ollama with local models
- **Session-Based**: Temporary storage, automatic cleanup
- **RAG-Powered**: Answers grounded in uploaded documents
- **Web Interface**: Clean, responsive React frontend
- **Multi-Format**: Supports PDF and CSV files

## 📖 Technology Stack

**Frontend**: React 19, TypeScript, Vite  
**Backend**: ASP.NET Core 8, .NET Web API  
**AI Runtime**: Ollama (local)  
**Models**: `nomic-embed-text` (embedding), `llama3.2` (generation)  
**Storage**: In-memory (session-scoped)

## 🚀 Getting Help

- Check the [FAQ](faq.md) for common issues
- Review [Configuration](configuration.md) for setup problems
- See [Development](development.md) for contribution guidelines
- Open an issue on GitHub

---

**Last updated**: March 2026
