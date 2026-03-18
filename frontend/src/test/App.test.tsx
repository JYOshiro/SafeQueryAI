import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from '../App';
import * as api from '../services/api';

// ─── Shared mocks ─────────────────────────────────────────────────────────────

vi.mock('../services/api', () => ({
  createSession: vi.fn(),
  getSession: vi.fn(),
  getSessionFiles: vi.fn(),
  clearSession: vi.fn(),
  uploadFile: vi.fn(),
  uploadFileWithProgress: vi.fn(),
  askQuestion: vi.fn(),
  askQuestionStream: vi.fn(),
}));

const defaultSession = { sessionId: 'test-session-id', createdAt: 'now', expiresAt: 'later' };

beforeEach(() => {
  vi.mocked(api.createSession).mockResolvedValue(defaultSession);
  vi.mocked(api.getSessionFiles).mockResolvedValue({ files: [] });
});

afterEach(() => {
  vi.clearAllMocks();
});

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('App', () => {
  it('shows loading text before session is initialised', async () => {
    // createSession never resolves during this test
    vi.mocked(api.createSession).mockReturnValue(new Promise(() => {}));

    render(<App />);

    expect(screen.getByText(/starting session/i)).toBeInTheDocument();
  });

  it('renders the main UI once the session is ready', async () => {
    render(<App />);

    await waitFor(() =>
      expect(screen.getByText(/privacy-first document q&a/i, { exact: false })).toBeInTheDocument(),
    );
  });

  it('shows an error banner when the backend is unreachable', async () => {
    vi.mocked(api.createSession).mockRejectedValue(new Error('Network error'));

    render(<App />);

    await waitFor(() =>
      expect(screen.getByText(/could not start session/i)).toBeInTheDocument(),
    );
  });

  it('displays the session ID in the session info section', async () => {
    render(<App />);

    await waitFor(() => {
      // SessionInfo renders the id truncated but with full id in a title attribute
      const sessionEl = document.querySelector('[title="test-session-id"]');
      expect(sessionEl).toBeInTheDocument();
    });
  });

  it('creates a fresh session after clearing', async () => {
    const freshSession = { sessionId: 'fresh-session-id', createdAt: 'later', expiresAt: 'even-later' };
    vi.mocked(api.createSession)
      .mockResolvedValueOnce(defaultSession)
      .mockResolvedValueOnce(freshSession);
    vi.mocked(api.clearSession).mockResolvedValue({ cleared: true });
    vi.mocked(api.getSessionFiles).mockResolvedValue({ files: [] });

    vi.spyOn(window, 'confirm').mockReturnValue(true);

    render(<App />);

    await waitFor(() => expect(document.querySelector('[title="test-session-id"]')).toBeInTheDocument());

    const clearButton = screen.getByRole('button', { name: /clear session and remove all uploaded files/i });
    await userEvent.click(clearButton);

    await waitFor(() => {
      expect(api.clearSession).toHaveBeenCalledWith('test-session-id');
      expect(api.createSession).toHaveBeenCalledTimes(2);
    });
  });
});
