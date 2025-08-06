import React, { useState, type InputHTMLAttributes } from "react";
import "./Input.scss";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  variant?: "primary" | "password" | "email" | "edit";
  className?: string;
  children?: React.ReactNode;
  onEditClick?: () => void; // Add callback for edit functionality
}

const Input: React.FC<InputProps> = ({
  variant = "primary",
  className = "",
  children,
  onEditClick,
  ...props
}) => {
  const variantClass = variant ? `input--${variant}` : "";
  const [showPassword, setShowPassword] = useState(false);

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };

  const handleEditClick = () => {
    if (onEditClick) {
      onEditClick();
    }
  };

  return (
    <>
      {variant === "password" ? (
        <div className="input__wrapper">
          <input
            type={showPassword ? "text" : "password"}
            className={`input ${variantClass} ${className} ${
              showPassword ? " active" : ""
            }`}
            {...props}
          />
          <span
            className="input__eye"
            onClick={toggleShowPassword}
            aria-label="Show/Hide password"
            tabIndex={0}
            role="button"
          />
          {children}
        </div>
      ) : variant === "edit" ? (
        <div className="input__wrapper">
          <input className={`input ${variantClass} ${className}`} {...props} />
          <span
            className="input__pencil"
            onClick={handleEditClick}
            aria-label="Edit field"
            tabIndex={0}
            role="button"
          />
          {children}
        </div>
      ) : (
        <input className={`input ${variantClass} ${className}`} {...props}>
          {children}
        </input>
      )}
    </>
  );
};

export default Input;
