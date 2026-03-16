/**
 * Shared TypeScript types that mirror the backend API contracts.
 * Keep these in sync with the C# Contracts folder in the backend.
 */

// ─── Session ────────────────────────────────────────────────────────────────

export interface CreateSessionResponse {
  sessionId: string;
  createdAt: string; // ISO 8601
}

// ─── Files ──────────────────────────────────────────────────────────────────

export interface SessionFileItem {
  fileId: string;
  fileName: string;
  fileType: string; // "pdf" | "csv"
  fileSizeBytes: number;
  uploadedAt: string; // ISO 8601
}

export interface SessionFilesResponse {
  sessionId: string;
  files: SessionFileItem[];
}

export interface FileUploadResponse {
  fileId: string;
  fileName: string;
  fileType: string;
  fileSizeBytes: number;
  uploadedAt: string;
}

// ─── Questions ──────────────────────────────────────────────────────────────

export interface AskQuestionRequest {
  question: string;
}

export interface EvidenceItem {
  fileName: string;
  snippet: string;
}

export interface AskQuestionResponse {
  question: string;
  answer: string;
  hasConfidentAnswer: boolean;
  evidence: EvidenceItem[];
}

// ─── Clear Session ───────────────────────────────────────────────────────────

export interface ClearSessionResponse {
  sessionId: string;
  cleared: boolean;
  message: string;
}

// ─── Error ───────────────────────────────────────────────────────────────────

export interface ApiError {
  error: string;
  detail?: string;
}
