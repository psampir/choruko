import { useState } from "react";
import Button from '../../components/Button';

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
        <input
          className="standard__input login__input--email"
          type="text"
          placeholder="Email"
        />
      </form>

      <Button
        variant="primary"
        className="login__button--login"
        onClick={handleResetPassword}
      // style={{ marginTop: "0" }}
      >
        Reset Password
      </Button>
      <Button
        variant="additional"
        className="login__button--reset"
        onClick={onLoginClick}
      >
        Go back to Login
      </Button>

      <div
        className="login__bottom-container"
        style={{ marginTop: "77px" }}
      ></div>
    </>
  );
}

export default ResetPassword;
