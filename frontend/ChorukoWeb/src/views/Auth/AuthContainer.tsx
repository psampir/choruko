import { useState } from "react";
import LogIn from "./LogIn";
import SignUp from "./SignUp";
import ResetPassword from "./ResetPassword";
import RaccoonAnimation from '../../waving-loop';
import raccoonSvg from "../../assets/svg/raccoon-waving.svg";
import "./Auth.scss";

type AuthView = "login" | "signup" | "resetPassword";

function AuthContainer() {
  const [currentView, setCurrentView] = useState<AuthView>("login");

  const showLogin = () => setCurrentView("login");
  const showSignUp = () => setCurrentView("signup");
  const showResetPassword = () => setCurrentView("resetPassword");

  return (
    <div className="login active">
      {/* Common header with persistent animation */}
      <div className="login__top-container">
        <div className="login__logo-container">
          <RaccoonAnimation
            svgPath={raccoonSvg}
            restartInterval={8000}
            className="login__raccoon"
          />
        </div>
        <h1 className="login__title">Choruko</h1>

        {/* Different views */}
        {currentView === "login" && (
          <LogIn
            onSignUpClick={showSignUp}
            onResetPasswordClick={showResetPassword}
          />
        )}
        {currentView === "signup" && (
          <SignUp onLoginClick={showLogin} />
        )}
        {currentView === "resetPassword" && (
          <ResetPassword onLoginClick={showLogin} />
        )}
      </div>
    </div>
  );
}

export default AuthContainer;
