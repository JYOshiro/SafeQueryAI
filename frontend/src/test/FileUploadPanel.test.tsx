import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import FileUploadPanel from '../components/FileUploadPanel';

const makeOnUpload = () => vi.fn(() => Promise.resolve());

describe('FileUploadPanel', () => {
  let onFilesUploaded: ReturnType<typeof makeOnUpload>;

  beforeEach(() => {
    onFilesUploaded = makeOnUpload();
  });

  // ── Rendering ──────────────────────────────────────────────────────────────

  it('renders the drop zone with upload hint', () => {
    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    expect(screen.getByRole('button', { name: /upload files.*click or drag/i })).toBeInTheDocument();
  });

  it('shows uploading text when isUploading and phase is uploading', () => {
    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={true}
        uploadError={null}
        uploadProgress={{ phase: 'uploading', percent: 50 }}
      />,
    );

    expect(screen.getByText(/uploading file/i)).toBeInTheDocument();
  });

  it('shows analysing text when phase is analyzing', () => {
    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={true}
        uploadError={null}
        uploadProgress={{ phase: 'analyzing', percent: 100 }}
      />,
    );

    expect(screen.getByText(/indexing the document/i)).toBeInTheDocument();
  });

  it('renders upload error when provided', () => {
    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError="File too large"
        uploadProgress={null}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('File too large');
  });

  it('does not render error element when uploadError is null', () => {
    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  // ── File input validation (via the hidden file input) ─────────────────────

  it('does not call onFilesUploaded when an unsupported file type is selected', async () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});
    const user = userEvent.setup();

    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const file = new File(['content'], 'document.docx', { type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' });

    await user.upload(input, file);

    expect(onFilesUploaded).not.toHaveBeenCalled();
  });

  it('does not call onFilesUploaded when file exceeds size limit', async () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});
    const user = userEvent.setup();

    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const bigContent = 'x'.repeat(21 * 1024 * 1024); // 21 MB
    const oversizedFile = new File([bigContent], 'big.pdf', { type: 'application/pdf' });

    Object.defineProperty(oversizedFile, 'size', { value: 21 * 1024 * 1024 });

    await user.upload(input, oversizedFile);

    expect(onFilesUploaded).not.toHaveBeenCalled();
  });

  it('calls onFilesUploaded with valid PDF file', async () => {
    const user = userEvent.setup();

    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const validFile = new File(['%PDF-1.4'], 'doc.pdf', { type: 'application/pdf' });

    await user.upload(input, validFile);

    expect(onFilesUploaded).toHaveBeenCalledWith([validFile]);
  });

  it('calls onFilesUploaded with valid CSV file', async () => {
    const user = userEvent.setup();

    render(
      <FileUploadPanel
        onFilesUploaded={onFilesUploaded}
        isUploading={false}
        uploadError={null}
        uploadProgress={null}
      />,
    );

    const input = document.querySelector('input[type="file"]') as HTMLInputElement;
    const csvFile = new File(['a,b,c\n1,2,3'], 'data.csv', { type: 'text/csv' });

    await user.upload(input, csvFile);

    expect(onFilesUploaded).toHaveBeenCalledWith([csvFile]);
  });
});
