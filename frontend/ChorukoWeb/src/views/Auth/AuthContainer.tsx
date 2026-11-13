import { useState, useEffect } from "react";
import LogIn from "./LogIn";
import { SignUp } from "./SignUp";
import ForgotPassword from "./ForgotPassword";
import "./Auth.scss";
import RaccoonLogo from "../../components/RaccoonLogo";
import { useAuth } from "../../contexts/AuthContext";
import PWAInstallPopup from "../../components/PWAInstallPopup";
import Button from "../../components/Button";

type AuthView = "login" | "signup" | "resetPassword";

// Main auth wrapper managing login/signup/reset navigation
function AuthContainer() {
  const [currentView, setCurrentView] = useState<AuthView>("login");
  const { clearError } = useAuth();
  const [showInstallButton, setShowInstallButton] = useState(false);
  const [showPWAPopup, setShowPWAPopup] = useState(false);

  useEffect(() => {
    // Check if app is running in standalone mode (PWA installed)
    const navigatorWithStandalone = window.navigator as Navigator & { standalone?: boolean };
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches || 
                         navigatorWithStandalone.standalone === true;
    
    // Check if on mobile device
    const isMobile = /iPhone|iPad|iPod|Android/i.test(navigator.userAgent);
    
    // Show install button only if: not in standalone mode and on mobile
    if (!isStandalone && isMobile) {
      setShowInstallButton(true);
    }
  }, []);

  const showLogin = () => {
    clearError();
    setCurrentView("login");
  };

  const handleModeChange = (mode: string) => {
    clearError();
    setCurrentView(mode as AuthView);
  };

  return (
    <>
      <PWAInstallPopup open={showPWAPopup} onClose={() => setShowPWAPopup(false)} />
      
      <div className="version">{import.meta.env.VITE_APP_VERSION}</div>
      {showInstallButton && (
        <Button 
          variant="additional"
          className="install-button" 
          onClick={() => setShowPWAPopup(true)}
          aria-label="Install app"
        >
          Install
        </Button>
      )}
      <div className="login active">
        {/* Common header with persistent raccoon animation and app title */}
        <div className="login__top-container">
          <RaccoonLogo />
          <h1 className="login__title">Choruko</h1>

          {/* Conditionally render auth views based on currentView state */}
          {currentView === "login" && <LogIn onModeChange={handleModeChange} />}
          {currentView === "signup" && <SignUp onModeChange={handleModeChange} />}
          {currentView === "resetPassword" && (
            <ForgotPassword onLoginClick={showLogin} />
          )}
        </div>
      </div>
    </>
  );
}

export default AuthContainer;
