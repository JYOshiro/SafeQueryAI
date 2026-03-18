---
layout: default
title: Frontend Guide
---

# Frontend Guide

The SafeQueryAI frontend is a React + TypeScript application designed for clear session-based workflows: upload, ask, review, clear.

## Objectives

- Provide an understandable UI for document question-answering.
- Keep privacy-first architecture visible in user interactions.
- Integrate reliably with backend REST and SSE endpoints.

## Project Structure

```
frontend/
├── src/
│   ├── components/
│   │   ├── App.tsx
│   │   ├── QuestionForm.tsx
│   │   ├── FileUploadPanel.tsx
│   │   ├── AnswerPanel.tsx
│   │   ├── SessionInfo.tsx
│   │   ├── UploadedFileList.tsx
│   │   └── PrivacyBanner.tsx
│   ├── services/
│   │   └── api.ts
│   ├── types/
│   │   └── api.ts
│   ├── styles/
│   │   ├── app.css
│   │   └── index.css
│   ├── App.css
│   ├── index.css
│   ├── main.tsx
│   └── App.tsx
├── index.html
├── vite.config.ts
├── tsconfig.json
└── package.json
```

## Runtime Integration

| Item | Value |
|---|---|
| Frontend local URL | `http://localhost:5173` |
| Backend API base | `/api` (proxied in Vite) |
| Vite proxy target | `http://localhost:5000` |

Environment variable in use:

- `VITE_API_BASE_URL` (default `/api`)

## Core Components

### App.tsx

Coordinates session creation, file upload flow, and question-answer interactions.

### QuestionForm.tsx
Collects natural-language questions and controls submit availability.

**Props**:
```typescript
interface QuestionFormProps {
  sessionId: string;
  onAnswer: (answer: string) => void;
  isLoading: boolean;
}
```

Expected behavior:

- Submit disabled when no session files are available.
- Submit disabled while active question processing is in progress.
- Whitespace-only questions are rejected.

### FileUploadPanel.tsx
Handles PDF/CSV file selection and upload progress states.

Supported formats and limits:

- `.pdf`, `.csv`
- Configured max: `20 MB`

### AnswerPanel.tsx
Displays returned answers and confidence/evidence context.

## API Layer Behavior

The frontend API client in `src/services/api.ts` provides:

- Session create/get/clear methods.
- File list/upload methods.
- Question ask (standard response) and ask stream (SSE) methods.
- Shared error handling for non-2xx responses.

## UX and Messaging Guidelines

Use consistent wording in UI and docs:

- document question-answering
- session-based processing
- temporary storage
- local LLM runtime
- privacy-first architecture

Avoid implying:

- cloud upload
- persistent document history
- external analytics telemetry

## Styling and Accessibility

Baseline expectations:

- Semantic elements for forms, actions, and content regions.
- Keyboard-friendly interactions for upload and question submission.
- Readable contrast and spacing for long-form answers.

Example responsive pattern:

```css
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

## Development Commands

### Environment Variables

Use `.env` or `.env.local` in the frontend directory:

```env
VITE_API_BASE_URL=/api
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

## Customization Notes

### Adjust API base URL

Set `VITE_API_BASE_URL` to another value only when proxy behavior changes.

### Update theme variables

Use shared variables in stylesheet files:

```css
:root {
  --primary-color: #0066cc;
  --border-color: #ddd;
  --background-color: #fff;
}
```

## Browser Support

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

## Troubleshooting

Backend connection fails:

- Ensure backend is running on `http://localhost:5000`.
- Confirm Vite proxy target has not changed.

Unexpected upload validation behavior:

- Confirm file type and size align with backend rules.
- Confirm frontend validation messages match backend error responses.

Questions not streaming:

- Confirm stream endpoint `/api/sessions/{sessionId}/questions/stream` is reachable.
- Confirm Ollama is running and models are loaded.
- Review browser console and backend logs.
