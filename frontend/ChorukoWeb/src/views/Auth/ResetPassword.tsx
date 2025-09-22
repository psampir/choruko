import { useState } from "react";
import Button from '../../components/Button';
import Input from '../../components/Input';

interface ResetPasswordProps {
  onLoginClick: () => void;
}

function ResetPassword({ onLoginClick }: ResetPasswordProps) {
  const [showPopup, setShowPopup] = useState(false);

  const isValidEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleResetPassword = () => {
    const emailInput = document.querySelector<HTMLInputElement>(
      ".login__input--email"
    );

    if (emailInput && emailInput.value.trim() !== "") {
      if (isValidEmail(emailInput.value.trim())) {
        emailInput.value = "";
        setShowPopup(true);
      } else {
        // Handle invalid email format
        alert("Please enter a valid email address");
      }
    } else {
      // Handle empty email
      alert("Please enter your email address");
    }
  };

  const closePopup = () => {
    setShowPopup(false);
  };

  return (
    <>
      {showPopup && (
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

      <form className="login__form" style={{ gap: "22px" }}>
        <Input
          variant="email"
          className="login__input--email"
          type="text"
          placeholder="Email"
        />

        <Button
          variant="primary"
          className="login__button--login"
          onClick={handleResetPassword}
        >
          Reset Password
        </Button>
      </form>

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
