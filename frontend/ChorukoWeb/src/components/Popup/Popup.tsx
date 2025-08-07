import React from "react";
import "./Popup.scss";
import { useState } from "react";
import Button from "../Button";

interface PopupProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: "primary" | "red";
  className?: string;
}

const Popup: React.FC<PopupProps> = ({
  variant = "primary",
  className = "",
  children,
  ...props
}) => {
  const variantClass = variant ? `popup--${variant}` : "";

  const [isOpen, setIsOpen] = useState(true);
  const closePopup = () => {
    setIsOpen(false);
  };

  // Handler to prevent popup content clicks from bubbling up
  const handlePopupClick = (e: React.MouseEvent) => {
    e.stopPropagation();
  };

  return (
    <div
      className={`popup-container ${isOpen ? "active" : ""}`}
      onClick={closePopup}
    >
      <div
        className={`popup ${variantClass} ${className}`}
        onClick={handlePopupClick}
        {...props}
      >
        <i className="close-icon" onClick={closePopup}></i>
        <h2 className="popup-title">Popup Title</h2>
        <Button variant="primary" onClick={closePopup}>
          Close
        </Button>
      </div>
    </div>
  );
};

export default Popup;
