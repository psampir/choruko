import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./ManageHousehold.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";

import householdDefaulIcon from "../../../assets/svg/householdDefault.svg";
import arrowLeft from "../../../assets/svg/arrow-left.svg";
import defineActivity from "../../../assets/svg/activities.svg";
import delegateAdmin from "../../../assets/svg/star-emty.svg";
import addParticipants from "../../../assets/svg/profile-icon-light.svg";
import deleteHousehold from "../../../assets/svg/trash-bin.svg";
import decline from "../../../assets/svg/decline.svg"

const buttonActions = {
  defineActivity: "define_activity",
  delegateAdmin: "delegate_admin",
  addParticipants: "add_participants",
  deleteHousehold: "delete_household",
  deleteParticipant: "delete_participant",
} as const;

type ButtonAction = (typeof buttonActions)[keyof typeof buttonActions];

const mockParticipants = [
  {
    id: 1,
    name: "Andreas Toretto",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 2,
    name: "Ann Huang",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 3,
    name: "Gabbie Mayers",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 4,
    name: "John Smith",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 5,
    name: "Sarah Wilson",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 6,
    name: "Mike Johnson",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 7,
    name: "Lisa Brown",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
  {
    id: 8,
    name: "David Lee",
    avatar: "/src/assets/svg/profile-icon.svg",
  },
];

const ManageHousehold: React.FC = () => {
  const navigate = useNavigate();
  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [nameValue, setNameValue] = useState<string>("Poznańska");
  const [profileImage, setProfileImage] = useState<string>(householdDefaulIcon);
  const [clickedButton, setClickedButton] = useState<ButtonAction | null>(null);

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

  const handleButtonClick = (action: ButtonAction) => {
    setClickedButton(action);
    if (action !== "define_activity") {
      setOpenPopup(true);
    } else {
      navigate("/activities");
    }
    console.log(
      "Button clicked:",
      action,
      "Current clicked button state:",
      clickedButton
    );
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

  // const handleParticipantClick = (
  //   e: React.MouseEvent<HTMLDivElement>,
  //   participant: (typeof mockParticipants)[0]
  // ) => {
  //   setOpenPopup(true)
  // };

  return (
    <div className="manage-household">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate("/leaderboard")}
      />
      <div
        className={`notification-bar ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-household__title">Manage Household</h1>
      <div className="household-header">
        <img className="household-image" src={profileImage} alt="Household" />
        <Button variant="primary" onClick={triggerFileInput}>
          Change Picture
        </Button>
        <input
          id="profile-image-input"
          type="file"
          accept="image/*"
          onChange={handleImageChange}
          style={{ display: "none" }}
        />
      </div>
      <div className="manage-household__main-section">
        <div className="manage-household__name-section">
          <Input
            variant="edit"
            value={nameValue}
            onChange={(e) => setNameValue(e.target.value)}
          />
          <Button variant="primary" onClick={handleNameChange}>
            Change Name
          </Button>
        </div>
        <div className="manage-household__buttons-grid">
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.defineActivity)}
          >
            <img src={defineActivity} alt="Define Activity" />
            Define Activity
          </Button>
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.delegateAdmin)}
          >
            <img src={delegateAdmin} alt="Delegate Admin" />
            Delegate Admin
          </Button>
          <Button
            variant="primary"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.addParticipants)}
          >
            <img src={addParticipants} alt="Add Participants" />
            Add Participants
          </Button>
          <Button
            variant="red"
            className="household-action-btn"
            onClick={() => handleButtonClick(buttonActions.deleteHousehold)}
          >
            <img src={deleteHousehold} alt="Delete household" />
            Delete Household
          </Button>
        </div>
      </div>
      {mockParticipants && (
        <div className="participants-list">
          <div className="participants-list__header">PARTICIPANTS</div>
          {mockParticipants.map((participant, index) => (
            <div
              className="participant"
              key={participant.id}
              id={String(index)}
              // onClick={(e) => handleParticipantClick(e, participant)}
            >
              <span>
                <img
                  src={participant.avatar ? participant.avatar : profileImage}
                  alt="Profile icon"
                />
                <p>{participant.name}</p>
              </span>

              <img src={decline} alt="Delete member" className="delete-member" onClick={()=>{setOpenPopup(true)}}/>
            </div>
          ))}
        </div>
      )}

      {openPopup && (
        <Popup
          variant={
            clickedButton === buttonActions.deleteHousehold ? "red" : "primary"
          }
          title=""
          onClose={() => setOpenPopup(false)}
        >
          {clickedButton === buttonActions.delegateAdmin && (
            <>
              <p>Selelect a new household admin</p>
              {mockParticipants && (
                <div className="participants-list">
                  <div className="participants-list__header">PARTICIPANTS</div>
                  {mockParticipants.map((participant, index) => (
                    <div
                      className="participant__delegate-admin"
                      key={participant.id}
                      id={String(index)}
                      onClick={handleDelegateAdmin}
                    >
                      <img
                        src={
                          participant.avatar ? participant.avatar : profileImage
                        }
                        alt=""
                      />
                      <p>{participant.name}</p>
                    </div>
                  ))}
                </div>
              )}
              <Button variant="primary" onClick={()=>setOpenPopup(false)}>Delegate</Button>
            </>
          )}
          {clickedButton === buttonActions.addParticipants && (
            <>
              <p>Add Participant</p>
              <Input
                variant="primary"
                type="email"
                placeholder="Email"
                style={{ marginBottom: "var(--spacing-lg)" }}
              />
              <Button variant="primary">Send Invitations</Button>
            </>
          )}
          {clickedButton === buttonActions.deleteHousehold && (
            <>
              <p>Are you sure you want to delete this household?</p>
              <img src={profileImage} alt="Household Image" />
              <p>Household Name</p>
              <Button variant="red">Delete</Button>
            </>
          )}
        </Popup>
      )}
    </div>
  );
};

export default ManageHousehold;
