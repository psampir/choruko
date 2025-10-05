import React, { useState } from "react";
import Button from "../../components/Button";
import Input from "../../components/Input";
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import ErrorMessage from "../../components/ErrorMessage";

interface SignUpProps {
  onModeChange: (mode: string) => void;
}

export const SignUp: React.FC<SignUpProps> = ({ onModeChange }) => {
  const {
    register,
    loading: authLoading,
    error: authError,
    clearError,
  } = useAuth();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [localError, setLocalError] = useState("");

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const parseErrorMessage = (errorMessage: string): string => {
    try {
      const errorData = JSON.parse(errorMessage);
      if (errorData.errors) {
        const fieldErrors = Object.values(errorData.errors).flat() as string[];
        return fieldErrors.join(". ");
      }
      if (errorData.title) return errorData.title;
      if (errorData.message) return errorData.message;
      return "An error occurred. Please try again.";
    } catch {
      return errorMessage || "An unexpected error occurred. Please try again.";
    }
  };

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError("");

    // Basic validation
    if (formData.password !== formData.confirmPassword) {
      setLocalError("Passwords do not match");
      return;
    }

    try {
      const result = await register(formData);

      if (result.success) {
        // Registration successful - navigate to dashboard
        navigate("/dashboard");
      } else {
        // added failure handling
        setLocalError(parseErrorMessage(result.message || "Sign up failed."));
      }
    } catch (error: any) {
      // added catch parsing
      setLocalError(
        parseErrorMessage(
          error?.message || "Connection failed. Please try again."
        )
      );
    }
  };

  return (
    <>
      {/* added ErrorMessage overlay */}
      <ErrorMessage
        message={localError || authError || ""}
        variant="overlay"
        dismissTime={5000}
        onClose={() => {
          setLocalError("");
          clearError();
        }}
      />
      <div className="login">
        <form onSubmit={handleSignUp} className="login__form">
          <div className="login__input-container">
            <Input
              type="text"
              name="firstName"
              placeholder="First Name"
              value={formData.firstName}
              onChange={handleInputChange}
              required
            />

            <Input
              type="text"
              name="lastName"
              placeholder="Last Name"
              value={formData.lastName}
              onChange={handleInputChange}
              required
            />

            <Input
              type="email"
              name="email"
              placeholder="Email"
              value={formData.email}
              onChange={handleInputChange}
              required
            />

            <Input
              type="password"
              name="password"
              placeholder="Password"
              value={formData.password}
              onChange={handleInputChange}
              required
            />

            <Input
              type="password"
              name="confirmPassword"
              placeholder="Confirm Password"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              required
            />
          </div>

          <Button
            type="submit"
            className="login__button--signup"
            disabled={authLoading}
          >
            {authLoading ? "Creating Account..." : "Sign Up"}
          </Button>
        </form>

        <div className="login__bottom-container">
          <p className="login__without-account">Already have an account?</p>
          <Button
            className="login__button--login"
            onClick={() => onModeChange("login")}
          >
            Log In
          </Button>
        </div>
      </div>
    </>
  );
};
