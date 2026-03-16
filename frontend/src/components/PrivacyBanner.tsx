import { useState } from 'react';

export default function PrivacyBanner() {
  const [dismissed, setDismissed] = useState(false);

  if (dismissed) return null;

  return (
    <div className="privacy-banner" role="banner" aria-label="Privacy notice">
      <div className="privacy-banner-content">
        <span className="privacy-icon" aria-hidden="true">🔒</span>
        <p>
          <strong>Privacy-first design:</strong> Your files are processed only during this
          browser session and stored temporarily in memory. Nothing is saved to a database
          or shared with third parties. Use <em>Clear Session</em> at any time to remove all
          uploaded files immediately.
        </p>
      </div>
      <button
        className="privacy-banner-close"
        onClick={() => setDismissed(true)}
        aria-label="Dismiss privacy notice"
      >
        ✕
      </button>
    </div>
  );
}
