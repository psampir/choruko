import React, { useState, useRef, type InputHTMLAttributes } from "react";
import "./Input.scss";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  variant?: "primary" | "password" | "email" | "edit" | "datetime";
  className?: string;
  children?: React.ReactNode;
  onEditClick?: () => void;
}

const Input: React.FC<InputProps> = ({
  variant = "primary",
  className = "",
  children,
  onEditClick,
  ...props
}) => {
  // Detect password either by variant or incoming type for convenience
  const isPassword = variant === "password" || props.type === "password";

  // Prevent incoming type from overriding our controlled type
  const { type: _ignoredType, ...restProps } = props;

  const variantClass = isPassword
    ? "input--password"
    : variant
    ? `input--${variant}`
    : "";

  const [showPassword, setShowPassword] = useState(false);
  const datetimeInputRef = useRef<HTMLInputElement>(null);

  const toggleShowPassword = () => setShowPassword((v) => !v);

  const handleEditClick = () => {
    if (onEditClick) onEditClick();
  };

  const handleDatetimeIconClick = () => {
    if (datetimeInputRef.current) {
      datetimeInputRef.current.showPicker();
    }
  };

  return (
    <>
      {isPassword ? (
        <div className="input__wrapper">
          <input
            {...restProps}
            type={showPassword ? "text" : "password"}
            className={`input ${variantClass} ${className}${
              showPassword ? " active" : ""
            }`}
          />
          <span
            className="input__eye"
            onClick={toggleShowPassword}
            onKeyDown={(e) =>
              (e.key === "Enter" || e.key === " ") && toggleShowPassword()
            }
            aria-label={showPassword ? "Hide password" : "Show password"}
            aria-pressed={showPassword}
            tabIndex={0}
            role="button"
          />
          {children}
        </div>
      ) : variant === "edit" ? (
        <div className="input__wrapper">
          <input
            className={`input ${variantClass} ${className}`}
            {...restProps}
          />
          <span
            className="input__pencil"
            onClick={handleEditClick}
            aria-label="Edit field"
            tabIndex={0}
            role="button"
          />
          {children}
        </div>
      ) : variant === "datetime" ? (
        <div className="input__wrapper input__wrapper--datetime">
          <input
            ref={datetimeInputRef}
            type="datetime-local"
            className={`input ${variantClass} ${className} ${
              !restProps.value ? "input--empty" : ""
            }`}
            onClick={handleDatetimeIconClick}
            {...restProps}
          />
          {!restProps.value && restProps.placeholder && (
            <span className="input__placeholder">{restProps.placeholder}</span>
          )}
          <span
            className="input__date"
            onClick={handleDatetimeIconClick}
            aria-label="Select date and time"
            tabIndex={0}
            role="button"
          />
          {children}
        </div>
      ) : (
        <>
          <input
            className={`input ${variantClass} ${className}`}
            {...restProps}
          />
          {children}
        </>
      )}
    </>
  );
};

export default Input;
