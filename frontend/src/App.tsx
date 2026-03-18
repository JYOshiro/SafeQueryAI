import { useEffect, useState, useCallback } from 'react';
import type { AskQuestionResponse, SessionFileItem } from './types/api';
import * as api from './services/api';

import PrivacyBanner from './components/PrivacyBanner';
import SessionInfo from './components/SessionInfo';
import FileUploadPanel from './components/FileUploadPanel';
import UploadedFileList from './components/UploadedFileList';
import QuestionForm from './components/QuestionForm';
import AnswerPanel from './components/AnswerPanel';

import './styles/app.css';

export default function App() {
  // ─── Session state ────────────────────────────────────────────────────────
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [sessionError, setSessionError] = useState<string | null>(null);

  // ─── Files state ──────────────────────────────────────────────────────────
  const [files, setFiles] = useState<SessionFileItem[]>([]);
  const [filesLoading, setFilesLoading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<{ phase: 'uploading' | 'analyzing'; percent: number } | null>(null);

  // ─── Question/answer state ─────────────────────────────────────────────────
  const [answerResult, setAnswerResult] = useState<AskQuestionResponse | null>(null);
  const [isAsking, setIsAsking] = useState(false);
  const [answerError, setAnswerError] = useState<string | null>(null);
  const [streamingAnswer, setStreamingAnswer] = useState('');

  // ─── Clear session state ──────────────────────────────────────────────────
  const [isClearing, setIsClearing] = useState(false);

  // ─── Initialise session on mount ──────────────────────────────────────────
  useEffect(() => {
    let cancelled = false;

    async function initSession() {
      try {
        const session = await api.createSession();
        if (!cancelled) {
          setSessionId(session.sessionId);
        }
      } catch (err) {
        if (!cancelled) {
          setSessionError('Could not start session. Is the backend running?');
          console.error('Session init error:', err);
        }
      }
    }

    initSession();
    return () => { cancelled = true; };
  }, []);

  // ─── Refresh file list ────────────────────────────────────────────────────
  const refreshFiles = useCallback(async (sid: string) => {
    setFilesLoading(true);
    try {
      const response = await api.getSessionFiles(sid);
      setFiles(response.files);
    } catch (err) {
      console.error('Failed to load files:', err);
    } finally {
      setFilesLoading(false);
    }
  }, []);

  useEffect(() => {
    if (sessionId) refreshFiles(sessionId);
  }, [sessionId, refreshFiles]);

  // ─── Upload handler ───────────────────────────────────────────────────────
  async function handleFilesUploaded(selectedFiles: File[]) {
    if (!sessionId) return;
    setIsUploading(true);
    setUploadError(null);
    setUploadProgress(null);

    try {
      for (const file of selectedFiles) {
        await api.uploadFileWithProgress(sessionId, file, (phase, percent) => {
          setUploadProgress({ phase, percent });
        });
      }
      await refreshFiles(sessionId);
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Upload failed.';
      setUploadError(message);
    } finally {
      setIsUploading(false);
      setUploadProgress(null);
    }
  }

  // ─── Ask question handler ─────────────────────────────────────────────────
  async function handleAskQuestion(question: string) {
    if (!sessionId) return;
    setIsAsking(true);
    setAnswerError(null);
    setAnswerResult(null);
    setStreamingAnswer('');

    let accumulated = '';
    try {
      const meta = await api.askQuestionStream(sessionId, question, (token) => {
        accumulated += token;
        setStreamingAnswer(accumulated);
      });
      setAnswerResult({
        question: meta.question,
        answer: accumulated,
        hasConfidentAnswer: meta.hasConfidentAnswer,
        evidence: meta.evidence,
      });
      setStreamingAnswer('');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Failed to get an answer.';
      setAnswerError(message);
    } finally {
      setIsAsking(false);
    }
  }

  // ─── Clear session handler ────────────────────────────────────────────────
  async function handleClearSession() {
    if (!sessionId) return;
    if (!window.confirm('Clear session? All uploaded files will be permanently removed.')) return;

    setIsClearing(true);
    try {
      await api.clearSession(sessionId);
      // Create a fresh session after clearing
      const newSession = await api.createSession();
      setSessionId(newSession.sessionId);
      setFiles([]);
      setAnswerResult(null);
      setAnswerError(null);
      setUploadError(null);
    } catch (err) {
      console.error('Clear session error:', err);
    } finally {
      setIsClearing(false);
    }
  }

  // ─── Render ───────────────────────────────────────────────────────────────

  if (sessionError) {
    return (
      <div className="app-wrapper">
        <header className="app-header">
          <h1>🔒 SafeQueryAI</h1>
        </header>
        <div className="app-init-error">
          <p>{sessionError}</p>
          <p>Make sure the backend API is running on <code>http://localhost:5000</code>.</p>
        </div>
      </div>
    );
  }

  if (!sessionId) {
    return (
      <div className="app-wrapper">
        <header className="app-header">
          <h1>🔒 SafeQueryAI</h1>
        </header>
        <div style={{ textAlign: 'center', padding: '3rem 1rem' }}>
          <p className="loading-text">Starting session…</p>
        </div>
      </div>
    );
  }

  return (
    <div className="app-wrapper">
      <header className="app-header">
        <h1>🔒 SafeQueryAI</h1>
        <span className="header-tagline">Privacy-first document Q&amp;A</span>
      </header>

      <PrivacyBanner />

      <SessionInfo
        sessionId={sessionId}
        fileCount={files.length}
        onClearSession={handleClearSession}
        isClearing={isClearing}
      />

      <main className="app-main">
        <div className="col-left">
          <FileUploadPanel
            onFilesUploaded={handleFilesUploaded}
            isUploading={isUploading}
            uploadError={uploadError}
            uploadProgress={uploadProgress}
          />
          <UploadedFileList files={files} isLoading={filesLoading} />
        </div>

        <div className="col-right">
          <QuestionForm
            onAsk={handleAskQuestion}
            isAsking={isAsking}
            hasFiles={files.length > 0}
          />
          <AnswerPanel
            result={answerResult}
            isLoading={isAsking}
            error={answerError}
            streamingText={streamingAnswer}
          />
        </div>
      </main>
    </div>
  );
}
