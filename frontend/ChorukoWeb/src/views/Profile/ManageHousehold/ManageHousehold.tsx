import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { useAuth } from "../../../contexts/AuthContext";
import householdService from "../../../services/householdService";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import "./ManageHousehold.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";

import householdDefaulIcon from "../../../assets/svg/householdDefault.svg";
import profileDefaultIcon from "../../../assets/svg/profile-icon.svg";
import arrowLeft from "../../../assets/svg/arrow-left.svg";
import defineActivity from "../../../assets/svg/activities.svg";
import delegateAdmin from "../../../assets/svg/star-emty.svg";
import addParticipants from "../../../assets/svg/profile-icon-light.svg";
import deleteHousehold from "../../../assets/svg/trash-bin.svg";
import decline from "../../../assets/svg/decline.svg"

const buttonActions = {
  defineActivity: "define_activity",
  delegateAdmin: "delegate_admin",
  addParticipants: "add_participants",
  deleteHousehold: "delete_household",
  deleteParticipant: "delete_participant",
} as const;

type ButtonAction = (typeof buttonActions)[keyof typeof buttonActions];

const ManageHousehold: React.FC = () => {
  const navigate = useNavigate();
  const { householdInfo, members, refreshHousehold } = useHousehold();
  const { logout } = useAuth();
  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [nameValue, setNameValue] = useState<string>("");
  const [profileImage, setProfileImage] = useState<string>(householdDefaulIcon);
  const [clickedButton, setClickedButton] = useState<ButtonAction | null>(null);
  const [loading, setLoading] = useState(false);

  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
    type: "success" | "error";
  }>({
    message: "",
    isVisible: false,
    type: "success",
  });

  // Initialize values from householdInfo
  useEffect(() => {
    if (householdInfo) {
      setNameValue(householdInfo.name);
      setProfileImage(householdInfo.profilePictureUrl || householdDefaulIcon);
    }
  }, [householdInfo]);

  const showNotification = (message: string, type: "success" | "error" = "success") => {
    setNotification({ message, isVisible: true, type });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false, type: "success" });
    }, 3000);
  };

  const handleNameChange = async () => {
    if (!nameValue.trim()) {
      showNotification("Please enter a household name", "error");
      return;
    }

    if (nameValue.trim().length < 3) {
      showNotification("Household name must be at least 3 characters", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await householdService.renameHousehold(nameValue.trim());
      if (result.success) {
        showNotification("Household name changed successfully", "success");
        await refreshHousehold();
      } else {
        showNotification(result.message || "Failed to change household name", "error");
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleImageChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Show preview
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        setProfileImage(result);
      };
      reader.readAsDataURL(file);

      // Upload to server
      setLoading(true);
      try {
        const result = await householdService.changeHouseholdPicture(file);
        if (result.success) {
          showNotification("Household picture changed successfully", "success");
          await refreshHousehold();
        } else {
          showNotification(result.message || "Failed to change household picture", "error");
          // Revert preview on failure
          setProfileImage(householdInfo?.profilePictureUrl || householdDefaulIcon);
        }
      } catch (error) {
        showNotification("An unexpected error occurred", "error");
        setProfileImage(householdInfo?.profilePictureUrl || householdDefaulIcon);
      } finally {
        setLoading(false);
      }
    }
  };

  const triggerFileInput = () => {
    document.getElementById("profile-image-input")?.click();
  };

  const handleButtonClick = (action: ButtonAction) => {
    setClickedButton(action);
    if (action !== "define_activity") {
      setOpenPopup(true);
    } else {
      navigate("/activities");
    }
    console.log(
      "Button clicked:",
      action,
      "Current clicked button state:",
      clickedButton
    );
  };

  const handleDelegateAdmin = (event: React.MouseEvent<HTMLDivElement>) => {
    const participantsListInPopup =
      event.currentTarget.closest(".participants-list");

    participantsListInPopup
      ?.querySelectorAll(".participant__delegate-admin")
      .forEach((element) => {
        element.classList.remove("admin-choose");
      });
    event.currentTarget.classList.add("admin-choose");
  };

  const handleDeleteHousehold = async () => {
    setLoading(true);
    try {
      const result = await householdService.deleteHousehold();
      if (result.success) {
        showNotification("Household deleted successfully", "success");
        setOpenPopup(false);
        // Redirect to joinOrCreateHousehold after a short delay
        setTimeout(async () => {
          navigate("/joinOrCreateHousehold");
        }, 1500);
      } else {
        showNotification(result.message || "Failed to delete household", "error");
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="manage-household">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate(-1)}
      />
      <div
        className={`notification-bar notification-bar--${notification.type} ${notification.isVisible ? "notification-bar--visible" : ""
          }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-household__title">Manage Household</h1>
      <div className="household-header">
        <img className="household-image" src={profileImage} alt="Household" />
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
      <div className="manage-household__main-section">
        <div className="manage-household__name-section">
          <Input
            variant="edit"
            value={nameValue}
            onChange={(e) => setNameValue(e.target.value)}
          />
          <Button variant="primary" onClick={handleNameChange} disabled={loading}>
            {loading ? "Saving..." : "Change Name"}
          </Button>
        </div>
        <div className="manage-household__buttons-grid">
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.defineActivity)}
          >
            <img src={defineActivity} alt="Define Activity" />
            Define Activity
          </Button>
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.delegateAdmin)}
          >
            <img src={delegateAdmin} alt="Delegate Admin" />
            Delegate Admin
          </Button>
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.addParticipants)}
          >
            <img src={addParticipants} alt="Add Participants" />
            Add Participants
          </Button>
          <Button
            variant="red"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.deleteHousehold)}
          >
            <img src={deleteHousehold} alt="Delete household" />
            Delete Household
          </Button>
        </div>
      </div>
      {members && members.length > 0 && (
        <div className="participants-list">
          <div className="participants-list__header">PARTICIPANTS</div>
          {members.map((member, index) => (
            <div
              className="participant"
              key={member.id}
              id={String(index)}
            >
              <span>
                <img
                  src={getProfilePictureUrl(member.profilePicturePath) || profileDefaultIcon}
                  alt={`${member.firstName} ${member.lastName}`}
                />
                <p>{`${member.firstName} ${member.lastName}`}</p>
              </span>

              <img src={decline} alt="Delete member" className="delete-member" onClick={() => { setOpenPopup(true) }} />
            </div>
          ))}
        </div>
      )}

      {openPopup && (
        <Popup
          variant={
            clickedButton === buttonActions.deleteHousehold ? "red" : "primary"
          }
          title=""
          onClose={() => setOpenPopup(false)}
        >
          {clickedButton === buttonActions.delegateAdmin && (
            <>
              <p>Selelect a new household admin</p>
              {members && members.length > 0 && (
                <div className="participants-list">
                  <div className="participants-list__header">PARTICIPANTS</div>
                  {members.map((member, index) => (
                    <div
                      className="participant__delegate-admin"
                      key={member.id}
                      id={String(index)}
                      onClick={handleDelegateAdmin}
                    >
                      <img
                        src={
                          getProfilePictureUrl(member.profilePicturePath) || profileDefaultIcon
                        }
                        alt={`${member.firstName} ${member.lastName}`}
                      />
                      <p>{`${member.firstName} ${member.lastName}`}</p>
                    </div>
                  ))}
                </div>
              )}
              <Button variant="primary" onClick={() => setOpenPopup(false)}>Delegate</Button>
            </>
          )}
          {clickedButton === buttonActions.addParticipants && (
            <>
              <p>Add Participant</p>
              <Input
                variant="primary"
                type="email"
                placeholder="Email"
                style={{ marginBottom: "var(--spacing-lg)" }}
              />
              <Button variant="primary">Send Invitations</Button>
            </>
          )}
          {clickedButton === buttonActions.deleteHousehold && (
            <>
              <p>Are you sure you want to delete this household?</p>
              <img src={profileImage} alt="Household Image" />
              <p>{householdInfo?.name || "Household"}</p>
              <Button variant="red" onClick={handleDeleteHousehold} disabled={loading}>
                {loading ? "Deleting..." : "Delete"}
              </Button>
            </>
          )}
        </Popup>
      )}
    </div>
  );
};

export default ManageHousehold;
