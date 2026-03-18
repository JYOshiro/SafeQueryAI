---
layout: default
title: Getting Started
---

# Getting Started with SafeQueryAI

This guide will get you up and running with SafeQueryAI in minutes.

## Prerequisites

Ensure you have the following installed:

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js | 18+ | [nodejs.org](https://nodejs.org) |
| Ollama | latest | [ollama.com](https://ollama.com) |

## Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/SafeQueryAI.git
cd SafeQueryAI
```

## Step 2: Set Up Ollama

SafeQueryAI requires Ollama running with two models:

### Install Ollama

Download from [ollama.com](https://ollama.com) and follow installation steps for your OS.

### Pull Required Models

```bash
ollama pull nomic-embed-text
ollama pull llama3.2
```

### Start Ollama Service

```bash
ollama serve
```

Ollama will be available at `http://localhost:11434`

## Step 3: Backend Setup

```bash
cd backend

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the API
dotnet run
```

The API will be available at `https://localhost:7180` (or as configured in `appsettings.json`)

## Step 4: Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start the development server
npm run dev
```

The frontend will typically be available at `http://localhost:5173`

## Step 5: Verify Installation

1. **Open the browser**: Navigate to `http://localhost:5173`
2. **Upload a document**: Try uploading a PDF or CSV file
3. **Ask a question**: Submit a natural-language question about the document
4. **Review the answer**: The AI should provide an answer grounded in your document

## Troubleshooting

### Ollama Connection Failed
- Ensure Ollama is running: `ollama serve`
- Check if models are available: `ollama list`
- Verify the API endpoint in backend configuration

### Frontend Cannot Connect to Backend
- Check `frontend/src/services/api.ts` for the correct backend URL
- Ensure CORS is properly configured in backend
- Check browser console for detailed error messages

### Models Not Found
- Verify models are pulled: `ollama list`
- Check that Ollama has been running long enough for models to load

## What's Next?

- [Read the Architecture](architecture.md) to understand how SafeQueryAI works
- [Check the API Documentation](api-documentation.md) for available endpoints
- [Review Configuration](configuration.md) for customization options
