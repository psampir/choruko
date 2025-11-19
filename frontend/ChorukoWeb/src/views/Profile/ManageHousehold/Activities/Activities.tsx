import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Activities.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import Popup from "../../../../components/Popup";
import { useActivity } from "../../../../contexts/ActivityContext";
import { useIsDarkTheme } from "../../../../utils/iconUtils";

import arrowLeftLight from "../../../../assets/svg/light/arrow-left.svg";
import editBoxLight from "../../../../assets/svg/light/edit-box.svg";
import starLight from "../../../../assets/svg/light/star.svg";

import arrowLeftDark from "../../../../assets/svg/dark/arrow-left.svg";
import editBoxDark from "../../../../assets/svg/dark/edit-box.svg";
import starDark from "../../../../assets/svg/dark/star.svg";

// Manage household activities - create, edit, delete activities with point values
const Activities: React.FC = () => {
  const navigate = useNavigate();
  const {
    activities,
    loading,
    error,
    refreshActivities,
    createActivity,
    editActivity,
    deleteActivity,
    clearError,
  } = useActivity();
  const isDark = useIsDarkTheme();

  const arrowLeft = isDark ? arrowLeftDark : arrowLeftLight;
  const editBox = isDark ? editBoxDark : editBoxLight;
  const star = isDark ? starDark : starLight;

  const [openEditPopup, setOpenEditPopup] = useState<boolean>(false);
  const [openCreatePopup, setOpenCreatePopup] = useState<boolean>(false);
  const [editActivityName, setEditActivityName] = useState("");
  const [editActivityPoints, setEditActivityPoints] = useState("");
  const [editingActivityId, setEditingActivityId] = useState<string | null>(
    null
  );

  const [createActivityName, setCreateActivityName] = useState("");
  const [createActivityPoints, setCreateActivityPoints] = useState("");

  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
    type: "success" | "error";
  }>({
    message: "",
    isVisible: false,
    type: "success",
  });

  // Load activities on mount
  useEffect(() => {
    refreshActivities();
  }, [refreshActivities]);

  // Display error notifications from context
  useEffect(() => {
    if (error) {
      showNotification(error, "error");
      clearError();
    }
  }, [error, clearError]);

  const showNotification = (
    message: string,
    type: "success" | "error" = "success"
  ) => {
    setNotification({ message, isVisible: true, type });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false, type: "success" });
    }, 3000);
  };

  const handleEditClick = (activity: {
    id: string;
    name: string;
    pointsAwarded: number;
  }) => {
    setEditActivityName(activity.name);
    setEditActivityPoints(activity.pointsAwarded.toString());
    setEditingActivityId(activity.id);
    setOpenEditPopup(true);
  };

  const handleCreateClick = () => {
    setCreateActivityName("");
    setCreateActivityPoints("");
    setOpenCreatePopup(true);
  };

  const saveActivityData = async () => {
    if (editingActivityId !== null) {
      const points = parseInt(editActivityPoints, 10);

      if (isNaN(points) || points < 1 || points > 1000) {
        showNotification("Points must be between 1 and 1000", "error");
        return;
      }

      if (editActivityName.trim().length < 3) {
        showNotification(
          "Activity name must be at least 3 characters long",
          "error"
        );
        return;
      }

      const result = await editActivity(
        editingActivityId,
        editActivityName,
        points
      );

      if (result.success) {
        showNotification(
          result.message || "Activity updated successfully!",
          "success"
        );
        setOpenEditPopup(false);
        setEditingActivityId(null);
        setEditActivityName("");
        setEditActivityPoints("");
      } else {
        showNotification(
          result.message || "Failed to update activity",
          "error"
        );
      }
    }
  };

  const createNewActivity = async () => {
    const points = parseInt(createActivityPoints, 10);

    if (isNaN(points) || points < 1 || points > 1000) {
      showNotification("Points must be between 1 and 1000", "error");
      return;
    }

    if (createActivityName.trim().length < 3) {
      showNotification(
        "Activity name must be at least 3 characters long",
        "error"
      );
      return;
    }

    const result = await createActivity(createActivityName, points);

    if (result.success) {
      showNotification(
        result.message || "Activity created successfully!",
        "success"
      );
      setOpenCreatePopup(false);
      setCreateActivityName("");
      setCreateActivityPoints("");
    } else {
      showNotification(result.message || "Failed to create activity", "error");
    }
  };

  const deleteActivityData = async () => {
    if (editingActivityId !== null) {
      const result = await deleteActivity(editingActivityId);

      if (result.success) {
        showNotification(
          result.message || "Activity deleted successfully!",
          "success"
        );
        setOpenEditPopup(false);
        setEditingActivityId(null);
        setEditActivityName("");
        setEditActivityPoints("");
      } else {
        showNotification(
          result.message || "Failed to delete activity",
          "error"
        );
      }
    }
  };

  return (
    <div className="activities">
      <img
        className="back-arrow"
        src={arrowLeft}
        alt="Back"
        onClick={() => navigate(-1)}
      />
      <Button variant="add" onClick={handleCreateClick}>
        Add
      </Button>
      <div
        className={`notification-bar notification-bar--${notification.type} ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

      <h1 className="manage-household__title">Activities</h1>

      {loading && <div className="loading-message">Loading activities...</div>}

      {!loading && activities.length === 0 && (
        <div className="no-activities-message">
          No activities found. Create one to get started!
        </div>
      )}

      {!loading && activities.length > 0 && (
        <div className="participants-list">
          <div className="participants-list__header">ACTIVITIES</div>
          {activities.map((activity) => (
            <div className="activity" key={activity.id} id={activity.id}>
              <div className="activity-data">
                <span className="points">
                  <img src={star} alt="" />
                  {activity.pointsAwarded}
                </span>
                <p>{activity.name}</p>
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

      {openCreatePopup && (
        <Popup
          variant="primary"
          title="Create Activity"
          onClose={() => setOpenCreatePopup(false)}
        >
          <Input
            variant="edit"
            value={createActivityName}
            onChange={(e) => setCreateActivityName(e.target.value)}
            placeholder="Activity name"
          />
          <Input
            variant="edit"
            type="number"
            value={createActivityPoints}
            onChange={(e) => setCreateActivityPoints(e.target.value)}
            placeholder="Points"
            min="1"
            max="1000"
          />
          <div className="button__edit-activity">
            <Button variant="primary" onClick={createNewActivity}>
              Create
            </Button>
          </div>
        </Popup>
      )}

      {openEditPopup && (
        <Popup
          variant="primary"
          title="Edit Activity"
          onClose={() => setOpenEditPopup(false)}
        >
          <Input
            variant="edit"
            value={editActivityName}
            onChange={(e) => setEditActivityName(e.target.value)}
            placeholder="Activity name"
          />
          <Input
            variant="edit"
            type="number"
            value={editActivityPoints}
            onChange={(e) => setEditActivityPoints(e.target.value)}
            placeholder="Points"
            min="1"
            max="1000"
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
