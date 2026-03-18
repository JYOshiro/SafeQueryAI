import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import QuestionForm from '../components/QuestionForm';

describe('QuestionForm', () => {
  const onAsk = vi.fn(() => Promise.resolve());

  beforeEach(() => {
    onAsk.mockClear();
  });

  // ── Rendering ──────────────────────────────────────────────────────────────

  it('renders the textarea and submit button', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={true} />);

    expect(screen.getByRole('textbox')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /submit question/i })).toBeInTheDocument();
  });

  it('shows placeholder hinting to upload a file when hasFiles is false', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={false} />);

    const textarea = screen.getByRole('textbox');
    expect(textarea).toHaveAttribute('placeholder', expect.stringContaining('Upload a file'));
  });

  // ── Input state ────────────────────────────────────────────────────────────

  it('disables textarea and button when hasFiles is false', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={false} />);

    expect(screen.getByRole('textbox')).toBeDisabled();
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('disables controls while isAsking is true', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={true} hasFiles={true} />);

    expect(screen.getByRole('textbox')).toBeDisabled();
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('disables submit button when the question is empty', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={true} />);

    expect(screen.getByRole('button')).toBeDisabled();
  });

  // ── Submission ─────────────────────────────────────────────────────────────

  it('calls onAsk with trimmed question when form is submitted', async () => {
    const user = userEvent.setup();
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={true} />);

    await user.type(screen.getByRole('textbox'), '  What is AI?  ');
    await user.click(screen.getByRole('button', { name: /submit question/i }));

    expect(onAsk).toHaveBeenCalledWith('What is AI?');
    expect(onAsk).toHaveBeenCalledTimes(1);
  });

  it('does not call onAsk when question is only whitespace', async () => {
    const user = userEvent.setup();
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={true} />);

    await user.type(screen.getByRole('textbox'), '   ');
    // Button should still be disabled for whitespace-only input
    expect(screen.getByRole('button')).toBeDisabled();
    expect(onAsk).not.toHaveBeenCalled();
  });

  // ── Button label ───────────────────────────────────────────────────────────

  it('shows "Searching…" label while isAsking', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={true} hasFiles={true} />);

    expect(screen.getByRole('button')).toHaveTextContent('Searching…');
  });

  it('shows "Ask" label when not asking', () => {
    render(<QuestionForm onAsk={onAsk} isAsking={false} hasFiles={true} />);

    expect(screen.getByRole('button')).toHaveTextContent('Ask');
  });
});
