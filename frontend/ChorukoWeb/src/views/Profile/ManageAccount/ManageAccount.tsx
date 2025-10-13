import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useUser } from "../../../contexts/UserContext";
import "./ManageAccount.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";
import ProfileDefaultIcon from "../../../assets/svg/profile-icon-light.svg";
import arrowLeft from "../../../assets/svg/arrow-left.svg";
import { getProfilePictureUrl } from "../../../utils/imageUtils";

const ManageAccount: React.FC = () => {
  const navigate = useNavigate();
  const { user, updateUserNames, updatePassword, updateProfilePicture, deleteAccount, loading: userLoading } = useUser();
  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [firstNameValue, setFirstNameValue] = useState<string>("");
  const [lastNameValue, setLastNameValue] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");
  const [profileImage, setProfileImage] = useState<string>(ProfileDefaultIcon);

  // Initialize name values and profile picture from user data
  useEffect(() => {
    if (user) {
      setFirstNameValue(user.firstName);
      setLastNameValue(user.lastName);
      
      // Load profile picture from backend if available
      const pictureUrl = getProfilePictureUrl(user.profilePicturePath);
      if (pictureUrl) {
        setProfileImage(pictureUrl);
      }
    }
  }, [user]);

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

  const handleNameChange = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();

    if (!firstNameValue.trim() || !lastNameValue.trim()) {
      showNotification("Please enter both first and last name");
      return;
    }

    const result = await updateUserNames(firstNameValue, lastNameValue);
    
    if (result.success) {
      showNotification("Your name has been changed successfully");
    } else {
      showNotification(result.message || "Failed to change name");
    }
  };

  const handlePasswordChange = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();

    if (!newPassword || !confirmPassword) {
      showNotification("Please fill in all password fields");
      return;
    }

    if (newPassword !== confirmPassword) {
      showNotification("New passwords do not match");
      return;
    }

    const result = await updatePassword(newPassword, confirmPassword);
    
    if (result.success) {
      showNotification("Your password has been changed successfully");
      // Clear password fields after successful change
      setNewPassword("");
      setConfirmPassword("");
    } else {
      showNotification(result.message || "Failed to change password");
    }
  };

  const handleImageChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Update the preview immediately
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        setProfileImage(result);
      };
      reader.readAsDataURL(file);

      // Upload to server
      const result = await updateProfilePicture(file);
      
      if (result.success) {
        showNotification("Profile picture updated successfully");
      } else {
        showNotification(result.message || "Failed to update profile picture");
        // Revert to original image on failure
        if (user) {
          const pictureUrl = getProfilePictureUrl(user.profilePicturePath);
          if (pictureUrl) {
            setProfileImage(pictureUrl);
          } else {
            setProfileImage(ProfileDefaultIcon);
          }
        }
      }
    }
  };

  const triggerFileInput = () => {
    document.getElementById("profile-image-input")?.click();
  };

  const handleDeleteAccount = async () => {
    const result = await deleteAccount();
    
    if (result.success) {
      // User will be logged out and redirected by the deleteAccount function
      showNotification("Account deleted successfully");
    } else {
      showNotification(result.message || "Failed to delete account");
      setOpenPopup(false);
    }
  };

  return (
    <div className="manage-account">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate(-1)}
      />
      <div
        className={`notification-bar ${notification.isVisible ? "notification-bar--visible" : ""
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
            placeholder="First Name"
            value={firstNameValue}
            onChange={(e) => setFirstNameValue(e.target.value)}
          />
          <Input
            variant="edit"
            placeholder="Last Name"
            value={lastNameValue}
            onChange={(e) => setLastNameValue(e.target.value)}
          />
          <Button variant="primary" type="button" onClick={handleNameChange} disabled={userLoading}>
            {userLoading ? "Changing..." : "Change Name"}
          </Button>
        </div>
        <div className="manage-account__main-section change-password">
          <Input 
            variant="password" 
            placeholder="New Password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
          />
          <Input 
            variant="password" 
            placeholder="Confirm New Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
          />
          <Button variant="primary" type="button" onClick={handlePasswordChange} disabled={userLoading}>
            {userLoading ? "Changing..." : "Change Password"}
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
          <p>{user ? `${user.firstName} ${user.lastName}` : `${firstNameValue} ${lastNameValue}`}</p>
          <Button variant="red" onClick={handleDeleteAccount} disabled={userLoading}>
            {userLoading ? "Deleting..." : "Delete"}
          </Button>
        </Popup>
      )}
    </div>
  );
};

export default ManageAccount;
