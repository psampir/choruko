import { useState, useEffect } from "react";
import "./Invitations.scss";
import householdDefault from "/src/assets/svg/householdDefault.svg";
import NavbarMobile from "../../../components/NavbarMobile";
interface Invitation {
  id: number;
  name: string;
  image: string;
}

function Invitations() {
  const [invitations, setInvitations] = useState<Invitation[]>([]);

  // Function outside return that adds a new invitation
  const newInvitation = () => {
    const invitation = {
      id: Date.now(), // Simple ID for now
      name: "New Invitation",
      image: householdDefault,
    };
    setInvitations((prev) => [...prev, invitation]);
  };

  const declineInvitation = (id: number) => {
    setInvitations((prev) => prev.filter((invitation) => invitation.id !== id));
  };

  // Add one invitation when component mounts
  useEffect(() => {
    newInvitation();
  }, []);

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
        {invitations.map((invitation) => (
          <li key={invitation.id}>
            <img src={invitation.image} alt={invitation.name} />
            <h6>{invitation.name}</h6>
            <span className="accept"></span>
            <span
              onClick={() => declineInvitation(invitation.id)}
              className="decline"
            ></span>
          </li>
        ))}
      </ul>
    </div>
  );
}

export default Invitations;
