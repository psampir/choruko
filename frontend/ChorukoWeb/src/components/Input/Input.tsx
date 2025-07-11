import React, { useState, type InputHTMLAttributes } from "react";
import "./Input.scss";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  variant?: "primary" | "password" | "email";
  className?: string;
  children?: React.ReactNode;
}

const Input: React.FC<InputProps> = ({
  variant = "primary",
  className = "",
  children,
  ...props
}) => {
  const variantClass = variant ? `input--${variant}` : "";
  const [showPassword, setShowPassword] = useState(false);

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };
  return (
    <>
      {variant === "password" ? (
        <div className="input__wrapper">
          <input 
            type={showPassword ? "text" : "password"}
            className={`input ${variantClass} ${className} ${showPassword ? " active" : ""}`} 
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
      ) : (
        <input className={`input ${variantClass} ${className}`} {...props}>
          {children}
        </input>
      )}
    </>
  );
};

export default Input;
