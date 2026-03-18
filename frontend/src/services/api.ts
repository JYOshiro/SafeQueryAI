/**
 * API service layer — all backend communication goes through here.
 * Centralising requests makes it easy to add auth headers or change the base URL later.
 */

import type {
  CreateSessionResponse,
  SessionFilesResponse,
  FileUploadResponse,
  AskQuestionResponse,
  ClearSessionResponse,
  EvidenceItem,
} from '../types/api';

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

async function request<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: {
      Accept: 'application/json',
      ...(options?.body instanceof FormData
        ? {} // Let the browser set multipart content-type with boundary
        : { 'Content-Type': 'application/json' }),
      ...options?.headers,
    },
    ...options,
  });

  if (!response.ok) {
    let errorMessage = `Request failed: ${response.status} ${response.statusText}`;
    try {
      const body = await response.json();
      if (body?.error) errorMessage = body.error;
      if (body?.detail) errorMessage += ` — ${body.detail}`;
    } catch {
      // ignore JSON parse errors on error responses
    }
    throw new Error(errorMessage);
  }

  return response.json() as Promise<T>;
}

// ─── Session ─────────────────────────────────────────────────────────────────

export const createSession = (): Promise<CreateSessionResponse> =>
  request<CreateSessionResponse>('/sessions', { method: 'POST' });

export const getSession = (sessionId: string): Promise<CreateSessionResponse> =>
  request<CreateSessionResponse>(`/sessions/${sessionId}`);

export const clearSession = (sessionId: string): Promise<ClearSessionResponse> =>
  request<ClearSessionResponse>(`/sessions/${sessionId}`, { method: 'DELETE' });

// ─── Files ────────────────────────────────────────────────────────────────────

export const getSessionFiles = (sessionId: string): Promise<SessionFilesResponse> =>
  request<SessionFilesResponse>(`/sessions/${sessionId}/files`);

export const uploadFile = (sessionId: string, file: File): Promise<FileUploadResponse> => {
  const formData = new FormData();
  formData.append('file', file);
  return request<FileUploadResponse>(`/sessions/${sessionId}/files`, {
    method: 'POST',
    body: formData,
  });
};

/**
 * Uploads a file with progress callbacks.
 * Phase 'uploading' reports 0–100 as the bytes are transferred.
 * Phase 'analyzing' fires once bytes are sent and Ollama begins indexing.
 */
export function uploadFileWithProgress(
  sessionId: string,
  file: File,
  onProgress: (phase: 'uploading' | 'analyzing', percent: number) => void,
): Promise<FileUploadResponse> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    const formData = new FormData();
    formData.append('file', file);

    xhr.upload.addEventListener('progress', (e) => {
      if (e.lengthComputable) {
        onProgress('uploading', Math.round((e.loaded / e.total) * 100));
      }
    });

    xhr.upload.addEventListener('load', () => {
      onProgress('analyzing', 100);
    });

    xhr.addEventListener('load', () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        try {
          resolve(JSON.parse(xhr.responseText) as FileUploadResponse);
        } catch {
          reject(new Error('Invalid response from server'));
        }
      } else {
        let errorMessage = `Request failed: ${xhr.status}`;
        try {
          const body = JSON.parse(xhr.responseText);
          if (body?.error) errorMessage = body.error;
          if (body?.detail) errorMessage += ` — ${body.detail}`;
        } catch { /* ignore */ }
        reject(new Error(errorMessage));
      }
    });

    xhr.addEventListener('error', () => reject(new Error('Network error during upload')));
    xhr.addEventListener('abort', () => reject(new Error('Upload aborted')));

    xhr.open('POST', `${BASE_URL}/sessions/${sessionId}/files`);
    xhr.setRequestHeader('Accept', 'application/json');
    xhr.send(formData);
  });
}

// ─── Questions ────────────────────────────────────────────────────────────────

export const askQuestion = (
  sessionId: string,
  question: string,
): Promise<AskQuestionResponse> =>
  request<AskQuestionResponse>(`/sessions/${sessionId}/questions`, {
    method: 'POST',
    body: JSON.stringify({ question }),
  });

/** SSE event shapes from the /questions/stream endpoint */
interface TokenEvent { type: 'token'; content: string }
interface DoneEvent  { type: 'done'; question: string; hasConfidentAnswer: boolean; evidence: EvidenceItem[] }
type StreamEvent = TokenEvent | DoneEvent;

/**
 * Streams the answer token-by-token via Server-Sent Events.
 * Calls `onToken` for each partial text chunk and resolves with metadata once done.
 */
export async function askQuestionStream(
  sessionId: string,
  question: string,
  onToken: (token: string) => void,
  signal?: AbortSignal,
): Promise<{ question: string; hasConfidentAnswer: boolean; evidence: EvidenceItem[] }> {
  const response = await fetch(`${BASE_URL}/sessions/${sessionId}/questions/stream`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'text/event-stream' },
    body: JSON.stringify({ question }),
    signal,
  });

  if (!response.ok) {
    let errorMessage = `Request failed: ${response.status} ${response.statusText}`;
    try {
      const body = await response.json();
      if (body?.error) errorMessage = body.error;
    } catch { /* ignore */ }
    throw new Error(errorMessage);
  }

  const reader = response.body!.getReader();
  const decoder = new TextDecoder();
  let buffer = '';
  let meta: { question: string; hasConfidentAnswer: boolean; evidence: EvidenceItem[] } | null = null;

  while (true) {
    const { done, value } = await reader.read();
    if (done) break;

    buffer += decoder.decode(value, { stream: true });
    const lines = buffer.split('\n');
    buffer = lines.pop() ?? '';

    for (const line of lines) {
      if (!line.startsWith('data: ')) continue;
      const dataStr = line.slice(6).trim();
      if (!dataStr) continue;

      try {
        const event = JSON.parse(dataStr) as StreamEvent;
        if (event.type === 'token') {
          onToken(event.content);
        } else if (event.type === 'done') {
          meta = { question: event.question, hasConfidentAnswer: event.hasConfidentAnswer, evidence: event.evidence };
        }
      } catch { /* ignore malformed events */ }
    }
  }

  if (!meta) throw new Error('Stream ended without a final result');
  return meta;
}
