import { useState, useEffect } from "react";
import Button from "../../components/Button";
import Input from "../../components/Input";
import ErrorMessage from "../../components/ErrorMessage";
import authService from "../../services/authService";
import { parseErrorMessage } from "../../utils/errorUtils";
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface ResetPasswordProps {
  onLoginClick: () => void;
}

// Password reset - checks backend email config before allowing reset
function ResetPassword({ onLoginClick }: ResetPasswordProps) {
  const [showPopup, setShowPopup] = useState(false);
  const [emailEnabled, setEmailEnabled] = useState<boolean | null>(null); // null=loading, true=enabled, false=disabled
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Check if email functionality is configured on backend
  useEffect(() => {
    let ignore = false;

    const loadConfig = async () => {
      try {
        const res = await fetch(`${apiBaseUrl}/config/email-enabled`, {
          headers: { Accept: "application/json" },
        });
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const data = await res.json();
        if (!ignore) setEmailEnabled(Boolean(data?.enableEmails));
      } catch (err) {
        console.error("Failed to fetch email-enabled config", err);
        if (!ignore) setEmailEnabled(false);
      }
    };
    loadConfig();

    return () => {
      ignore = true;
    };
  }, []);

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!email.trim()) {
      const errorMsg = parseErrorMessage("Please enter your email address");
      setError(errorMsg);
      return;
    }

    if (!isValidEmail(email.trim())) {
      const errorMsg = parseErrorMessage("Please enter a valid email address");
      setError(errorMsg);
      return;
    }

    setLoading(true);

    try {
      const result = await authService.forgotPassword(email);

      if (result.success) {
        setEmail("");
        setShowPopup(true);
      } else {
        const errorMsg = parseErrorMessage(result.message || "Failed to send reset email");
        setError(errorMsg);
      }
    } catch (error: any) {
      const errorMsg = parseErrorMessage(error.message || "Network error occurred");
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  const closePopup = () => {
    setShowPopup(false);
  };

  return (
    <>
      <ErrorMessage
        message={error}
        variant="overlay"
        dismissTime={5000}
        onClose={() => setError("")}
      />

      {emailEnabled === true && showPopup && (
        <div className="login__popup-container active">
          <div className="login__popup">
            <i className="login__close-icon" onClick={closePopup}></i>
            <h4 className="login__popup-message">
              A temporary password has been sent to your email.
            </h4>
            <Button
              variant="primary-popup"
              className="login__button--ok"
              onClick={closePopup}
            >
              OK
            </Button>
          </div>
        </div>
      )}

      {emailEnabled === null ? (
        <div className="login__loading">Loading...</div>
      ) : emailEnabled ? (
        <form className="login__form" style={{ gap: "22px" }} onSubmit={handleResetPassword}>
          <Input
            style={{ minWidth: "288px" }}
            variant="email"
            className="login__input--email"
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />

          <Button
            variant="primary"
            className="login__button--login"
            type="submit"
            disabled={loading}
          >
            {loading ? "Sending..." : "Reset Password"}
          </Button>
        </form>
      ) : (
        <div className="login__notice">
          Automated password reset is not configured.
          <br />
          Please contact your administrator to reset your password manually.
        </div>
      )}

      <Button
        variant="additional"
        className="login__button--reset"
        onClick={onLoginClick}
      >
        Go back to Login
      </Button>
    </>
  );
}

export default ResetPassword;
