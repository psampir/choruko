import React, { useEffect } from "react";
import "./NotificationBar.scss";

interface NotificationBarProps {
  message: string;
  isVisible: boolean;
  type: "success" | "error";
  onClose?: () => void;
  autoDismiss?: boolean;
  dismissTime?: number;
}

const NotificationBar: React.FC<NotificationBarProps> = ({
  message,
  isVisible,
  type,
  onClose,
  autoDismiss = false,
  dismissTime = 5000,
}) => {
  useEffect(() => {
    if (isVisible && autoDismiss && onClose) {
      const timer = setTimeout(() => {
        onClose();
      }, dismissTime);

      return () => clearTimeout(timer);
    }
  }, [isVisible, autoDismiss, onClose, dismissTime]);

  useEffect(() => {
    if (isVisible) {
      document.body.classList.add('notification-bar-visible');
    } else {
      document.body.classList.remove('notification-bar-visible');
    }

    return () => {
      document.body.classList.remove('notification-bar-visible');
    };
  }, [isVisible]);

  if (!message) return null;

  return (
    <div
      className={`notification-bar notification-bar--${type} ${
        isVisible ? "notification-bar--visible" : ""
      }`}
    >
      <span>{message}</span>
    </div>
  );
};

export default NotificationBar;
