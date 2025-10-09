import Button from "../../components/Button";
import RaccoonLogo from "../../components/RaccoonLogo";
import { useUser } from "../../contexts/UserContext";
import "./JoinOrCreateHousehold.scss";

interface LoginProps {
  onJoinHousehold: () => void;
  onCreateHousehold: () => void;
}

const JoinOrCreateHousehold: React.FC<LoginProps> = ({
  onJoinHousehold,
  onCreateHousehold,
}) => {
  const { user } = useUser();

  return (
    <div className="join-or-create-household">
      <RaccoonLogo />
      {user && <h1 className="join-or-create-household__title">Welcome, {user?.firstName}!</h1>}

      <div className="join-or-create-household__buttons">
        <Button variant="primary" onClick={onJoinHousehold}>
          Join Household
        </Button>
        <Button variant="primary" onClick={onCreateHousehold}>
          Create Household
        </Button>
      </div>
    </div>
  );
};

export default JoinOrCreateHousehold;
