import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Activities.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import Popup from "../../../../components/Popup";

import householdDefaulIcon from "../../../../assets/svg/householdDefault.svg";
import arrowLeft from "../../../../assets/svg/arrow-left.svg";
import editBox from "../../../../assets/svg/edit-box.svg"



const mockParticipants = [
  {
    id: 1,
    info: "Vacuming the floor",
    points: "62"
  },
  {
    id: 1,
    info: "Clean dishes",
    points: "62"
  },
  {
    id: 1,
    info: "Make a breackfast",
    points: "62"
  },
  {
    id: 1,
    info: "Throw out the trash",
    points: "62"
  },
  {
    id: 1,
    info: "Clean the room",
    points: "62"
  },
  {
    id: 1,
    info: "Wash clothes",
    points: "62"
  },
  {
    id: 1,
    info: "Mow the lawn",
    points: "62"
  },
];

const Activities: React.FC = () => {
  const navigate = useNavigate();
  const [openPopup, setOpenPopup] = useState<Boolean>(false);
  const [nameValue, setNameValue] = useState<string>("Poznańska");
  const [profileImage, setProfileImage] = useState<string>(householdDefaulIcon);
  const [clickedButton, setClickedButton] = useState()

  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
  }>({
    message: "",
    isVisible: false,
  });

  const showNotification = (message: string) => {
    setNotification({ message, isVisible: true });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false });
    }, 3000);
  };

  const handleNameChange = () => {
    if (nameValue.trim()) {
      showNotification("Your name has been changed");
    }
  };

  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        setProfileImage(result);
        showNotification("Profile picture updated");
      };
      reader.readAsDataURL(file);
    }
  };

  const triggerFileInput = () => {
    document.getElementById("profile-image-input")?.click();
  };


  const handleDelegateAdmin = (event: React.MouseEvent<HTMLDivElement>) => {
    const participantsListInPopup =
      event.currentTarget.closest(".participants-list");

    participantsListInPopup
      ?.querySelectorAll(".participant__delegate-admin")
      .forEach((element) => {
        element.classList.remove("admin-choose");
      });
    event.currentTarget.classList.add("admin-choose");
  };

  return (
    <div className="activities">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate("/manageHousehold")}
      />
      <div
        className={`notification-bar ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-household__title">Activities</h1>

      <div className="manage-household__main-section">
        
      </div>
      {mockParticipants && (
        <div className="participants-list">
          <div className="participants-list__header">PARTICIPANTS</div>
          {mockParticipants.map((participant, index) => (
            <div
              className="participant"
              key={participant.id}
              id={String(index)}
            >
              <p>{participant.info}</p>
              <img src={editBox} alt="Edit Box" onClick={()=>{setOpenPopup(true)}}/>
            </div>
          ))}
        </div>
      )}

      {openPopup && (
        <Popup
          variant="primary"
          title="Edit Activity"
          onClose={() => setOpenPopup(false)}
        >
        </Popup>
      )}
    </div>
  );
};

export default Activities;
