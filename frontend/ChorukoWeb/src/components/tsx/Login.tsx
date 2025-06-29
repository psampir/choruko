import "../scss/Login.scss";
import raccoon from "../../assets/svg/raccoon-waving.svg";
import { useState } from "react";

function Login() {
  const [showPassword, setShowPassword] = useState(false);
  const [showPasswordConf, setShowPasswordConf] = useState(false);

  const [resetPassword, setResetPassword] = useState(false);
  const [signIn, setSignIn] = useState(false);

  const toggleShowPassword: () => void = () => {
    setShowPassword(!showPassword);
  };
  const toggleShowPasswordConf: () => void = () => {
    setShowPasswordConf(!showPasswordConf);
  };
  const resetPasswordView: () => void = () => {
    setResetPassword(!resetPassword);
  };
  const signInView: () => void = () => {
    setSignIn(!signIn);
  };

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const toggleLoginInBtnName = () => {
    if (resetPassword) {
      return "Reset Password";
    } else if (signIn) {
      return "Sign Up";
    } else {
      return "Login In";
    }
  };

  const cleanInputs = () => {
    const emailInput = document.querySelector<HTMLInputElement>(".login__input--email");
    console.log(emailInput?.value.trim())
    if (emailInput && emailInput.value.trim() !== "") {
      if (isValidEmail(emailInput.value.trim())) {
        emailInput.value = "";
        document.querySelector(".login__popup-container")?.classList.add("active");
        setResetPassword(false);
      }
   } else {
      // Handle invalid email format
      alert("Please enter a valid email address");
    }
  };

  const loginBtnHandler = () => {
    if (resetPassword) {
        cleanInputs();
  
    } else if (signIn) {
      // Handle sign up logic here
      console.log("Signing up...");
    } else {
      // Handle login logic here
      console.log("Logging in...");
    }
  };

  const closePopup = () => {
    document.querySelector(".login__popup-container")?.classList.remove("active");
  };

  return (
    <div className="login active">
      <div className="login__popup-container">
        <div className="login__popup">
          <i className="login__close-icon" onClick={closePopup}></i>
          <h4 className="login__popup-message">
            A temporary password has been sent to your email.
          </h4>
          <button className="standard__button login__button--ok" onClick={closePopup}>OK</button>
        </div>
      </div>

      <div className="login__top-container">
        <div className="login__logo-container">
          {/* <picture>
            <source srcSet="/images/logo.webp" type="image/webp" media='(max-width: 480px)'/>
            <source srcSet="/images/logo2.webp" type="image/webp" media='(max-width: 768px)'/>
            <img className='login__logo' src="/images/logo.png" alt="Choruko Logo" />
          </picture> */}
          <img
            src={raccoon}
            alt="Racoon Animated Image"
            className="login__raccoon"
          />
        </div>
        <h1 className="login__title">Choruko</h1>
        <form className="login__form" style={{ gap: signIn ? "16px" : "22px" }}>
          <input className="standard__input login__input--email" type="text" placeholder="Email" />

          <div className="login__input-wrapper">
            <input
              className={`standard__input login__input--password ${
                showPassword ? " active" : ""
              }`}
              type={showPassword ? "text" : "password"}
              placeholder="Password"
              style={{ display: resetPassword ? "none" : "block" }}
            />
            <span
              className="login__input-eye"
              onClick={toggleShowPassword}
              aria-label="Show/Hide password"
              tabIndex={0}
              role="button"
              style={{ display: resetPassword ? "none" : "block" }}
            />
          </div>

          <div
            className="login__input-wrapper"
            style={{ display: signIn ? "block" : "none" }}
          >
            <input
              className={`standard__input login__input--password ${
                showPasswordConf ? " active" : ""
              }`}
              type={showPasswordConf ? "text" : "password"}
              placeholder="Confirm Password"
              style={{ display: resetPassword ? "none" : "block" }}
            />
            <span
              className="login__input-eye"
              onClick={toggleShowPasswordConf}
              aria-label="Show/Hide password"
              tabIndex={0}
              role="button"
              style={{ display: resetPassword ? "none" : "block" }}
            />
          </div>
        </form>

        <button
          className="standard__button login__button--login"
          onClick={loginBtnHandler}
          style={{ marginTop: resetPassword ? "0" : "" }}
        >
          {toggleLoginInBtnName()}
        </button>
        <button
          className="standard-additional__button login__button--reset"
          onClick={resetPasswordView}
        >
          {resetPassword ? "Go back to Login" : "Reset Password"}
        </button>
      </div>

      <div
        className="login__bottom-container"
        style={{ marginTop: resetPassword ? "77px" : "0" }}
      >
        <p className="login__without-account">
          {signIn ? "Already have an account?" : `Don't have an account?`}
        </p>
        <button
          className="standard__button login__button--signup"
          onClick={signInView}
        >
          {signIn ? "Login In" : "Sign Up"}
        </button>
      </div>
    </div>
  );
}

export default Login;
