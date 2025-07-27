import { useState, useEffect } from "react";
import "./Invitations.scss";
import householdDefault from "/src/assets/svg/householdDefault.svg";

function Invitations() {
  const [invitations, setInvitations] = useState<any[]>([]);

  // Function outside return that adds a new invitation
  const newInvitation = () => {
    const invitation = {
      id: Date.now(), // Simple ID for now
      name: "New Invitation",
      image: householdDefault,
    };
    setInvitations((prev) => [...prev, invitation]);
  };

  // Add one invitation when component mounts
  useEffect(() => {
    newInvitation();
  }, []);

  return (
    <div className="invitations">
      <h2>Invitations</h2>
      <ul className="invitations-list">
        {invitations.map((invitation) => (
          <li key={invitation.id}>
            <img src={invitation.image} alt={invitation.name} />
            <h6>{invitation.name}</h6>
            <span className="accept"></span>
            <span className="decline"></span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default Invitations;
