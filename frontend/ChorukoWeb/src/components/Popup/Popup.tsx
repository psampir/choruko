import React from "react";
import "./Popup.scss";
import { useState } from "react";

interface PopupProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: "primary" | "red";
  title?: string;
  className?: string;
  open?: boolean;
  onClose?: () => void; // notify parent to close
}

const Popup: React.FC<PopupProps> = ({
  variant = "primary",
  title = "Popup Title",
  className = "",
  children,
  open,
  onClose,
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
    <div className={`popup-container ${isOpen ? "active" : ""}`} onClick={handleOutsideClick}>
      <div className={`popup ${variantClass} ${className}`} {...props}>
        <i className="close-icon" onClick={closePopup}></i>
        <h2 className="popup-title">{title}</h2>
        {children}
      </div>
    </div>
  );
};

export default Popup;
