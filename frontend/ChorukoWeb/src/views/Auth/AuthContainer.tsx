import { useState } from "react";
import LogIn from "./LogIn";
import { SignUp } from "./SignUp";
import ResetPassword from "./ResetPassword";
import "./Auth.scss";
import RaccoonLogo from "../../components/RaccoonLogo";

type AuthView = "login" | "signup" | "resetPassword";

// Main auth wrapper managing login/signup/reset navigation
function AuthContainer() {
  const [currentView, setCurrentView] = useState<AuthView>("login");

  const showLogin = () => setCurrentView("login");

  const handleModeChange = (mode: string) => {
    setCurrentView(mode as AuthView);
  };

  return (
    <div className="login active">
      {/* Common header with persistent raccoon animation and app title */}
      <div className="login__top-container">
        <RaccoonLogo />
        <h1 className="login__title">Choruko</h1>

        {/* Conditionally render auth views based on currentView state */}
        {currentView === "login" && <LogIn onModeChange={handleModeChange} />}
        {currentView === "signup" && <SignUp onModeChange={handleModeChange} />}
        {currentView === "resetPassword" && (
          <ResetPassword onLoginClick={showLogin} />
        )}
      </div>
    </div>
  );
}

export default AuthContainer;
