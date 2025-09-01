import { useState } from "react";
import "./Invitations.scss";
import householdDefault from "/src/assets/svg/householdDefault.svg";
import NavbarMobile from "../../../components/NavbarMobile";
import Popup from "../../../components/Popup";
interface Invitation {
  id: number;
  name: string;
  image: string;
}

function Invitations() {
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
  const addInvitation = () => {
    const invitation = {
      id: Date.now() + Math.random(), // Better unique ID
      name: "New Invitation",
      image: householdDefault,
    };
    setInvitations((prev) => [...prev, invitation]);
  };

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
      />
      <h2>Invitations</h2>
      <ul className="invitations-list">
        {invitations.length ? (
          invitations.map((invitation) => (
            <li key={invitation.id}>
              <img src={invitation.image} alt={invitation.name} />
              <h6>{invitation.name}</h6>
              <span
                onClick={() =>
                  acceptInvitation({ id: invitation.id, name: invitation.name })
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
      {popup && selectedInvitation && (
        <Popup
          variant="primary"
          title="Accept Invitation"
          onClose={handleCancel}
          actions={[
            {
              label: "Accept Invitation",
              onClick: handleAccept,
              variant: "primary",
            },
            {
              label: "Cancel",
              onClick: handleCancel,
              variant: "secondary",
            },
          ]}
        >
          <p>
            Do you want to accept the invitation from{" "}
            <strong>{selectedInvitation.name}</strong>?
          </p>
          <p>
            Accepting this invitation will add you to the household where you'll
            be able to participate in tasks, expenses, and activities.
          </p>
        </Popup>
      )}
    </div>
  );
}

export default Invitations;
