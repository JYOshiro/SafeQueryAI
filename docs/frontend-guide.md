---
layout: default
title: Frontend Guide
---

# Frontend Guide

The SafeQueryAI frontend is a modern React 19 application built with TypeScript and Vite. This guide covers the component structure, key patterns, and customization options.

## Project Structure

```
frontend/
├── src/
│   ├── components/
│   │   ├── App.tsx                 # Root component
│   │   ├── QuestionForm.tsx        # Question input form
│   │   ├── FileUploadPanel.tsx     # File upload
│   │   ├── AnswerPanel.tsx         # Answer display
│   │   ├── SessionInfo.tsx         # Session metadata
│   │   ├── UploadedFileList.tsx    # Files listing
│   │   └── PrivacyBanner.tsx       # Privacy notice
│   ├── services/
│   │   └── api.ts                  # API client
│   ├── types/
│   │   └── api.ts                  # TypeScript interfaces
│   ├── styles/
│   │   ├── app.css                 # Global styles
│   │   └── index.css               # Base styles
│   ├── App.css                     # App component styles
│   ├── index.css                   # Root styles
│   ├── main.tsx                    # Entry point
│   └── App.tsx                     # Main app
├── index.html                      # HTML template
├── vite.config.ts                  # Vite configuration
├── tsconfig.json                   # TypeScript config
└── package.json                    # Dependencies
```

## Key Components

### App.tsx
The root component managing overall application state and layout.

**State**:
- `sessionId`: Current session UUID
- `uploadedFiles`: List of uploaded files
- `isLoading`: Loading indicator

### QuestionForm.tsx
Handles question input and submission.

**Props**:
```typescript
interface QuestionFormProps {
  sessionId: string;
  onAnswer: (answer: string) => void;
  isLoading: boolean;
}
```

**Features**:
- Text input with character counter
- SSL check for disabled state
- Enter to submit (Ctrl+Enter for new line)

### FileUploadPanel.tsx
Drag-and-drop file upload interface.

**Supported Formats**:
- PDF files (`.pdf`)
- CSV files (`.csv`)

**Max Size**: 50 MB per file

### AnswerPanel.tsx
Displays streaming answers in real-time.

**Features**:
- Streaming text display
- Loading animation
- Error boundary
- Copy answer button

## API Client

The `services/api.ts` file provides typed API methods:

```typescript
export const api = {
  sessions: {
    create(): Promise<CreateSessionResponse>,
    get(sessionId: string): Promise<SessionInfo>,
    delete(sessionId: string): Promise<ClearSessionResponse>
  },
  files: {
    upload(sessionId: string, file: File): Promise<FileUploadResponse>,
    list(sessionId: string): Promise<SessionFilesResponse>
  },
  questions: {
    ask(sessionId: string, question: string): AsyncIterable<AnswerStreamChunk>
  }
};
```

### Example Usage

```typescript
import { api } from './services/api';

// Create session
const { sessionId } = await api.sessions.create();

// Upload file
const file = fileInput.files[0];
await api.files.upload(sessionId, file);

// Ask question with streaming
for await (const chunk of api.questions.ask(sessionId, question)) {
  if (chunk.status === 'complete') {
    console.log('Answer complete');
  } else {
    console.log(chunk.chunk); // Append to answer text
  }
}
```

## Styling Guide

The frontend uses a minimal CSS-in-JS approach with separate stylesheets.

### Color Palette

|  | Usage |
|--|-------|
| **Primary** | Buttons, links (configure in `app.css`) |
| **Text** | Body text, headings |
| **Border** | Input borders, separators |
| **Background** | Page background, panels |

### Adding Styles

1. **Global styles**: Update `styles/app.css`
2. **Component styles**: Add CSS modules or inline styles in component file
3. **Responsive**: Use CSS media queries for mobile/desktop

Example:

```css
/* app.css */
.question-form {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
}

@media (max-width: 768px) {
  .question-form {
    flex-direction: column;
  }
}
```

## TypeScript Types

All API responses are typed in `types/api.ts`:

```typescript
export interface CreateSessionResponse {
  sessionId: string;
  createdAt: string;
  expiresAt: string;
}

export interface AnswerStreamChunk {
  chunk?: string;
  status: 'generating' | 'complete' | 'error';
  error?: string;
}
```

Use these types to keep your components type-safe.

## State Management

Currently using React's built-in `useState`. For larger apps, consider:

- Redux for centralized state
- Zustand for minimal boilerplate
- Jotai for atomic state

## Development

### Environment Variables

Create a `.env` file in the `frontend` directory:

```env
VITE_API_URL=http://localhost:7180/api
```

### Build Commands

```bash
# Development server with HMR
npm run dev

# Production build
npm run build

# Preview production build
npm run preview

# Lint with ESLint
npm run lint
```

## Performance Optimization

- **Code splitting**: Components are lazy-loaded where possible
- **Image optimization**: Use `<img loading="lazy">`
- **Debouncing**: Question input is debounced to prevent rapid submissions
- **Streaming**: Answers display incrementally as received

## Accessibility

- Semantic HTML (`<button>`, `<form>`, etc.)
- ARIA labels for screen readers
- Keyboard navigation support (Tab, Enter, Escape)
- Color contrast meets WCAG AA standard

## Customization Tips

### Change API Endpoint
Edit `services/api.ts` or set `VITE_API_URL` environment variable.

### Customize Colors
Update CSS variables in `styles/app.css`:

```css
:root {
  --primary-color: #0066cc;
  --border-color: #ddd;
  --background-color: #fff;
}
```

### Add a New Component

1. Create component file in `components/`
2. Define props interface in file
3. Import and use in `App.tsx`
4. Add styles to `app.css`

### Change Branding

Update these files:
- `index.html`: Title and favicon
- `src/components/App.tsx`: Logo/header
- `styles/app.css`: Colors and fonts

## Browser Support

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

## Troubleshooting

**Backend connection fails**
- Ensure backend is running on configured URL
- Check CORS configuration in backend

**Styling looks wrong**
- Clear browser cache (Ctrl+Shift+Delete)
- Check CSS file paths

**Questions not streaming**
- Check browser console for errors
- Ensure Ollama is running
- Verify backend logs
