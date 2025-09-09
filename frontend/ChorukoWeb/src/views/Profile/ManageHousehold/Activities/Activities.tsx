import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Activities.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import Popup from "../../../../components/Popup";

import householdDefaulIcon from "../../../../assets/svg/householdDefault.svg";
import arrowLeft from "../../../../assets/svg/arrow-left.svg";
import editBox from "../../../../assets/svg/edit-box.svg";
import star from "../../../../assets/svg/star.svg";

type Activity = {
  id: number;
  info: string;
  points: string;
};

const Activities: React.FC = () => {
  const navigate = useNavigate();
  const [activities, setActivities] = useState<Activity[]>([
    {
      id: 1,
      info: "Vacuming the floor",
      points: "62",
    },
    {
      id: 2,
      info: "Clean dishes",
      points: "45",
    },
    {
      id: 3,
      info: "Make a breakfast",
      points: "30",
    },
    {
      id: 4,
      info: "Throw out the trash",
      points: "15",
    },
    {
      id: 5,
      info: "Clean the room",
      points: "50",
    },
    {
      id: 6,
      info: "Wash clothes",
      points: "35",
    },
    {
      id: 7,
      info: "Mow the lawn",
      points: "80",
    },
  ]);

  const [openPopup, setOpenPopup] = useState<Boolean>(false);
  const [nameValue, setNameValue] = useState<string>("Poznańska");
  const [profileImage, setProfileImage] = useState<string>(householdDefaulIcon);
  const [clickedButton, setClickedButton] = useState();
  const [editActivityName, setEditActivityName] = useState("");
  const [editActivityPoints, setEditActivityPoints] = useState("");
  const [editingActivityId, setEditingActivityId] = useState<number | null>(
    null
  );

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

  const handleEditClick = (activity: Activity) => {
    setEditActivityName(activity.info);
    setEditActivityPoints(activity.points);
    setEditingActivityId(activity.id);
    setOpenPopup(true);
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

  const saveActivityData = () => {
    if (editingActivityId !== null) {
      // Update the specific activity in the activities array
      setActivities((prevActivities) =>
        prevActivities.map((activity) =>
          activity.id === editingActivityId
            ? {
                ...activity,
                info: editActivityName,
                points: editActivityPoints,
              }
            : activity
        )
      );

      showNotification("Activity updated successfully!");
      setOpenPopup(false);
      setEditingActivityId(null);
      setEditActivityName("");
      setEditActivityPoints("");
    }
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

      <div className="manage-household__main-section"></div>
      {activities && (
        <div className="participants-list">
          <div className="participants-list__header">ACTIVITIES</div>
          {activities.map((activity) => (
            <div
              className="activity"
              key={activity.id}
              id={String(activity.id)}
            >
              <div className="activity-data">
                <span className="points"><img src={star} alt=""/>{activity.points}</span>
                <p>{activity.info}</p>
              </div>
              <img
                src={editBox}
                alt="Edit Box"
                onClick={() => {
                  handleEditClick(activity);
                }}
              />
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
          <Input
            variant="edit"
            value={editActivityName}
            onChange={(e) => setEditActivityName(e.target.value)}
            placeholder="Activity name"
          />
          <Input
            variant="edit"
            value={editActivityPoints}
            onChange={(e) => setEditActivityPoints(e.target.value)}
            placeholder="Points"
          />
          <div className="button__edit-activity">
            <Button variant="red">Delete</Button>
            <Button variant="primary" onClick={saveActivityData}>
              Save
            </Button>
          </div>
        </Popup>
      )}
    </div>
  );
};

export default Activities;
