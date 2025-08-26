import React, { useState } from "react";
import Button from "../../components/Button";
import Input from "../../components/Input";
import { useAuth } from "../../contexts/AuthContext"; // Use AuthContext

interface SignUpProps {
  onModeChange: (mode: string) => void;
}

export const SignUp: React.FC<SignUpProps> = ({ onModeChange }) => {
  const { register, loading: authLoading, error: authError } = useAuth();
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

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    console.log("🚀 SignUp form submitted!");
    setLocalError("");

    // Basic validation
    if (formData.password !== formData.confirmPassword) {
      console.log("❌ Password mismatch");
      setLocalError("Passwords do not match");
      return;
    }

    console.log("✅ Validation passed, calling register...");
    try {
      const success = await register(formData);
      console.log("📡 Register response:", success);

      if (success) {
        // Registration successful - redirect to login
        console.log("Registration successful!");
        onModeChange("login");
      }
    } catch (error) {
      console.error("Registration error:", error);
    }
  };

  return (
    <div className="login">
      <form onSubmit={handleSignUp} className="login__form">
        {(localError || authError) && (
          <div
            className="login__error"
            style={{ color: "red", marginBottom: "10px" }}
          >
            {localError || authError}
          </div>
        )}

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
  );
};
