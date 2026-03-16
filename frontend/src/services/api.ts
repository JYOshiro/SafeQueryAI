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

// ─── Questions ────────────────────────────────────────────────────────────────

export const askQuestion = (
  sessionId: string,
  question: string,
): Promise<AskQuestionResponse> =>
  request<AskQuestionResponse>(`/sessions/${sessionId}/questions`, {
    method: 'POST',
    body: JSON.stringify({ question }),
  });
