import React from "react";
import "./Popup.scss";
import { useState } from "react";
import Button from "../Button";

interface PopupProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: "primary" | "red";
  title?: string;
  className?: string;
}

const Popup: React.FC<PopupProps> = ({
  variant = "primary",
  title = "Popup Title",
  className = "",
  children,
  ...props
}) => {
  const variantClass = variant ? `popup--${variant}` : "";
  const [isOpen, setIsOpen] = useState(true);

  const closePopup = () => {
    setIsOpen(false);
  };

  const handleOutsideClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      closePopup();
    }
  };

  const buttonVariant = variant === "red" ? "red-popup" : "primary-popup";

  return (
    <div className={`popup-container ${isOpen ? "active" : ""}`} onClick={handleOutsideClick}>
      <div className={`popup ${variantClass} ${className}`} {...props}>
        <i className="close-icon" onClick={closePopup}></i>
        <h2 className="popup-title">{title}</h2>
        {children}
        <Button variant={buttonVariant} onClick={closePopup}>Close</Button>
      </div>
    </div>
  );
};

export default Popup;
