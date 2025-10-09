import { useState } from "react";
import LogIn from "./LogIn";
import { SignUp } from "./SignUp";
import ResetPassword from "./ResetPassword";
import "./Auth.scss";
import RaccoonLogo from "../../components/RaccoonLogo";

type AuthView = "login" | "signup" | "resetPassword";

function AuthContainer() {
  const [currentView, setCurrentView] = useState<AuthView>("login");

  const showLogin = () => setCurrentView("login");

  const handleModeChange = (mode: string) => {
    setCurrentView(mode as AuthView);
  };

  return (
    <div className="login active">
      {/* Common header with persistent animation */}
      <div className="login__top-container">
        <RaccoonLogo />
        <h1 className="login__title">Choruko</h1>

        {/* Different views */}
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
