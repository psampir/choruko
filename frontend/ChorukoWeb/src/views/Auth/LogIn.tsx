import { useState } from "react";
import Button from '../../components/Button';

interface LoginProps {
  onSignUpClick: () => void;
  onResetPasswordClick: () => void;
}

function LogIn({ onSignUpClick, onResetPasswordClick }: LoginProps) {
  const [showPassword, setShowPassword] = useState(false);

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };

  const handleLogin = () => {
    // Handle login logic here
    console.log("Logging in...");
  };

  return (
    <>
      <form className="login__form" style={{ gap: "22px" }}>
        <input
          className="standard__input login__input--email"
          type="text"
          placeholder="Email"
        />

        <div className="login__input-wrapper">
          <input
            className={`standard__input login__input--password ${showPassword ? " active" : ""}`}
            type={showPassword ? "text" : "password"}
            placeholder="Password"
          />
          <span
            className="login__input-eye"
            onClick={toggleShowPassword}
            aria-label="Show/Hide password"
            tabIndex={0}
            role="button"
          />
        </div>
      </form>

      <Button
        variant="primary"
        className="login__button--login"
        onClick={handleLogin}
      >
        Log In
      </Button>
      <Button
        variant="additional"
        className="login__button--reset"
        onClick={onResetPasswordClick}
      >
        Reset Password
      </Button>

      <div className="login__bottom-container">
        <p className="login__without-account">
          Don't have an account?
        </p>
        <Button
          variant="primary"
          className="login__button--signup"
          onClick={onSignUpClick}
        >
          Sign Up
        </Button>
      </div>
    </>
  );
}

export default LogIn;
