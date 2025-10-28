import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Invitations.scss";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import householdDefaultLight from "/src/assets/svg/light/householdDefault.svg";
import householdDefaultDark from "/src/assets/svg/dark/householdDefault.svg";
import NavbarMobile from "../../../components/NavbarMobile";
import Popup from "../../../components/Popup";
interface Invitation {
  id: number;
  name: string;
  image: string;
}

function Invitations() {
  const navigate = useNavigate();
  const isDark = useIsDarkTheme();
  const householdDefault = isDark ? householdDefaultDark : householdDefaultLight;
  // Initialize with one invitation directly instead of using useEffect
  const [invitations, setInvitations] = useState<Invitation[]>([
    {
      id: Date.now(),
      name: "New Invitation",
      image: householdDefault,
    },
  ]);

  const [popup, setPopup] = useState<boolean>(false);
  const [selectedInvitation, setSelectedInvitation] = useState<{
    id: number;
    name: string;
  } | null>(null);

  // Function to add a new invitation (kept for future use)
  // const addInvitation = () => {
  //   const invitation = {
  //     id: Date.now() + Math.random(), // Better unique ID
  //     name: "New Invitation",
  //     image: householdDefault,
  //   };
  //   setInvitations((prev) => [...prev, invitation]);
  // };

  const acceptInvitation = (invitation: { id: number; name: string }) => {
    setSelectedInvitation(invitation);
    setPopup(true);
  };

  const handleAccept = () => {
    if (selectedInvitation) {
      // Remove the invitation from the list
      setInvitations((prev) =>
        prev.filter((inv) => inv.id !== selectedInvitation.id)
      );
      // Close popup
      setPopup(false);
      setSelectedInvitation(null);
      // Here you could add logic to actually join the household
    }
  };

  const handleCancel = () => {
    setPopup(false);
    setSelectedInvitation(null);
  };

  const declineInvitation = (id: number) => {
    setInvitations((prev) => prev.filter((invitation) => invitation.id !== id));
  };

  return (
    <div className="invitations">
      <NavbarMobile />
      <img
        className="invitations back-arrow"
        src="/src/assets/svg/arrow-left.svg"
        alt="back-arrow"
        onClick={() => navigate(-1)}
      />
      <h2>Invitations</h2>
      <div className="invitations-list">
        <ul className="scrollable-content">
          {invitations.length ? (
            invitations.map((invitation) => (
              <li key={invitation.id}>
                <img src={invitation.image} alt={invitation.name} />
                <h6>{invitation.name}</h6>
                <span
                  onClick={() =>
                    acceptInvitation({
                      id: invitation.id,
                      name: invitation.name,
                    })
                  }
                  className="accept"
                ></span>
                <span
                  onClick={() => declineInvitation(invitation.id)}
                  className="decline"
                ></span>
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
          title="Confirm the invitation"
          onClose={handleCancel}
          actions={[
            {
              label: "Join",
              onClick: handleAccept,
              variant: "primary",
            },
          ]}
        >
          <div className="invitation-confirmation">
            <div className="household-image-container">
              <img
                src={householdDefault}
                alt="Household"
                className="household-image"
              />
            </div>
            <h3 className="household-name">Home1</h3>
          </div>
        </Popup>
      )}
    </div>
  );
}

export default Invitations;
