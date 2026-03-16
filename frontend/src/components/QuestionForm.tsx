import { useState } from 'react';

interface QuestionFormProps {
  onAsk: (question: string) => Promise<void>;
  isAsking: boolean;
  hasFiles: boolean;
}

export default function QuestionForm({ onAsk, isAsking, hasFiles }: QuestionFormProps) {
  const [question, setQuestion] = useState('');

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const trimmed = question.trim();
    if (!trimmed) return;
    await onAsk(trimmed);
  }

  return (
    <div className="question-panel">
      <h2 className="panel-title">Ask a Question</h2>
      <p className="panel-subtitle">
        Questions are answered using only the content of your uploaded files.
      </p>

      <form onSubmit={handleSubmit} className="question-form">
        <textarea
          className="question-input"
          placeholder={
            hasFiles
              ? 'Type your question here…'
              : 'Upload a file first, then ask a question.'
          }
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          disabled={isAsking || !hasFiles}
          rows={3}
          maxLength={1000}
          aria-label="Question input"
        />
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isAsking || !hasFiles || !question.trim()}
          aria-label="Submit question"
        >
          {isAsking ? 'Searching…' : 'Ask'}
        </button>
      </form>
    </div>
  );
}
