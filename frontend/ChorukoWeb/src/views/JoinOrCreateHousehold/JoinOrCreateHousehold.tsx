// import './JoinOrCreateHousehold.scss';
import Button from "../../components/Button";
import RaccoonLogo from "../../components/RaccoonLogo";
import "./JoinOrCreateHousehold.scss";
import { useState } from "react";

interface LoginProps {
  onJoinHousehold: () => void;
  onCreateHousehold: () => void;
}

type JoinOrCreateHouseholdView =
  | "previewOptions"
  | "joinHousehold"
  | "createHousehold";

const JoinOrCreateHousehold: React.FC<LoginProps> = ({
  onJoinHousehold,
  onCreateHousehold,
}) => {
  const [currentView, setCurrentView] =
    useState<JoinOrCreateHouseholdView>("previewOptions");

  const showPreviewOptions = () => setCurrentView("previewOptions");
  const showJoinHousehold = () => setCurrentView("joinHousehold");
  const showCreateHousehold = () => setCurrentView("createHousehold");

  return (
    <div className="join-or-create-household">
      {currentView === "previewOptions" && (
        <>
          <RaccoonLogo />
          <h1 className="join-or-create-household__title">Choruko</h1>
          <div className="join-or-create-household__buttons">
            <Button variant="primary" onClick={onJoinHousehold}>
              Join Household
            </Button>
            <Button variant="primary" onClick={onCreateHousehold}>
              Create Household
            </Button>
          </div>
        </>
      )}
      {currentView === "joinHousehold" && (
        <div>
          <h2>Join an Existing Household</h2>
          {/* Join Household Form */}
        </div>
      )}
      {currentView === "createHousehold" && (
        <div>
          <h2>Create a New Household</h2>
          {/* Create Household Form */}
        </div>
      )}
    </div>
  );
};

export default JoinOrCreateHousehold;
