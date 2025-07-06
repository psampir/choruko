import { useState } from "react";
import Button from '../../components/Button';

interface SignUpProps {
  onLoginClick: () => void;
}

function SignUp({ onLoginClick }: SignUpProps) {
  const [showPassword, setShowPassword] = useState(false);
  const [showPasswordConf, setShowPasswordConf] = useState(false);

  const toggleShowPassword = () => {
    setShowPassword(!showPassword);
  };

  const toggleShowPasswordConf = () => {
    setShowPasswordConf(!showPasswordConf);
  };

  const handleSignUp = () => {
    // Handle sign up logic here
    console.log("Signing up...");
  };

  return (
    <>
      <form className="login__form" style={{ gap: "16px" }}>
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

        <div className="login__input-wrapper">
          <input
            className={`standard__input login__input--password ${showPasswordConf ? " active" : ""}`}
            type={showPasswordConf ? "text" : "password"}
            placeholder="Confirm Password"
          />
          <span
            className="login__input-eye"
            onClick={toggleShowPasswordConf}
            aria-label="Show/Hide password"
            tabIndex={0}
            role="button"
          />
        </div>
      </form>

      <Button
        variant="primary"
        className="login__button--login"
        onClick={handleSignUp}
      >
        Sign Up
      </Button>

      <div className="login__bottom-container">
        <p className="login__without-account">
          Already have an account?
        </p>
        <Button
          variant="primary"
          className="login__button--signup"
          onClick={onLoginClick}
        >
          Log In
        </Button>
      </div>
    </>
  );
}

export default SignUp;
