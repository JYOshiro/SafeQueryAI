---
layout: default
title: FAQ
---

# Frequently Asked Questions

## Installation & Setup

### Q: Do I need to install anything besides Ollama?
**A:** Yes, you need:
- .NET SDK 8.0+ (for backend)
- Node.js 18+ (for frontend)
- Ollama (for AI models)

All three are free and cross-platform.

### Q: What if I don't have Ollama installed?
**A:** SafeQueryAI will still work with keyword-based matching for document browsing. However, Q&A functionality requires Ollama. Download it from [ollama.com](https://ollama.com).

### Q: Can I use different AI models than llama3.2 and nomic-embed-text?
**A:** Yes! Any models available in Ollama can be used:
1. Pull your preferred models: `ollama pull your-model`
2. Update `appsettings.json`:
   ```json
   {
     "OllamaSettings": {
       "GenerationModel": "your-model",
       "EmbeddingModel": "your-embedding-model"
     }
   }
   ```
3. Restart the backend

### Q: What are the system requirements?
**A:** Minimum:
- 8 GB RAM
- 4 GB disk space for models
- CPU with support for the chosen LLM

Recommended:
- 16 GB RAM
- GPU (NVIDIA with CUDA, or Apple M1/M2 for native inference)
- SSD for faster file uploads

### Q: Can I run SafeQueryAI on macOS/Linux/Windows?
**A:** Yes! All components are cross-platform:
- .NET Core runs on all platforms
- Node.js runs on all platforms
- Ollama runs on macOS, Linux, and Windows

## Usage

### Q: How large can uploaded files be?
**A:** Default maximum is **50 MB** per file. This is configurable in `appsettings.json`:
```json
{
  "FileStorageSettings": {
    "MaxFileSizeBytes": 52428800
  }
}
```

### Q: What file formats are supported?
**A:** Currently:
- **PDF** files (text-based, not scanned images)
- **CSV** files (spreadsheet data)

For scanned PDFs, consider using OCR preprocessing.

### Q: How long does a session last?
**A:** Default session duration is **60 minutes** of inactivity. This means:
- If you're actively asking questions, the session continues
- If there's no activity for 60 minutes, the session expires and files are deleted
- This is configurable in `appsettings.json`:
  ```json
  {
    "SessionSettings": {
      "SessionTimeoutMinutes": 60
    }
  }
  ```

### Q: What happens to my data after I upload a file?
**A:** Your data is:
- Stored **only in the temporary session folder** (`./TempSessions`)
- **Never sent** to cloud services or databases
- **Deleted automatically** when the session expires
- This is core to SafeQueryAI's privacy guarantee

### Q: Can I delete my data before session expiry?
**A:** Yes! Use the **"Clear Session"** button in the frontend, or call the API:
```bash
DELETE /api/sessions/{sessionId}
```

### Q: Why is my answer not accurate?
**A:** Several reasons:
- **Model limitations**: Smaller models are less capable
- **Chunking**: Document chunking might split relevant context
- **Similarity threshold**: Retrieved chunks may not be most relevant
- **Document quality**: Poor OCR or formatted PDFs may extract incorrectly

Try adjusting RAG parameters in `appsettings.json` (see [Configuration](configuration.md)).

### Q: Can I use CSV files?
**A:** Yes! CSVs are parsed and formatted for context. Each row/column is treated as contextual information for Q&A.

## Performance & Scaling

### Q: Why are responses slow?
**A:** Typical response time is **5-30 seconds**, depending on:
- Model size (llama3.2 vs larger models)
- Document complexity
- System resources
- Network latency to Ollama

To improve performance:
- Use a smaller, faster model
- Reduce `RagSettings.TopKResults` in config
- Increase `RagSettings.SimilarityThreshold`
- Use a GPU for Ollama inference

### Q: Can I handle multiple users concurrently?
**A:** Yes, the current implementation supports multiple concurrent sessions. Each user gets independent session storage and vector stores.

For production scale (100+ concurrent users):
- Add persistent database backend
- Use distributed vector store (Pinecone, Weaviate)
- Deploy multiple backend instances with load balancing

### Q: How many documents can I upload per session?
**A:** Technically unlimited, but practically:
- **Memory limited**: In-memory vector store is limited by RAM
- **Typical limit**: 100-500 documents depending on size and system RAM
- **Recommended**: Keep to 10-50 documents per session for best performance

## Troubleshooting

### Q: Backend won't start - "connection refused"
**A:** Ollama is likely not running. Fix:
```bash
ollama serve
```

Or ensure the backend is configured to work without Ollama.

### Q: "Model not found" error
**A:** Models aren't pulled. Fix:
```bash
ollama pull nomic-embed-text
ollama pull llama3.2
ollama list  # Verify
```

### Q: Frontend can't reach backend (CORS error)
**A:** CORS configuration issue. Check:
1. Backend is running on correct port
2. Frontend API URL is correct
3. CORS policy includes frontend origin in `Program.cs`

### Q: Files aren't uploading
**A:** Possible causes:
- File is too large (>50 MB)
- File format not supported (must be PDF or CSV)
- Backend storage path doesn't exist
- Insufficient disk space

Check browser console and backend logs for details.

### Q: Session keeps expiring
**A:** Either:
- You're inactive for 60+ minutes (sessions expire)
- Backend crashed or restarted
- System ran out of disk space

Increase session timeout in `appsettings.json` if needed.

### Q: My answer is just "I don't know"
**A:** Reasons:
- Document doesn't contain the information
- Question is too vague or unrelated
- Similarity threshold is too high
- Chunks weren't retrieved correctly

Try:
- Rephrasing the question more specifically
- Lowering `SimilarityThreshold` in config
- Verifying document content

### Q: High memory usage
**A:** In-memory vector store can consume significant RAM with many documents. Solutions:
- Reduce number of active sessions
- Use a persistent vector store for production
- Monitor with system tools: `free -h` (Linux), Task Manager (Windows)

## Privacy & Security

### Q: Is my data really private?
**A:** Yes, completely:
- **No cloud uploads**: All processing happens locally
- **No database**: No persistent storage beyond session
- **No telemetry**: No tracking or analytics
- **Automatic cleanup**: Files deleted when session expires

Your data never leaves your machine.

### Q: Can I deploy behind a firewall?
**A:** Yes! SafeQueryAI is designed for on-premise deployment. It works offline once models are downloaded.

### Q: What about authentication/authorization?
**A:** Current version has no built-in auth. For production, add:
- JWT authentication
- API key validation
- RBAC (role-based access control)

See [Configuration](configuration.md#authentication--authorization).

### Q: Should I use HTTPS in production?
**A:** Yes, absolutely. See [Deployment](deployment.md#httpstls) for setup.

## Development & Contribution

### Q: How can I contribute?
**A:** Fork the repo, make changes, and submit a PR. See [Development](development.md#contributing).

### Q: Can I use SafeQueryAI for commercial purposes?
**A:** Check the LICENSE file of the project. Most open-source projects allow commercial use with proper attribution.

### Q: Is there a roadmap?
**A:** Check GitHub Issues and Projects for planned features and discussions.

### Q: How do I report a bug?
**A:** Open an issue on GitHub with:
- Clear description of problem
- Steps to reproduce
- Error messages and logs
- System information (OS, versions)

## API & Integration

### Q: Can I integrate SafeQueryAI into my app?
**A:** Yes! Use the REST API. See [API Documentation](api-documentation.md).

### Q: What are rate limits?
**A:** Default limits per session:
- 10 file uploads per session
- 100 questions per hour
- 50 MB max file size

Modify in `appsettings.json`.

### Q: Can I use the API from my frontend framework?
**A:** Yes! The API is standard REST with JSON. Works with any framework (Vue, Angular, Svelte, etc.).

### Q: Do I need authentication to use the API?
**A:** Not by default. For production, add authentication (see [Configuration](configuration.md)).

## Deployment

### Q: Can I deploy to AWS/Azure/Heroku?
**A:** Yes! See [Deployment](deployment.md) for detailed instructions for each platform.

### Q: What about containerization?
**A:** Use Docker. See [Deployment](deployment.md#docker-deployment) for Dockerfile and docker-compose setups.

### Q: Can I scale to many users?
**A:** Yes, but requires architectural changes. See [Deployment - Scaling Considerations](deployment.md#scaling-considerations).

## Licensing & Legal

### Q: What license does SafeQueryAI use?
**A:** Check the LICENSE file in the repository. Common licenses are MIT, Apache 2.0, or GPL.

### Q: Can I modify and redistribute?
**A:** Depends on the license. MIT/Apache allow it; GPL requires source disclosure.

### Q: Are there any dependencies with restrictive licenses?
**A:** Refer to each dependency's license file. Common libraries (React, .NET Core) use permissive licenses.

## Still Have Questions?

- Check other documentation pages (Getting Started, API Docs, etc.)
- Search GitHub Issues
- Create a new GitHub Issue with your question
- Check the project README for contact information

---

Last updated: March 2026
