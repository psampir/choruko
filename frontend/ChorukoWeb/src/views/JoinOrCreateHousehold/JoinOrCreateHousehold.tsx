import Button from "../../components/Button";
import RaccoonLogo from "../../components/RaccoonLogo";
import { useUser } from "../../contexts/UserContext";
import { useNavigate } from "react-router-dom";
import "./JoinOrCreateHousehold.scss";

// Initial household setup - choose between joining or creating
const JoinOrCreateHousehold: React.FC = () => {
  const { user } = useUser();
  const navigate = useNavigate();

  return (
    <div className="join-or-create-household">
      <RaccoonLogo />
      {user && (
        <h1 className="join-or-create-household__title">
          Welcome, {user?.firstName}!
        </h1>
      )}

      <div className="join-or-create-household__buttons">
        <Button variant="primary" onClick={() => navigate("/invitations")}>
          Join Household
        </Button>
        <Button variant="primary" onClick={() => navigate("/createHousehold")}>
          Create Household
        </Button>
      </div>
    </div>
  );
};

export default JoinOrCreateHousehold;
