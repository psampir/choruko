import "./CreateHousehold.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import NavbarMobile from "../../../components/NavbarMobile";

function CreateHousehold() {
  return (
    <div className="create-household">
      <NavbarMobile />
      <img
        className="create-household back-arrow"
        src="/src/assets/svg/arrow-left.svg"
        alt="back-arrow"
      />
      <h1>Create Household</h1>
      <img
        src="/src/assets/svg/householdDefault.svg"
        alt="Household"
        className="create-household__icon"
      />
      <Button className="create-household__select-image" variant="primary">
        Select an Image
      </Button>
      <Input variant="edit" placeholder="Household Name" />
      <Button variant="primary">Change Name</Button>
      <div className="create-household__actions">
        <Button variant="primary">
          <img
            src="/src/assets/svg/activities.svg"
            alt="Define Activities"
            className="activity-icon"
          />
          <p>Define Activities</p>
        </Button>
        <Button variant="primary">
          <img
            src="/src/assets/svg/profile-light.svg"
            alt="Add Participants"
            className="profile-icon"
          />
          <p>Add Participants</p>
        </Button>
      </div>
      <Button variant="primary">Create</Button>
    </div>
  );
}
export default CreateHousehold;
