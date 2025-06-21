import React from "react";
import "./Button.css";

interface ButtonProps {
  children: React.ReactNode;
  primary?: boolean;
  icon?: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({ children, primary, icon }) => {
  const className = [
    "button",
    primary ? "button--primary" : "",
    icon ? "button--with-icon" : ""
  ].join(" ").trim();

  return (
    <button className={className}>
      {icon && <span className="button__icon">{icon}</span>}
      {children}
    </button>
  );
};

export default Button;





// Button.css

// .button { /* Block */ }
// .button--primary { /* Modifier */ }
// .button--with-icon { /* Modifier */ }
// .button__icon { /* Element */ }



// Usage

// <Button primary icon={<MyIcon />}>Save</Button>
// <Button>Cancel</Button>