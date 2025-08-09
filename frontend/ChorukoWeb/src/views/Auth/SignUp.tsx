import Button from "../../components/Button";
import Input from "../../components/Input";

interface SignUpProps {
  onLoginClick: () => void;
}

function SignUp({ onLoginClick }: SignUpProps) {
  const handleSignUp = () => {
    // Handle sign up logic here
    console.log("Signing up...");
  };

  return (
    <>
      <form className="login__form" style={{ gap: "22px" }}>
        <Input
          variant="primary"
          type="text"
          placeholder="Name"
        />
        <Input
          variant="primary"
          placeholder="Last Name"
        />
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
        <Input
          variant="password"
          className={`input--password`}
          placeholder="Password"
        />
      </form>

      <Button
        variant="primary"
        className="login__button--login"
        onClick={handleSignUp}
      >
        Sign Up
      </Button>

      <div className="login__bottom-container">
        <p className="login__without-account">Already have an account?</p>
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
