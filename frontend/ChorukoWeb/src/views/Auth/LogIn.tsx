import Button from "../../components/Button";
import Input from "../../components/Input";

interface LoginProps {
  onSignUpClick: () => void;
  onResetPasswordClick: () => void;
}

function LogIn({ onSignUpClick, onResetPasswordClick }: LoginProps) {
  const handleLogin = () => {
    // Handle login logic here
    console.log("Logging in...");
  };

  return (
    <>
      <form className="login__form" style={{ gap: "22px" }}>
        <Input
          variant="email"
          className="login__input--email"
          type="text"
          placeholder="Email"
        />
        <Input
          variant="password"
          className={`input--password`}
          placeholder="Password"
        />
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
        <p className="login__without-account">Don't have an account?</p>
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
