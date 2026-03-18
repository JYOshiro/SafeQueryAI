---
layout: default
title: Development
---

# Development Guide

This guide covers development setup, contributing guidelines, and code structure for SafeQueryAI.

## Development Environment Setup

### Prerequisites

- .NET SDK 8.0+
- Node.js 18+
- Ollama installed and running
- Git
- IDE: Visual Studio, Visual Studio Code, or Rider

### Clone & Setup

```bash
# Clone repository
git clone https://github.com/yourusername/SafeQueryAI.git
cd SafeQueryAI

# Start Ollama
ollama serve

# In another terminal, pull models
ollama pull nomic-embed-text
ollama pull llama3.2
```

### Run Development Servers

**Terminal 1 — Backend**:
```bash
cd backend
dotnet watch run
```

**Terminal 2 — Frontend**:
```bash
cd frontend
npm install
npm run dev
```

Application should be available at `http://localhost:5173`

## Project Structure

### Backend Architecture

```
backend/
├── Controllers/          # HTTP endpoints
│   ├── FilesController.cs
│   ├── QuestionsController.cs
│   ├── SessionsController.cs
│   └── HealthController.cs
├── Services/             # Business logic
│   ├── DocumentIndexingService.cs
│   ├── OllamaService.cs
│   ├── QuestionAnsweringService.cs
│   ├── SessionService.cs
│   ├── TextExtractionService.cs
│   ├── VectorStoreService.cs
│   └── Interfaces/       # Service contracts
├── Models/               # Domain entities
│   ├── SessionInfo.cs
│   ├── DocumentChunk.cs
│   └── StoredFileInfo.cs
├── Contracts/            # DTOs for API
│   ├── AskQuestionRequest.cs
│   ├── AnswerStreamChunk.cs
│   └── ...
├── Program.cs            # DI and middleware setup
└── appsettings.json      # Configuration
```

### Frontend Architecture

```
frontend/
├── src/
│   ├── components/       # React components
│   │   ├── App.tsx       # Root component
│   │   ├── QuestionForm.tsx
│   │   ├── FileUploadPanel.tsx
│   │   └── AnswerPanel.tsx
│   ├── services/         # API client
│   │   └── api.ts        # Typed API wrapper
│   ├── types/            # TypeScript interfaces
│   │   └── api.ts        # API types
│   ├── styles/           # CSS files
│   └── main.tsx          # Entry point
├── vite.config.ts        # Build config
├── tsconfig.json         # TS config
└── package.json          # Dependencies
```

## Code Style Guidelines

### C# (.NET)

- **Naming**: PascalCase for classes, methods, properties; camelCase for parameters
- **Async**: Use `async`/`await` consistently
- **DI**: Inject dependencies through constructor
- **Error handling**: Use typed exceptions with meaningful messages
- **Logging**: Use built-in `ILogger<T>` from DI

Example:

```csharp
public class DocumentIndexingService : IDocumentIndexingService
{
    private readonly ILogger<DocumentIndexingService> _logger;
    private readonly IOllamaService _ollamaService;

    public DocumentIndexingService(
        ILogger<DocumentIndexingService> logger,
        IOllamaService ollamaService)
    {
        _logger = logger;
        _ollamaService = ollamaService;
    }

    public async Task<List<DocumentChunk>> IndexDocumentAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Indexing document: {FilePath}", filePath);
            // Implementation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document");
            throw;
        }
    }
}
```

### TypeScript/React

- **Naming**: PascalCase for components; camelCase for functions/variables
- **Props**: Define as interfaces, use `React.FC<Props>`
- **State**: Group related state, consider hooks
- **Comments**: Explain why, not what

Example:

```typescript
interface QuestionFormProps {
  sessionId: string;
  onSubmit: (question: string) => void;
  isLoading: boolean;
}

export const QuestionForm: React.FC<QuestionFormProps> = ({
  sessionId,
  onSubmit,
  isLoading
}) => {
  const [question, setQuestion] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(question);
    setQuestion('');
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        value={question}
        onChange={(e) => setQuestion(e.target.value)}
        disabled={isLoading}
      />
      <button type="submit" disabled={isLoading}>
        Ask
      </button>
    </form>
  );
};
```

## Testing

### Backend Unit Tests

Create test files in `backend/Tests/`:

