import Button from "../../components/Button";
import Input from "../../components/Input";
import ErrorMessage from "../../components/ErrorMessage";
import React, { useState } from "react";
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import { parseErrorMessage } from "../../utils/errorUtils";

interface LogInProps {
  onModeChange: (mode: string) => void;
}

export const LogIn: React.FC<LogInProps> = ({ onModeChange }) => {
  const {
    login,
    loading: authLoading,
    error: authError,
    clearError,
  } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [localError, setLocalError] = useState("");

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError("");

    try {
      const result = await login(email, password);

      if (result.success) {
        console.log("Login successful!");
        // Navigate based on household status
        if (result.hasHousehold) {
          navigate("/leaderboard");
        } else {
          navigate("/joinOrCreateHousehold");
        }
      } else {
        // Handle login failure with backend error message
        const errorMsg = parseErrorMessage(
          result.message || "Login failed. Please try again."
        );
        setLocalError(errorMsg);
      }
    } catch (error: any) {
      // Handle network or other errors
      const errorMsg = parseErrorMessage(
        error.message ||
        "Connection failed. Please check your internet and try again."
      );
      setLocalError(errorMsg);
    }
  };

  return (
    <>
      <ErrorMessage
        message={localError || authError || ""}
        variant="overlay"
        dismissTime={5000}
        onClose={() => {
          setLocalError("");
          clearError(); // Clear the auth context error as well
        }}
      />

      <div className="login">
        <form onSubmit={handleLogin} className="login__form">
          <div className="login__input-container">
            <Input
              className="login__input-email"
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <Input
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          <Button
            type="submit"
            className="login__button--login"
            disabled={authLoading}
          >
            {authLoading ? "Logging in..." : "Log In"}
          </Button>

          <Button
            variant="additional"
            type="button"
            className="login__button--reset"
            onClick={() => onModeChange("resetPassword")}
          >
            Reset Password
          </Button>
        </form>

        <div className="login__bottom-container">
          <p className="login__without-account">Don't have an account?</p>
          <Button
            className="login__button--signup"
            onClick={() => onModeChange("signup")}
          >
            Sign Up
          </Button>
        </div>
      </div>
    </>
  );
};

export default LogIn;
