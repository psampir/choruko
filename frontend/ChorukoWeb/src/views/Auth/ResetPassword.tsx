import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import Button from "../../components/Button";
import Input from "../../components/Input";
import authService from "../../services/authService";
import "./Auth.scss";
import RaccoonLogo from "../../components/RaccoonLogo";

function ResetPassword() {
  const [searchParams] = useSearchParams();
  
  const [email, setEmail] = useState("");
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    const emailParam = searchParams.get("email");
    const tokenParam = searchParams.get("token");

    if (!emailParam || !tokenParam) {
      setError("Invalid reset link. Please request a new password reset.");
      return;
    }

    setEmail(emailParam);
    setToken(tokenParam);
  }, [searchParams]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!newPassword || !confirmPassword) {
      setError("Please fill in all fields");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("Passwords do not match");
      return;
    }

    if (newPassword.length < 6) {
      setError("Password must be at least 6 characters long");
      return;
    }

    setLoading(true);

    try {
      const result = await authService.resetPassword(
        email,
        token,
        newPassword,
        confirmPassword
      );

      if (result.success) {
        setSuccess(true);
      } else {
        setError(result.message || "Failed to reset password");
      }
    } catch (error: any) {
      setError(error.message || "Network error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <div className="version">{import.meta.env.VITE_APP_VERSION}</div>
      <div className="login active">
        <div className="login__top-container">
          <RaccoonLogo />
          <h1 className="login__title">Choruko</h1>

          {success ? (
            <>
              <h2 className="login__subtitle">Password Reset Successfully!</h2>
              <p style={{ textAlign: "center", marginBottom: "20px" }}>
                Your password has been reset successfully.
              </p>
              <Button
                variant="primary"
                onClick={() => window.location.href = "/login"}
              >
                Go to Login
              </Button>
            </>
          ) : error && (!email || !token) ? (
            <>
              <h2 className="login__subtitle">Invalid Reset Link</h2>
              <p style={{ textAlign: "center", marginBottom: "20px", color: "red" }}>
                {error}
              </p>
              <Button
                variant="primary"
                onClick={() => window.location.href = "/login"}
              >
                Go to Login
              </Button>
            </>
          ) : (
            <>
              {error && (
                <div style={{ color: "red", marginBottom: "20px", textAlign: "center" }}>
                  {error}
                </div>
              )}

              <form onSubmit={handleSubmit} className="login__form">
                <div className="login__input-container">
                  <Input
                    type="password"
                    placeholder="New Password"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    required
                  />

                  <Input
                    type="password"
                    placeholder="Confirm New Password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    required
                  />
                </div>

                <Button
                  type="submit"
                  className="login__button--login"
                  disabled={loading}
                >
                  {loading ? "Resetting..." : "Reset Password"}
                </Button>
              </form>

              <Button
                variant="additional"
                className="login__button--reset"
                onClick={() => window.location.href = "/login"}
              >
                Back to Login
              </Button>
            </>
          )}
        </div>
      </div>
    </>
  );
}

export default ResetPassword;
