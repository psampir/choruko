import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useUser } from "../../../contexts/UserContext";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import "./ManageAccount.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";
import ProfileDefaultIconLight from "../../../assets/svg/light/profile-icon-light.svg";
import arrowLeftLight from "../../../assets/svg/light/arrow-left.svg";
import ProfileDefaultIconDark from "../../../assets/svg/dark/profile-icon-light.svg";
import arrowLeftDark from "../../../assets/svg/dark/arrow-left.svg";
import { getProfilePictureUrl } from "../../../utils/imageUtils";

// User account management - update name, password, and profile picture
const ManageAccount: React.FC = () => {
  const navigate = useNavigate();
  const {
    user,
    updateUserNames,
    updatePassword,
    updateProfilePicture,
    deleteAccount,
    loading: userLoading,
  } = useUser();
  const { refreshHousehold } = useHousehold();
  const isDark = useIsDarkTheme();

  const ProfileDefaultIcon = isDark
    ? ProfileDefaultIconDark
    : ProfileDefaultIconLight;
  const arrowLeft = isDark ? arrowLeftDark : arrowLeftLight;

  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [firstNameValue, setFirstNameValue] = useState<string>("");
  const [lastNameValue, setLastNameValue] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");
  const [profileImageSrc, setProfileImageSrc] =
    useState<string>(ProfileDefaultIcon);

  // Initialize form values and profile picture from user data
  useEffect(() => {
    if (user) {
      setFirstNameValue(user.firstName);
      setLastNameValue(user.lastName);

      const pictureUrl = getProfilePictureUrl(user.profilePicturePath);
      if (pictureUrl) {
        setProfileImageSrc(pictureUrl);
      } else {
        setProfileImageSrc(ProfileDefaultIcon);
      }
    }
  }, [user, ProfileDefaultIcon]);

  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
    type: "success" | "error";
  }>({
    message: "",
    isVisible: false,
    type: "success",
  });

  const showNotification = (
    message: string,
    type: "success" | "error" = "success"
  ) => {
    setNotification({ message, isVisible: true, type });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false, type: "success" });
    }, 3000);
  };

  const handleNameChange = async (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();

    if (!firstNameValue.trim() || !lastNameValue.trim()) {
      showNotification("Please enter both first and last name", "error");
      return;
    }

    const result = await updateUserNames(firstNameValue, lastNameValue);

    if (result.success) {
      await refreshHousehold();
      showNotification("Your name has been changed successfully", "success");
    } else {
      showNotification(result.message || "Failed to change name", "error");
    }
  };

  const handlePasswordChange = async (
    e: React.MouseEvent<HTMLButtonElement>
  ) => {
    e.preventDefault();
    e.stopPropagation();

    if (!newPassword || !confirmPassword) {
      showNotification("Please fill in all password fields", "error");
      return;
    }

    if (newPassword !== confirmPassword) {
      showNotification("New passwords do not match", "error");
      return;
    }

    const result = await updatePassword(newPassword, confirmPassword);

    if (result.success) {
      showNotification(
        "Your password has been changed successfully",
        "success"
      );
      // Clear password fields after successful change
      setNewPassword("");
      setConfirmPassword("");
    } else {
      showNotification(result.message || "Failed to change password", "error");
    }
  };

  const handleImageChange = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (file) {
      // Upload to server first
      const result = await updateProfilePicture(file);

      if (result.success) {
        // Only update preview after successful upload
        const reader = new FileReader();
        reader.onload = (e) => {
          const result = e.target?.result as string;
          setProfileImageSrc(result);
        };
        reader.readAsDataURL(file);
        await refreshHousehold();
        showNotification("Profile picture updated successfully", "success");
      } else {
        showNotification(
          result.message || "Failed to update profile picture",
          "error"
        );
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
      showNotification("Account deleted successfully", "success");
    } else {
      showNotification(result.message || "Failed to delete account", "error");
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
        className={`notification-bar notification-bar--${notification.type} ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-account__template-name">Manage Account</h1>
      <div className="account-header">
        <img className="account-image" src={profileImageSrc} alt="Account" />
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
          <Button
            variant="primary"
            type="button"
            onClick={handleNameChange}
            disabled={userLoading}
          >
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
          <Button
            variant="primary"
            type="button"
            onClick={handlePasswordChange}
            disabled={userLoading}
          >
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
          <img src={profileImageSrc} alt="Profile Image" />
          <p>
            {user
              ? `${user.firstName} ${user.lastName}`
              : `${firstNameValue} ${lastNameValue}`}
          </p>
          <Button
            variant="red"
            onClick={handleDeleteAccount}
            disabled={userLoading}
          >
            {userLoading ? "Deleting..." : "Delete"}
          </Button>
        </Popup>
      )}
    </div>
  );
};

export default ManageAccount;
