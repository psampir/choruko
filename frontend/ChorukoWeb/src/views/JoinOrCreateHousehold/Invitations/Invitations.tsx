import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Invitations.scss";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import invitationService from "../../../services/invitationService";
import authService from "../../../services/authService";
import type { InvitationResponse } from "../../../services/invitationService";
import householdDefaultLight from "/src/assets/svg/light/householdDefault.svg";
import householdDefaultDark from "/src/assets/svg/dark/householdDefault.svg";
import arrowLeftLight from "/src/assets/svg/light/arrow-left.svg";
import arrowLeftDark from "/src/assets/svg/dark/arrow-left.svg";
import NavbarMobile from "../../../components/NavbarMobile";
import Popup from "../../../components/Popup";
import Button from "../../../components/Button";
import NotificationBar from "../../../components/NotificationBar";

// Display and manage household invitations (accept/decline)
function Invitations() {
  const navigate = useNavigate();
  const isDark = useIsDarkTheme();

  const householdDefault = isDark
    ? householdDefaultDark
    : householdDefaultLight;
  const arrowLeft = isDark ? arrowLeftDark : arrowLeftLight;

  const [invitations, setInvitations] = useState<InvitationResponse[]>([]);
  const [loading, setLoading] = useState(false);
  const [popup, setPopup] = useState<boolean>(false);
  const [selectedInvitation, setSelectedInvitation] =
    useState<InvitationResponse | null>(null);

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

  const loadInvitations = async () => {
    setLoading(true);
    try {
      const result = await invitationService.getAllInvitations();
      if (result.success && result.data) {
        setInvitations(result.data);
      } else {
        showNotification(
          result.message || "Failed to load invitations",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadInvitations();
  }, []);

  const openInvitationPopup = (invitation: InvitationResponse) => {
    setSelectedInvitation(invitation);
    setPopup(true);
  };

  const handleAccept = async () => {
    if (!selectedInvitation) return;

    setLoading(true);
    try {
      const result = await invitationService.acceptInvitation(
        selectedInvitation.id
      );
      if (result.success) {
        showNotification("Invitation accepted successfully", "success");
        setPopup(false);
        setSelectedInvitation(null);

        // Wait a moment for the notification to be visible
        await new Promise(resolve => setTimeout(resolve, 1000));

        // Logout the user
        await authService.logout();

        // Redirect to login
        navigate("/login");
      } else {
        showNotification(
          result.message || "Failed to accept invitation",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    setPopup(false);
    setSelectedInvitation(null);
  };

  const handleDecline = async () => {
    if (!selectedInvitation) return;

    setLoading(true);
    try {
      const result = await invitationService.declineInvitation(
        selectedInvitation.id
      );
      if (result.success) {
        showNotification("Invitation declined", "success");
        setInvitations((prev) =>
          prev.filter((inv) => inv.id !== selectedInvitation.id)
        );
        setPopup(false);
        setSelectedInvitation(null);
      } else {
        showNotification(
          result.message || "Failed to decline invitation",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="invitations">
      <NotificationBar
        message={notification.message}
        isVisible={notification.isVisible}
        type={notification.type}
      />
      <NavbarMobile />
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="back-arrow"
        onClick={() => navigate(-1)}
      />
      <h2>Invitations</h2>
      <div className="invitations-list">
        <ul>
          {loading ? (
            <li>Loading invitations...</li>
          ) : invitations.length ? (
            invitations.map((invitation) => (
              <li key={invitation.id} onClick={() => openInvitationPopup(invitation)}>
                <img
                  src={getProfilePictureUrl(invitation.householdImagePath) || householdDefault}
                  alt={invitation.householdName}
                />
                <h6>{invitation.householdName}</h6>
              </li>
            ))
          ) : (
            <li>No invitations here</li>
          )}
        </ul>
      </div>
      {popup && selectedInvitation && (
        <Popup
          variant="primary"
          title=""
          onClose={handleCancel}
        >
          <>
            <p>Do you want to join this household?</p>
            <img
              src={getProfilePictureUrl(selectedInvitation.householdImagePath) || householdDefault}
              alt="Household"
            />
            <p>{selectedInvitation.householdName}</p>
            <p>
              Invited by: {selectedInvitation.inviterFirstName}{" "}
              {selectedInvitation.inviterLastName}
            </p>
            <p style={{ textAlign: "center", marginBottom: "1rem", fontSize: "var(--font-size-body)" }}>
              You will be logged out after joining to update your access permissions.
            </p>
            <div style={{ display: "flex", gap: "var(--spacing-md)", marginTop: "var(--spacing-md)" }}>
              <Button
                variant="primary"
                onClick={handleAccept}
                disabled={loading}
              >
                {loading ? "Accepting..." : "Accept"}
              </Button>
              <Button
                variant="red"
                onClick={handleDecline}
                disabled={loading}
              >
                {loading ? "Declining..." : "Decline"}
              </Button>
            </div>
          </>
        </Popup>
      )}
    </div>
  );
}

export default Invitations;
