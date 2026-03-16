interface SessionInfoProps {
  sessionId: string;
  fileCount: number;
  onClearSession: () => void;
  isClearing: boolean;
}

export default function SessionInfo({
  sessionId,
  fileCount,
  onClearSession,
  isClearing,
}: SessionInfoProps) {
  return (
    <div className="session-info">
      <div className="session-meta">
        <span className="session-label">Session</span>
        <code className="session-id" title={sessionId}>
          {sessionId.slice(0, 8)}…
        </code>
        <span className="session-files">
          {fileCount} file{fileCount !== 1 ? 's' : ''} uploaded
        </span>
      </div>
      <button
        className="btn btn-danger"
        onClick={onClearSession}
        disabled={isClearing}
        aria-label="Clear session and remove all uploaded files"
      >
        {isClearing ? 'Clearing…' : 'Clear Session'}
      </button>
    </div>
  );
}
