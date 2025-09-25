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
      info: "Vacuuming the floor",
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

  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [nameValue, setNameValue] = useState<string>("Poznańska");
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

  const deleteActivityData = () => {
    if (editingActivityId !== null) {
      // Remove the activity from the activities array
      setActivities((prevActivities) =>
        prevActivities.filter((activity) => activity.id !== editingActivityId)
      );

      showNotification("Activity deleted successfully!");
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
                <span className="points">
                  <img src={star} alt="" />
                  {activity.points}
                </span>
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
            <Button variant="red" onClick={deleteActivityData}>
              Delete
            </Button>
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
