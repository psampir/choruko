import "./CreateHousehold.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import NavbarMobile from "../../../components/NavbarMobile";
import { useNavigate } from "react-router-dom";
import householdIcon from "../../../assets/svg/householdDefault.svg";
import arrowLeftIcon from "../../../assets/svg/arrow-left.svg";

function CreateHousehold() {
  const navigate = useNavigate();

  return (
    <div className="create-household">
      <NavbarMobile />
      <img
        className="create-household back-arrow"
        src={arrowLeftIcon}
        alt="back-arrow"
        onClick={() => navigate(-1)}
      />
      <h1>Create Household</h1>
      <img
        src={householdIcon}
        alt="Household"
        className="create-household__icon"
      />
      <Button className="create-household__select-image" variant="primary">
        Select an Image
      </Button>
      <Input variant="edit" placeholder="Household Name" />
      <Button variant="primary" className="create-btn">
        Create
      </Button>

    </div >
  );
}

export default CreateHousehold;
