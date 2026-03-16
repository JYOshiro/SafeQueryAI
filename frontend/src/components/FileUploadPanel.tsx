import { useRef, useState } from 'react';

interface FileUploadPanelProps {
  onFilesUploaded: (files: File[]) => Promise<void>;
  isUploading: boolean;
  uploadError: string | null;
}

const ACCEPTED_TYPES = '.pdf,.csv';
const MAX_FILE_SIZE_MB = 20;

export default function FileUploadPanel({
  onFilesUploaded,
  isUploading,
  uploadError,
}: FileUploadPanelProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragOver, setDragOver] = useState(false);

  function validateFiles(files: FileList | null): File[] | string {
    if (!files || files.length === 0) return 'No files selected.';
    const validExtensions = ['.pdf', '.csv'];
    const maxBytes = MAX_FILE_SIZE_MB * 1024 * 1024;
    const valid: File[] = [];

    for (const file of Array.from(files)) {
      const ext = file.name.toLowerCase().slice(file.name.lastIndexOf('.'));
      if (!validExtensions.includes(ext))
        return `"${file.name}" is not a supported type. Only PDF and CSV files are accepted.`;
      if (file.size > maxBytes)
        return `"${file.name}" exceeds the ${MAX_FILE_SIZE_MB} MB size limit.`;
      valid.push(file);
    }
    return valid;
  }

  async function handleFiles(files: FileList | null) {
    const result = validateFiles(files);
    if (typeof result === 'string') {
      // Let parent display via uploadError — trigger by passing empty array and the error
      // We surface it by calling onFilesUploaded with an empty array and catching the error
      // For simplicity, use a validation-only approach:
      alert(result);
      return;
    }
    await onFilesUploaded(result);
  }

  return (
    <div className="upload-panel">
      <h2 className="panel-title">Upload Files</h2>
      <p className="panel-subtitle">Supported formats: PDF (text-based), CSV</p>

      <div
        className={`drop-zone ${dragOver ? 'drop-zone--active' : ''}`}
        onDragOver={(e) => { e.preventDefault(); setDragOver(true); }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragOver(false);
          handleFiles(e.dataTransfer.files);
        }}
        onClick={() => inputRef.current?.click()}
        role="button"
        tabIndex={0}
        aria-label="Upload files — click or drag and drop"
        onKeyDown={(e) => e.key === 'Enter' && inputRef.current?.click()}
      >
        <span className="drop-zone-icon" aria-hidden="true">📄</span>
        <p className="drop-zone-text">
          {isUploading ? 'Uploading…' : 'Click or drag & drop files here'}
        </p>
        <p className="drop-zone-hint">Max {MAX_FILE_SIZE_MB} MB per file</p>
      </div>

      <input
        ref={inputRef}
        type="file"
        accept={ACCEPTED_TYPES}
        multiple
        className="visually-hidden"
        onChange={(e) => handleFiles(e.target.files)}
        aria-hidden="true"
      />

      {uploadError && (
        <p className="error-message" role="alert">{uploadError}</p>
      )}
    </div>
  );
}
