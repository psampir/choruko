import React from "react";
import "./Popup.scss";
import { useState } from "react";

interface PopupAction {
  label: string;
  onClick: () => void;
  variant?: "primary" | "secondary" | "danger";
  disabled?: boolean;
}

interface PopupProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, "title"> {
  variant?: "primary" | "red";
  title?: string | React.ReactNode;
  className?: string;
  open?: boolean;
  onClose?: () => void;
  actions?: PopupAction[];
  customActions?: React.ReactNode;
  showCloseButton?: boolean;
}

const Popup: React.FC<PopupProps> = ({
  variant = "primary",
  title = "Popup Title",
  className = "",
  children,
  open,
  onClose,
  actions,
  customActions,
  showCloseButton = true,
  ...props
}) => {
  const variantClass = variant ? `popup--${variant}` : "";
  const [internalOpen, setInternalOpen] = useState(open ?? true);

  // keep internal state in sync when controlled
  React.useEffect(() => {
    if (open !== undefined) setInternalOpen(open);
  }, [open]);

  const isOpen = open !== undefined ? open : internalOpen;

  const closePopup = () => {
    if (onClose) onClose();
    else setInternalOpen(false);
  };

  const handleOutsideClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      closePopup();
    }
  };

  return (
    <div
      className={`popup-container ${isOpen ? "active" : ""}`}
      onClick={handleOutsideClick}
    >
      <div className={`popup ${variantClass} ${className}`} {...props}>
        {showCloseButton && <i className="close-icon" onClick={closePopup}></i>}
        <h2 className="popup-title">{title}</h2>

        {children && <div className="popup-content">{children}</div>}

        {((actions && actions.length > 0) || customActions) && (
          <div className="popup-actions">
            {actions &&
              actions.map((action, index) => (
                <button
                  key={index}
                  className={`popup-button popup-button--${
                    action.variant || "primary"
                  }`}
                  onClick={action.onClick}
                  disabled={action.disabled}
                >
                  {action.label}
                </button>
              ))}
            {customActions}
          </div>
        )}
      </div>
    </div>
  );
};

export default Popup;