```csharp
[TestFixture]
public class DocumentIndexingServiceTests
{
    private Mock<IOllamaService> _ollamaServiceMock;
    private DocumentIndexingService _service;

    [SetUp]
    public void Setup()
    {
        _ollamaServiceMock = new Mock<IOllamaService>();
        _service = new DocumentIndexingService(_ollamaServiceMock.Object);
    }

    [Test]
    public async Task IndexDocument_WithValidFile_ReturnsChunks()
    {
        // Arrange
        var filePath = "test.pdf";

        // Act
        var result = await _service.IndexDocumentAsync(filePath);

        // Assert
        Assert.IsNotEmpty(result);
    }
}
```

Run tests:

```bash
dotnet test backend/
```

### Frontend Unit Tests

Add Vitest to `frontend/package.json`:

```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom
```

Create test file:

```typescript
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QuestionForm } from './QuestionForm';

describe('QuestionForm', () => {
  it('submits question when form is submitted', async () => {
    const onSubmit = vi.fn();
    render(
      <QuestionForm
        sessionId="test-id"
        onSubmit={onSubmit}
        isLoading={false}
      />
    );

    const input = screen.getByRole('textbox');
    await userEvent.type(input, 'What is AI?');
    await userEvent.click(screen.getByRole('button'));

    expect(onSubmit).toHaveBeenCalledWith('What is AI?');
  });
});
```

Run tests:

```bash
npm run test
```

## Common Development Tasks

### Add a New API Endpoint

1. **Create request contract** in `Contracts/`:
   ```csharp
   public class MyRequest { }
   ```

2. **Create response contract** in `Contracts/`:
   ```csharp
   public class MyResponse { }
   ```

3. **Add controller method**:
   ```csharp
   [HttpPost("my-endpoint")]
   public async Task<MyResponse> MyEndpoint(MyRequest request)
   {
       // Implementation
   }
   ```

4. **Update frontend API client** in `services/api.ts`:
   ```typescript
   myEndpoint: (data: MyRequest): Promise<MyResponse> => 
     fetch('/api/my-endpoint', { method: 'POST', body: JSON.stringify(data) })
   ```

### Add a New React Component

1. Create file in `src/components/`:
   ```typescript
   interface MyComponentProps { }
   
   export const MyComponent: React.FC<MyComponentProps> = (props) => {
     return <div>Component</div>;
   };
   ```

2. Import and use in `App.tsx`:
   ```typescript
   import { MyComponent } from './components/MyComponent';
   ```

3. Add styling to `styles/app.css`

### Update Dependencies

**Backend**:
```bash
cd backend
dotnet add package NameOfPackage
```

**Frontend**:
```bash
cd frontend
npm install package-name
```

## Debugging

### Backend Debugging in VS Code

Create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/backend/bin/Debug/net8.0/SafeQueryAI.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/backend",
      "stopAtEntry": false,
      "serverReadyAction": {
        "port": 5000,
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      }
    }
  ]
}
```

### Frontend Debugging

Use React DevTools browser extension or:

```typescript
// In component
console.log('Debug info:', value);
debugger; // Pauses execution
```

### Logging

**Backend**:
```csharp
_logger.LogInformation("Info message");
_logger.LogWarning("Warning: {Details}", details);
_logger.LogError(ex, "Error occurred");
```

**Frontend**:
```typescript
console.log('Message');
console.debug('Debug info', obj);
console.error('Error', error);
```

## Contributing

### Workflow

1. **Create branch**: `git checkout -b feature/my-feature`
2. **Make changes**: Follow code style guidelines
3. **Test locally**: `dotnet test` and `npm test`
4. **Commit**: `git commit -m "Add feature: description"`
5. **Push**: `git push origin feature/my-feature`
6. **Create PR**: Open pull request with description

### PR Checklist

- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes (or clearly documented)
- [ ] Works with existing code
- [ ] No sensitive data committed

## Build & Release

### Build for Production

**Backend**:
```bash
cd backend
dotnet publish -c Release -o ../publish
```

**Frontend**:
```bash
cd frontend
npm run build
```

### Version Bumping

Update version in:
- `backend/SafeQueryAI.Api.csproj`: `<Version>1.0.0</Version>`
- `frontend/package.json`: `"version": "1.0.0"`

Tag and push:
```bash
git tag v1.0.0
git push origin v1.0.0
```

## Performance & Optimization

### Backend

- Use async/await consistently
- Implement caching for embeddings
- Monitor Ollama API latency
- Profile with `dotnet-trace`

### Frontend

- Code splitting with React.lazy
- Memoize expensive components
- Debounce input handlers
- Use React DevTools Profiler

## Resources

- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [Ollama API Docs](https://github.com/ollama/ollama/blob/main/docs/api.md)

## Getting Help

- Check existing issues on GitHub
- Create a new issue with clear description
- Discuss in pull request comments
- Review similar code patterns in codebase
