import React from 'react';
import type { ButtonHTMLAttributes } from 'react';
import './Button.scss';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'primary-popup' | 'red' | 'red-popup' | 'additional' | 'add';
  className?: string;
  children: React.ReactNode;
}

const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  className = '',
  children,
  ...props
}) => {
  const variantClass = variant ? `button--${variant}` : '';

  return (
    <button
      className={`button ${variantClass} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
};

export default Button;
