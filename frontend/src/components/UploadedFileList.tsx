import type { SessionFileItem } from '../types/api';

interface UploadedFileListProps {
  files: SessionFileItem[];
  isLoading: boolean;
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export default function UploadedFileList({ files, isLoading }: UploadedFileListProps) {
  if (isLoading) {
    return <p className="loading-text">Loading files…</p>;
  }

  if (files.length === 0) {
    return (
      <div className="file-list-empty">
        <p>No files uploaded yet. Upload a PDF or CSV to get started.</p>
      </div>
    );
  }

  return (
    <div className="file-list">
      <h2 className="panel-title">Uploaded Files ({files.length})</h2>
      <ul className="file-list-items" aria-label="Uploaded files">
        {files.map((file) => (
          <li key={file.fileId} className="file-list-item">
            <span className="file-icon" aria-hidden="true">
              {file.fileType === 'pdf' ? '📕' : '📊'}
            </span>
            <div className="file-meta">
              <span className="file-name">{file.fileName}</span>
              <span className="file-details">
                {file.fileType.toUpperCase()} · {formatBytes(file.fileSizeBytes)}
              </span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
