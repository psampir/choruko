import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { useAuth } from "../../../contexts/AuthContext";
import { useUser } from "../../../contexts/UserContext";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import { POPULAR_CURRENCIES } from "../../../utils/currencies";
import householdService from "../../../services/householdService";
import invitationService from "../../../services/invitationService";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import type { HouseholdParticipant } from "../../../services/householdService";
import "./ManageHousehold.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";
import NotificationBar from "../../../components/NotificationBar";

import householdDefaultIconLight from "../../../assets/svg/light/householdDefault.svg";
import profileDefaultIconLight from "../../../assets/svg/light/profile-icon.svg";
import arrowLeftLight from "../../../assets/svg/light/arrow-left.svg";
import defineActivityLight from "../../../assets/svg/light/activities.svg";
import delegateAdminLight from "../../../assets/svg/light/star-emty.svg";
import addParticipantsLight from "../../../assets/svg/light/profile-icon-light.svg";
import deleteHouseholdLight from "../../../assets/svg/light/trash-bin.svg";
import declineLight from "../../../assets/svg/light/decline.svg";
import checkIconLight from "../../../assets/svg/light/check.svg";

import householdDefaultIconDark from "../../../assets/svg/dark/householdDefault.svg";
import profileDefaultIconDark from "../../../assets/svg/dark/profile-icon.svg";
import arrowLeftDark from "../../../assets/svg/dark/arrow-left.svg";
import defineActivityDark from "../../../assets/svg/dark/activities.svg";
import delegateAdminDark from "../../../assets/svg/dark/star-emty.svg";
import addParticipantsDark from "../../../assets/svg/dark/profile-icon-light.svg";
import deleteHouseholdDark from "../../../assets/svg/dark/trash-bin.svg";
import declineDark from "../../../assets/svg/dark/decline.svg";
import checkIconDark from "../../../assets/svg/dark/check.svg";

const buttonActions = {
  defineActivity: "define_activity",
  delegateAdmin: "delegate_admin",
  addParticipants: "add_participants",
  deleteHousehold: "delete_household",
  deleteParticipant: "delete_participant",
} as const;

type ButtonAction = (typeof buttonActions)[keyof typeof buttonActions];

