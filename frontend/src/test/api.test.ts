import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { createSession, clearSession, askQuestionStream } from '../services/api';

// ─── Helpers ──────────────────────────────────────────────────────────────────

function buildSseStream(events: string[]): ReadableStream<Uint8Array> {
  const encoder = new TextEncoder();
  return new ReadableStream<Uint8Array>({
    start(controller) {
      for (const event of events) {
        controller.enqueue(encoder.encode(event));
      }
      controller.close();
    },
  });
}

// ─── request() wrapper (tested via createSession / clearSession) ─────────────

describe('request helper', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('returns parsed JSON on a successful response', async () => {
    const payload = { sessionId: 'abc-123', createdAt: 'now', expiresAt: 'later' };
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(payload), { status: 200 }),
    );

    const result = await createSession();

    expect(result.sessionId).toBe('abc-123');
  });

  it('throws with the error field from the response body on non-2xx status', async () => {
    const errorBody = { error: 'Session not found' };
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(errorBody), { status: 404 }),
    );

    await expect(clearSession('bad-id')).rejects.toThrow('Session not found');
  });

  it('throws with status text when response body has no error field', async () => {
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response('Not Found', { status: 404, statusText: 'Not Found' }),
    );

    await expect(clearSession('bad-id')).rejects.toThrow('404');
  });

  it('includes detail in the error message when provided', async () => {
    const errorBody = { error: 'Validation failed', detail: 'question is required' };
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(errorBody), { status: 400 }),
    );

    await expect(clearSession('any')).rejects.toThrow('Validation failed — question is required');
  });
});

// ─── askQuestionStream ────────────────────────────────────────────────────────

describe('askQuestionStream', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('yields each token to the onToken callback', async () => {
    const sse = [
      'data: {"type":"token","content":"Hello"}\n\n',
      'data: {"type":"token","content":" world"}\n\n',
      'data: {"type":"done","question":"Hi?","hasConfidentAnswer":true,"evidence":[]}\n\n',
    ];

    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(buildSseStream(sse), { status: 200 }),
    );

    const tokens: string[] = [];
    await askQuestionStream('s1', 'Hi?', (t) => tokens.push(t));

    expect(tokens).toEqual(['Hello', ' world']);
  });

  it('resolves with metadata from the done event', async () => {
    const evidence = [{ fileId: 'f1', fileName: 'doc.pdf', excerpt: 'text', score: 0.9 }];
    const sse = [
      `data: {"type":"done","question":"What?","hasConfidentAnswer":false,"evidence":${JSON.stringify(evidence)}}\n\n`,
    ];

    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(buildSseStream(sse), { status: 200 }),
    );

    const result = await askQuestionStream('s1', 'What?', () => {});

    expect(result.question).toBe('What?');
    expect(result.hasConfidentAnswer).toBe(false);
    expect(result.evidence).toEqual(evidence);
  });

  it('throws when the stream ends without a done event', async () => {
    const sse = ['data: {"type":"token","content":"partial"}\n\n'];

    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(buildSseStream(sse), { status: 200 }),
    );

    await expect(askQuestionStream('s1', 'q', () => {})).rejects.toThrow(
      'Stream ended without a final result',
    );
  });

  it('silently ignores malformed SSE data lines', async () => {
    const sse = [
      'data: not-valid-json\n\n',
      'data: {"type":"done","question":"q","hasConfidentAnswer":true,"evidence":[]}\n\n',
    ];

    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(buildSseStream(sse), { status: 200 }),
    );

    const result = await askQuestionStream('s1', 'q', () => {});
    expect(result.hasConfidentAnswer).toBe(true);
  });

  it('throws on a non-2xx response before reading the stream', async () => {
    const errorBody = { error: 'Unauthorized' };
    vi.mocked(fetch).mockResolvedValueOnce(
      new Response(JSON.stringify(errorBody), { status: 401 }),
    );

    await expect(askQuestionStream('s1', 'q', () => {})).rejects.toThrow('Unauthorized');
  });
});
