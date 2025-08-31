import React, { useEffect } from "react";
import "./ErrorMessage.scss";

interface ErrorMessageProps {
  message: string;
  onClose?: () => void;
  variant?: "overlay" | "inline";
  className?: string;
  autoDismiss?: boolean;
  dismissTime?: number; // in milliseconds
}

const ErrorMessage: React.FC<ErrorMessageProps> = ({
  message,
  onClose,
  variant = "inline",
  className = "",
  autoDismiss = true,
  dismissTime = 5000, // 5 seconds default
}) => {
  useEffect(() => {
    if (message && autoDismiss && onClose) {
      const timer = setTimeout(() => {
        onClose();
      }, dismissTime);

      // Cleanup timer if component unmounts or message changes
      return () => clearTimeout(timer);
    }
  }, [message, autoDismiss, onClose, dismissTime]);

  if (!message) return null;

  const baseClass =
    variant === "overlay" ? "error-message--overlay" : "error-message--inline";

  return (
    <div className={`error-message ${baseClass} ${className}`}>
      <span className="error-message__text">{message}</span>
      {onClose && (
        <button
          className="error-message__close"
          onClick={onClose}
          aria-label="Close error message"
        >
          ×
        </button>
      )}
    </div>
  );
};

export default ErrorMessage;