// Household management - edit name, manage participants, delegate admin, delete household
const ManageHousehold: React.FC = () => {
  const navigate = useNavigate();
  const {
    householdInfo,
    participants,
    refreshHousehold,
    error: householdError,
    loading: householdLoading,
  } = useHousehold();
  const { hasHousehold, checkAuth } = useAuth();
  const { user } = useUser();
  const isDark = useIsDarkTheme();

  const householdDefaultIcon = isDark
    ? householdDefaultIconDark
    : householdDefaultIconLight;
  const profileDefaultIcon = isDark
    ? profileDefaultIconDark
    : profileDefaultIconLight;
  const arrowLeft = isDark ? arrowLeftDark : arrowLeftLight;
  const defineActivity = isDark ? defineActivityDark : defineActivityLight;
  const delegateAdmin = isDark ? delegateAdminDark : delegateAdminLight;
  const addParticipants = isDark ? addParticipantsDark : addParticipantsLight;
  const deleteHousehold = isDark ? deleteHouseholdDark : deleteHouseholdLight;
  const decline = isDark ? declineDark : declineLight;
  const checkIcon = isDark ? checkIconDark : checkIconLight;

  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [nameValue, setNameValue] = useState<string>("");
  const [selectedCurrency, setSelectedCurrency] = useState<string>("USD");
  const [currencyCollapsed, setCurrencyCollapsed] = useState(false);
  const [profileImage, setProfileImage] =
    useState<string>(householdDefaultIcon);
  const [clickedButton, setClickedButton] = useState<ButtonAction | null>(null);
  const [selectedParticipant, setSelectedParticipant] =
    useState<HouseholdParticipant | null>(null);
  const [selectedNewAdmin, setSelectedNewAdmin] =
    useState<HouseholdParticipant | null>(null);
  const [invitationEmail, setInvitationEmail] = useState<string>("");
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

  // Redirect if user loses household access
  useEffect(() => {
    const checkHouseholdAccess = async () => {
      if (
        householdError &&
        householdError.includes("not part of any household")
      ) {
        await checkAuth(false);
        navigate("/joinOrCreateHousehold", { replace: true });
      } else if (!householdLoading && !hasHousehold) {
        navigate("/joinOrCreateHousehold", { replace: true });
      }
    };

    checkHouseholdAccess();
  }, [householdError, hasHousehold, householdLoading, checkAuth, navigate]);

  // Initialize household name and image from context
  useEffect(() => {
    if (householdInfo) {
      setNameValue(householdInfo.name);
      setProfileImage(householdInfo.profilePictureUrl || householdDefaultIcon);
      setSelectedCurrency(householdInfo.isoCurrencySymbol || "USD");
    }
  }, [householdInfo, householdDefaultIcon]);

  const showNotification = (
    message: string,
    type: "success" | "error" = "success"
  ) => {
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
        showNotification(
          result.message || "Failed to change household name",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleCurrencyChange = async () => {
    setLoading(true);
    try {
      const result = await householdService.changeHouseholdCurrency(selectedCurrency);
      if (result.success) {
        showNotification("Household currency changed successfully", "success");
        await refreshHousehold();
      } else {
        showNotification(
          result.message || "Failed to change household currency",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleImageChange = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
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
          showNotification(
            result.message || "Failed to change household picture",
            "error"
          );
          // Revert preview on failure
          setProfileImage(
            householdInfo?.profilePictureUrl || householdDefaultIcon
          );
        }
      } catch (error) {
        showNotification("An unexpected error occurred", "error");
        setProfileImage(
          householdInfo?.profilePictureUrl || householdDefaultIcon
        );
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
      // Check if delegate admin but no other participants
      if (action === "delegate_admin") {
        const delegateableParticipants = participants.filter(
          (participant) => participant.id !== user?.id
        );
        if (delegateableParticipants.length === 0) {
          showNotification("No other participants available to delegate admin role", "error");
          return;
        }
        setSelectedNewAdmin(null);
      }
      setOpenPopup(true);
    } else {
      navigate("/activities");
    }
  };

  const handleSelectNewAdmin = (
    participant: HouseholdParticipant,
    event: React.MouseEvent<HTMLDivElement>
  ) => {
    const participantsListInPopup =
      event.currentTarget.closest(".participants-list");

    participantsListInPopup
      ?.querySelectorAll(".participant__delegate-admin")
      .forEach((element) => {
        element.classList.remove("admin-choose");
      });
    event.currentTarget.classList.add("admin-choose");
    setSelectedNewAdmin(participant);
  };

  const handleDelegateAdmin = async () => {
    if (!selectedNewAdmin) {
      showNotification("Please select a new admin", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await householdService.setHouseholdAdmin(
        selectedNewAdmin.id
      );
      if (result.success) {
        showNotification("Admin delegated successfully", "success");
        setOpenPopup(false);
        setSelectedNewAdmin(null);
        // Refresh auth state since current user is no longer admin
        await checkAuth(false);
        // Redirect to profile or home after delegation
        setTimeout(() => {
          navigate("/profile");
        }, 1500);
      } else {
        showNotification(result.message || "Failed to delegate admin", "error");
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteHousehold = async () => {
    setLoading(true);
    try {
      const result = await householdService.deleteHousehold();
      if (result.success) {
        showNotification("Household deleted successfully", "success");
        setOpenPopup(false);
        // Redirect to joinOrCreateHousehold after deletion
        setTimeout(() => {
          navigate("/joinOrCreateHousehold");
        }, 1500);
      } else {
        showNotification(
          result.message || "Failed to delete household",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteParticipant = async () => {
    if (!selectedParticipant) return;

    setLoading(true);
    try {
      const result = await householdService.removeUserFromHousehold(
        selectedParticipant.id
      );
      if (result.success) {
        showNotification("Participant removed successfully", "success");
        setOpenPopup(false);
        setSelectedParticipant(null);
        await refreshHousehold();
      } else {
        showNotification(
          result.message || "Failed to remove participant",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteParticipantClick = (participant: HouseholdParticipant) => {
    setSelectedParticipant(participant);
    setClickedButton(buttonActions.deleteParticipant);
    setOpenPopup(true);
  };

  const handleSendInvitation = async () => {
    if (!invitationEmail.trim()) {
      showNotification("Please enter an email address", "error");
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(invitationEmail.trim())) {
      showNotification("Please enter a valid email address", "error");
      return;
    }

    if (!householdInfo?.id) {
      showNotification("Household information not available", "error");
      return;
    }

    setLoading(true);
    try {
      const result = await invitationService.createInvitation(
        householdInfo.id,
        invitationEmail.trim()
      );
      if (result.success) {
        showNotification("Invitation sent successfully", "success");
        setInvitationEmail("");
        setOpenPopup(false);
      } else {
        showNotification(
          result.message || "Failed to send invitation",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  // Sort participants to show admin first
  const sortedParticipants = [...participants].sort((a, b) => {
    if (a.id === householdInfo?.adminId) return -1;
    if (b.id === householdInfo?.adminId) return 1;
    return 0;
  });

  // Filter out current admin for delegation popup
  const delegateableParticipants = participants.filter(
    (participant) => participant.id !== user?.id
  );

  return (
    <div className="manage-household">
      <NotificationBar
        message={notification.message}
        isVisible={notification.isVisible}
        type={notification.type}
      />
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate(-1)}
      />

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
          <Button
            variant="primary"
            onClick={handleNameChange}
            disabled={loading}
          >
            {loading ? "Saving..." : "Change Name"}
          </Button>
        </div>

        {/* Currency Selection */}
        <div className="manage-household__currency-section">
          <div
            className={`participants-list ${currencyCollapsed ? "participants-list--collapsed" : ""
              }`}
          >
            <div
              className="participants-list__header"
              onClick={() => setCurrencyCollapsed(!currencyCollapsed)}
            >
              SELECT CURRENCY
              <span className="dropdown__arrow">&#9660;</span>
            </div>
            {!currencyCollapsed && (
              <div className="participants-list__content">
                {POPULAR_CURRENCIES.map((currency) => (
                  <div
                    className={`activity ${selectedCurrency === currency.code ? "activity--selected" : ""
                      }`}
                    key={currency.code}
                    onClick={() => setSelectedCurrency(currency.code)}
                  >
                    <div className="activity-data">
                      <span className="currency-code">{currency.code}</span>
                      <p>{currency.name}</p>
                    </div>
                    {selectedCurrency === currency.code && (
                      <img
                        src={checkIcon}
                        alt="Selected"
                        className="check-icon"
                      />
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
          <Button
            variant="primary"
            onClick={handleCurrencyChange}
            disabled={loading}
          >
            {loading ? "Saving..." : "Change Currency"}
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
      {participants && participants.length > 0 && (
        <div className="participants-list">
          <div className="participants-list__header">PARTICIPANTS</div>
          {sortedParticipants.map((participant, index) => (
            <div
              className="participant"
              key={participant.id}
              id={String(index)}
            >
              <span>
                <img
                  src={
                    getProfilePictureUrl(participant.profilePicturePath) ||
                    profileDefaultIcon
                  }
                  alt={`${participant.firstName} ${participant.lastName}`}
                />
                <p>{`${participant.firstName} ${participant.lastName}`}</p>
              </span>

              {participant.id !== user?.id && (
                <img
                  src={decline}
                  alt="Delete participant"
                  className="delete-participant"
                  onClick={() => handleDeleteParticipantClick(participant)}
                />
              )}
            </div>
          ))}
        </div>
      )}

      {openPopup && (
        <Popup
          variant={
            clickedButton === buttonActions.deleteHousehold ||
              clickedButton === buttonActions.deleteParticipant
              ? "red"
              : "primary"
          }
          title=""
          onClose={() => setOpenPopup(false)}
        >
          {clickedButton === buttonActions.delegateAdmin && (
            <>
              <p>Select a new household admin</p>
              <p className="warning-text">
                To apply your new permissions, you will need to log in again.
              </p>
              <div className="participants-list">
                <div className="participants-list__header">PARTICIPANTS</div>
                {delegateableParticipants.map((participant, index) => (
                  <div
                    className="participant__delegate-admin"
                    key={participant.id}
                    id={String(index)}
                    onClick={(e) => handleSelectNewAdmin(participant, e)}
                  >
                    <img
                      src={
                        getProfilePictureUrl(
                          participant.profilePicturePath
                        ) || profileDefaultIcon
                      }
                      alt={`${participant.firstName} ${participant.lastName}`}
                    />
                    <p>{`${participant.firstName} ${participant.lastName}`}</p>
                  </div>
                ))}
              </div>
              <Button
                variant="primary"
                onClick={handleDelegateAdmin}
                disabled={loading || !selectedNewAdmin}
              >
                {loading ? "Delegating..." : "Delegate"}
              </Button>
            </>
          )}
          {clickedButton === buttonActions.addParticipants && (
            <>
              <p>Add Participant</p>
              <Input
                variant="primary"
                type="email"
                placeholder="Email"
                value={invitationEmail}
                onChange={(e) => setInvitationEmail(e.target.value)}
                style={{ marginBottom: "var(--spacing-lg)" }}
              />
              <Button
                variant="primary"
                onClick={handleSendInvitation}
                disabled={loading}
              >
                {loading ? "Sending..." : "Send Invitation"}
              </Button>
            </>
          )}
          {clickedButton === buttonActions.deleteHousehold && (
            <>
              <p>Are you sure you want to delete this household?</p>
              <img src={profileImage} alt="Household Image" />
              <p>{householdInfo?.name || "Household"}</p>
              <Button
                variant="red"
                onClick={handleDeleteHousehold}
                disabled={loading}
              >
                {loading ? "Deleting..." : "Delete"}
              </Button>
            </>
          )}
          {clickedButton === buttonActions.deleteParticipant &&
            selectedParticipant && (
              <>
                <p>Are you sure you want to remove this participant?</p>
                <img
                  src={
                    getProfilePictureUrl(
                      selectedParticipant.profilePicturePath
                    ) || profileDefaultIcon
                  }
                  alt={`${selectedParticipant.firstName} ${selectedParticipant.lastName}`}
                />
                <p>{`${selectedParticipant.firstName} ${selectedParticipant.lastName}`}</p>
                <Button
                  variant="red"
                  onClick={handleDeleteParticipant}
                  disabled={loading}
                >
                  {loading ? "Removing..." : "Remove"}
                </Button>
              </>
            )}
        </Popup>
      )}
    </div>
  );
};

export default ManageHousehold;
