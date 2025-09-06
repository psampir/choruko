import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./ManageAccount.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";
import ProfileDefaultIcon from "../../../assets/svg/profile-icon-light.svg";
import arrowLeft from "../../../assets/svg/arrow-left.svg";

const ManageAccount: React.FC = () => {
  const navigate = useNavigate();
  const [openPopup, setOpenPopup] = useState<Boolean>(false);
  const [nameValue, setNameValue] = useState<string>("Vin Diesel");
  const [profileImage, setProfileImage] = useState<string>(ProfileDefaultIcon);

  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
  }>({
    message: "",
    isVisible: false,
  });

  const showNotification = (message: string) => {
    setNotification({ message, isVisible: true });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false });
    }, 3000);
  };

  const handleNameChange = () => {
    if (nameValue.trim()) {
      showNotification("Your name has been changed");
    }
  };

  const handlePasswordChange = () => {
    showNotification("Your password has been changed");
  };

  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        setProfileImage(result);
        showNotification("Profile picture updated");
      };
      reader.readAsDataURL(file);
    }
  };

  const triggerFileInput = () => {
    document.getElementById("profile-image-input")?.click();
  };

  return (
    <div className="manage-account">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate("/leaderboard")}
      />
      <div
        className={`notification-bar ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-account__template-name">Manage Account</h1>
      <div className="account-header">
        <img className="account-image" src={profileImage} alt="Account" />
        <Button variant="primary" onClick={triggerFileInput}>
          Change Picture
        </Button>
        <input
          id="profile-image-input"
          type="file"
          accept="image/*"
          onChange={handleImageChange}
          style={{ display: "none" }}
        />
      </div>
      <div className="manage-account__main-section">
        <div className="manage-account__main-section change-name">
          <Input
            variant="edit"
            value={nameValue}
            onChange={(e) => setNameValue(e.target.value)}
          />
          <Button variant="primary" onClick={handleNameChange}>
            Change Name
          </Button>
        </div>
        <div className="manage-account__main-section change-password">
          <Input variant="password" placeholder="New Password" />
          <Input variant="password" placeholder="Confirm New Password" />
          <Button variant="primary" onClick={handlePasswordChange}>
            Change Password
          </Button>
        </div>
        <Button
          variant="red"
          className="delete-account__btn"
          onClick={() => setOpenPopup(true)}
        >
          Delete Account
        </Button>
      </div>
      {openPopup && (
        <Popup variant="red" title="" onClose={() => setOpenPopup(false)}>
          <p>Are you sure you want to delete your account?</p>
          <img src={profileImage} alt="Profile Image" />
          <p>Vin Diesel</p>
          <Button variant="red">Delete</Button>
        </Popup>
      )}
    </div>
  );
};

export default ManageAccount;
