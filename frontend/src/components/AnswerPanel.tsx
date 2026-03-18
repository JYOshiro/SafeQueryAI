import type { AskQuestionResponse } from '../types/api';

interface AnswerPanelProps {
  result: AskQuestionResponse | null;
  isLoading: boolean;
  error: string | null;
  streamingText?: string;
}

export default function AnswerPanel({ result, isLoading, error, streamingText }: AnswerPanelProps) {
  // Streaming: we have tokens arriving but no final result yet
  if (isLoading && streamingText) {
    return (
      <div className="answer-panel answer-panel--streaming" aria-live="polite">
        <h2 className="panel-title">Answer</h2>
        <div className="answer-body">
          <p className="answer-streaming">
            {streamingText}<span className="answer-cursor" aria-hidden="true" />
          </p>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="answer-panel answer-panel--loading" aria-live="polite">
        <div className="thinking-indicator">
          <span>🤖 Thinking</span>
          <span className="thinking-dots" aria-hidden="true">
            <span>.</span><span>.</span><span>.</span>
          </span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="answer-panel answer-panel--error" role="alert">
        <p className="error-message">{error}</p>
      </div>
    );
  }

  if (!result) return null;

  return (
    <div
      className={`answer-panel ${result.hasConfidentAnswer ? 'answer-panel--confident' : 'answer-panel--uncertain'}`}
      aria-live="polite"
    >
      <h2 className="panel-title">Answer</h2>

      <div className="answer-question">
        <span className="label">Question:</span>
        <p>{result.question}</p>
      </div>

      <div className="answer-body">
        <p className="answer-text">{result.answer}</p>
      </div>

      {result.hasConfidentAnswer && result.evidence.length > 0 && (
        <div className="evidence-section">
          <h3 className="evidence-title">Evidence from uploaded files</h3>
          <ul className="evidence-list" aria-label="Evidence snippets">
            {result.evidence.map((item, idx) => (
              <li key={idx} className="evidence-item">
                <span className="evidence-filename">📄 {item.fileName}</span>
                <blockquote className="evidence-snippet">{item.snippet}</blockquote>
              </li>
            ))}
          </ul>
        </div>
      )}

      {!result.hasConfidentAnswer && (
        <p className="answer-insufficient">
          ℹ️ The system could not find sufficient evidence in the uploaded files to answer
          this question confidently.
        </p>
      )}
    </div>
  );
}
