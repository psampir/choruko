import React, { useState, useRef, type InputHTMLAttributes } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import "./Input.scss";
import { useThemeDir } from "../../utils/iconUtils";

import dateIconLight from "../../assets/svg/light/date.svg";
import dateIconDark from "../../assets/svg/dark/date.svg";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  variant?: "primary" | "password" | "email" | "edit" | "datetime" | "currency";
  className?: string;
  children?: React.ReactNode;
  onEditClick?: () => void;
  currencySymbol?: string; // Custom symbol for currency variant (default: "#")
  dateValue?: Date | null;
  onDateChange?: (date: Date | null) => void;
}

const Input: React.FC<InputProps> = ({
  variant = "primary",
  className = "",
  children,
  onEditClick,
  currencySymbol = "#",
  dateValue,
  onDateChange,
  ...props
}) => {
  const themeDir = useThemeDir();
  const dateIcon = themeDir === "dark" ? dateIconDark : dateIconLight;

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
  const datetimeInputRef = useRef<DatePicker>(null);

  const toggleShowPassword = () => setShowPassword((v) => !v);

  const handleEditClick = () => {
    if (onEditClick) onEditClick();
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
          <DatePicker
            ref={datetimeInputRef}
            selected={dateValue}
            onChange={onDateChange || (() => {})}
            showTimeSelect
            timeFormat="HH:mm"
            timeIntervals={15}
            dateFormat="MMMM d, yyyy HH:mm"
            placeholderText={props.placeholder || "Select date and time"}
            className={`input input--datetime ${className}`}
            wrapperClassName="datepicker-wrapper"
            calendarClassName="datepicker-calendar"
            popperClassName="datepicker-popper"
            popperPlacement="bottom"
          />
          <span
            className="input__date"
            onClick={() => datetimeInputRef.current?.setFocus()}
            aria-label="Select date and time"
            tabIndex={0}
            role="button"
          >
            <img src={dateIcon} alt="calendar icon" />
          </span>
          {children}
        </div>
      ) : variant === "currency" ? (
        <div className="input__wrapper">
          <input
            className={`input ${variantClass} ${className}`}
            {...restProps}
          />
          <span className="input__currency-symbol">{currencySymbol}</span>
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
