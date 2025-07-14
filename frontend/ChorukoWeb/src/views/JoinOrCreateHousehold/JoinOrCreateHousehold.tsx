// import './JoinOrCreateHousehold.scss';
import Button from '../../components/Button';
import RaccoonLogo from '../../components/RaccoonLogo';
import './JoinOrCreateHousehold.scss';

interface LoginProps {
  onJoinHousehold: () => void;
  onCreateHousehold: () => void;
}

const JoinOrCreateHousehold: React.FC<LoginProps> = ({ onJoinHousehold, onCreateHousehold }) => {
  return (
    <div className="join-or-create-household">  
      <RaccoonLogo />
      <div className="join-or-create-household__buttons">
        <Button variant="primary" onClick={onJoinHousehold}>Join Household</Button>
        <Button variant="primary" onClick={onCreateHousehold}>Create Household</Button>
      </div>
    </div>
  );
};

export default JoinOrCreateHousehold;
