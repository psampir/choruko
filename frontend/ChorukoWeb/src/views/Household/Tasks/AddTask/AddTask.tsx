import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useTask } from "../../../../contexts/TaskContext";
import { useActivity } from "../../../../contexts/ActivityContext";
import { useHousehold } from "../../../../contexts/HouseholdContext";
import "./AddTask.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import Popup from "../../../../components/Popup/Popup";
import ErrorMessage from "../../../../components/ErrorMessage";
import { useThemeDir } from "../../../../utils/iconUtils";
import { getProfilePictureUrl } from "../../../../utils/imageUtils";

import arrowLeftIconLight from "../../../../assets/svg/light/arrow-left.svg";
import arrowLeftIconDark from "../../../../assets/svg/dark/arrow-left.svg";
import checkIconLight from "../../../../assets/svg/light/check.svg";
import checkIconDark from "../../../../assets/svg/dark/check.svg";
import starLight from "../../../../assets/svg/light/star.svg";
import starDark from "../../../../assets/svg/dark/star.svg";
import profileDefaultIconLight from "../../../../assets/svg/light/profile-icon.svg";
import profileDefaultIconDark from "../../../../assets/svg/dark/profile-icon.svg";

interface Assignee {
  id: string;
  name: string;
  avatar: string | null;
}

// Create or edit task with activity selection and assignee management
const AddTask: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { createTask, editTask: editTaskContext, deleteTask } = useTask();
  const { activities, loading: activitiesLoading } = useActivity();
  const { participants } = useHousehold();
  const themeDir = useThemeDir();

  const arrowLeftIcon =
    themeDir === "dark" ? arrowLeftIconDark : arrowLeftIconLight;
  const checkIcon = themeDir === "dark" ? checkIconDark : checkIconLight;
  const star = themeDir === "dark" ? starDark : starLight;
  const profileDefaultIcon =
    themeDir === "dark" ? profileDefaultIconDark : profileDefaultIconLight;

  // Determine if editing existing task
  const editTask = location.state?.editTask;
  const isEditMode = !!editTask;

  const [taskName, setTaskName] = useState("");
  const [taskDescription, setTaskDescription] = useState("");
  const [selectedActivity, setSelectedActivity] = useState<string | null>(null);
  const [selectedAssignees, setSelectedAssignees] = useState<string[]>([]);
  const [participantsCount, setParticipantsCount] = useState<string>("");
  const [deadline, setDeadline] = useState<Date | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Pre-fill form when editing
  useEffect(() => {
    if (editTask) {
      setTaskName(editTask.name || "");
      setTaskDescription(editTask.description || "");

      // Parse deadline from backend (handles both deadLine and deadline fields)
      const deadlineValue = editTask.deadLine || editTask.deadline;
      if (deadlineValue) {
        const deadlineDate = new Date(deadlineValue);
        setDeadline(deadlineDate);
      }

      // Set selected activity
      if (editTask.activityId) {
        setSelectedActivity(editTask.activityId);
      }

      // Set participants count
      if (editTask.participantsAmount) {
        setParticipantsCount(editTask.participantsAmount.toString());
      }

      // Set selected assignees
      if (editTask.assignees && Array.isArray(editTask.assignees)) {
        const assigneeIds = editTask.assignees.map((a: any) => a.id);
        setSelectedAssignees(assigneeIds);
      }
    }
  }, [editTask]);

  // State for collapsed sections
  const [collapsedSections, setCollapsedSections] = useState({
    activities: false,
    assignees: false,
  });

  // Toggle section collapse
  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  // Convert participants to assignees format
  const assignees: Assignee[] = participants.map((p) => ({
    id: p.id,
    name: `${p.firstName} ${p.lastName}`,
    avatar: getProfilePictureUrl(p.profilePicturePath),
  }));

  const handleActivitySelect = (activityId: string) => {
    setSelectedActivity(activityId);
  };

  const handleAssigneeToggle = (assigneeId: string) => {
    setSelectedAssignees((prev) =>
      prev.includes(assigneeId)
        ? prev.filter((id) => id !== assigneeId)
        : [...prev, assigneeId]
    );
  };

  // Format date as local ISO string without timezone conversion
  const formatLocalDateTime = (date: Date): string => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const seconds = String(date.getSeconds()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  };

  const handleAddTask = async () => {
    // Validate required fields
    const missingFields = [];
    if (!taskName) missingFields.push("Task name");
    if (!taskDescription) missingFields.push("Task description");
    if (!selectedActivity) missingFields.push("Activity");
    if (!deadline) missingFields.push("Deadline");
    if (!participantsCount) missingFields.push("Maximum assignees");

    if (missingFields.length > 0) {
      setErrorMessage(`Please fill in: ${missingFields.join(", ")}`);
      return;
    }

    const participantsCountNum = parseInt(participantsCount);
    if (participantsCountNum < 1 || isNaN(participantsCountNum)) {
      setErrorMessage("Maximum assignees must be at least 1");
      return;
    }

    // Validate that selected assignees don't exceed maximum
    if (selectedAssignees.length > participantsCountNum) {
      setErrorMessage(
        `You have selected ${selectedAssignees.length} assignees, but maximum is ${participantsCountNum}`
      );
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      const localDeadline = formatLocalDateTime(deadline!);

      if (isEditMode && editTask) {
        // Update existing task
        const result = await editTaskContext(editTask.id, {
          name: taskName,
          description: taskDescription,
          activityId: selectedActivity!,
          deadLine: localDeadline,
          participantsAmount: participantsCountNum,
          assigneeIds: selectedAssignees,
        });

        if (result.success) {
          navigate("/tasks");
        } else {
          setErrorMessage(result.message || "Failed to update task");
        }
      } else {
        // Create new task
        const result = await createTask({
          name: taskName,
          description: taskDescription,
          activityId: selectedActivity!,
          deadLine: localDeadline,
          participantsAmount: participantsCountNum,
          assigneeIds: selectedAssignees,
        });

        if (result.success) {
          navigate("/tasks");
        } else {
          setErrorMessage(result.message || "Failed to create task");
        }
      }
    } catch (error) {
      console.error("Error saving task:", error);
      setErrorMessage("An unexpected error occurred");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteTask = async () => {
    if (editTask) {
      setIsSubmitting(true);
      const result = await deleteTask(editTask.id);

      if (result.success) {
        navigate("/tasks");
      } else {
        console.error("Delete failed:", result.message);
        setErrorMessage(result.message || "Failed to delete task");
        setIsSubmitting(false);
      }
    }
  };

  const handleBack = () => {
    navigate("/tasks");
  };

  return (
    <div className="add-task">
      <NavbarMobile />

      <ErrorMessage
        message={errorMessage}
        onClose={() => setErrorMessage("")}
        variant="overlay"
      />

      <div className="add-task__container">
        {/* Header */}
        <div className="add-task__header">
          <button
            className="add-task__back-btn"
            onClick={handleBack}
            aria-label="Go back"
          >
            <img src={arrowLeftIcon} alt="Back" />
          </button>
          <h1 className="add-task__title">
            {isEditMode ? "Edit task" : "Add a new task"}
          </h1>
        </div>

        {/* Task Name */}
        <div className="add-task__section">
          <Input
            variant="edit"
            placeholder="Task name"
            value={taskName}
            onChange={(e) => setTaskName(e.target.value)}
            className="add-task__input"
          />
        </div>

        {/* Task Description */}
        <div className="add-task__section">
          <Input
            variant="edit"
            placeholder="Task description"
            value={taskDescription}
            onChange={(e) => setTaskDescription(e.target.value)}
            className="add-task__input"
          />
        </div>

        {/* Choose Activity Section */}
        {activitiesLoading ? (
          <div className="add-task__loading">Loading activities...</div>
        ) : activities && activities.length > 0 ? (
          <div
            className={`participants-list ${
              collapsedSections.activities ? "participants-list--collapsed" : ""
            }`}
          >
            <div
              className="participants-list__header"
              onClick={() => toggleSection("activities")}
            >
              SELECT AN ACTIVITY
              <span className="dropdown__arrow">&#9660;</span>
            </div>
            {!collapsedSections.activities && (
              <div className="participants-list__content">
                {activities.map((activity) => (
                  <div
                    className={`activity ${
                      selectedActivity === activity.id
                        ? "activity--selected"
                        : ""
                    }`}
                    key={activity.id}
                    onClick={() => handleActivitySelect(activity.id)}
                  >
                    <div className="activity-data">
                      <span className="points">
                        <img src={star} alt="star" />
                        {activity.pointsAwarded}
                      </span>
                      <p>{activity.name}</p>
                    </div>
                    {selectedActivity === activity.id && (
                      <img
                        src={checkIcon}
                        alt="Selected"
                        className="check-icon"
                      />
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        ) : (
          <div className="add-task__no-activities">
            <p>No activities available. Please create activities first.</p>
          </div>
        )}

        {/* Participants Count */}
        <div className="add-task__section">
          <div className="input__wrapper">
            <input
              type="number"
              min="1"
              placeholder="Maximum assignees"
              value={participantsCount}
              onChange={(e) => {
                const value = e.target.value;
                const num = parseInt(value);
                // Allow empty or valid positive numbers
                if (value === "" || (num >= 1 && !isNaN(num))) {
                  setParticipantsCount(value);
                }
              }}
              className="input input--primary add-task__input"
            />
            <span className="input__participants-icon">
              <img src={profileDefaultIcon} alt="participants icon" />
            </span>
          </div>
        </div>

        {/* Select Assignees Section */}
        <div
          className={`participants-list ${
            collapsedSections.assignees ? "participants-list--collapsed" : ""
          }`}
        >
          <div
            className="participants-list__header"
            onClick={() => toggleSection("assignees")}
          >
            SELECT ASSIGNEES
            <span className="dropdown__arrow">&#9660;</span>
          </div>
          {!collapsedSections.assignees && (
            <div className="participants-list__content">
              {assignees.map((assignee) => (
                <div
                  key={assignee.id}
                  className={`activity ${
                    selectedAssignees.includes(assignee.id)
                      ? "activity--selected"
                      : ""
                  }`}
                  onClick={() => handleAssigneeToggle(assignee.id)}
                >
                  <div className="activity-data">
                    <img
                      src={assignee.avatar || profileDefaultIcon}
                      alt={assignee.name}
                      className="assignee-avatar"
                    />
                    <p>{assignee.name}</p>
                  </div>
                  {selectedAssignees.includes(assignee.id) && (
                    <img
                      src={checkIcon}
                      alt="Selected"
                      className="check-icon"
                    />
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Deadline */}
        <div className="add-task__section">
          <Input
            variant="datetime"
            placeholder="Deadline"
            dateValue={deadline}
            onDateChange={setDeadline}
            className="add-task__input add-task__input--date"
          />
        </div>

        {/* Add/Save Button */}
        <div className="add-task__footer">
          <Button
            variant="primary"
            onClick={handleAddTask}
            disabled={isSubmitting}
          >
            {isSubmitting ? "Saving..." : isEditMode ? "Save Changes" : "Add"}
          </Button>
          {isEditMode && (
            <Button
              variant="red"
              onClick={() => setShowDeleteConfirm(true)}
              className="add-task__delete-btn"
              disabled={isSubmitting}
            >
              Delete task
            </Button>
          )}
        </div>
      </div>

      {/* Delete Confirmation Popup */}
      {showDeleteConfirm && (
        <Popup
          open={showDeleteConfirm}
          onClose={() => setShowDeleteConfirm(false)}
          title="Delete Task"
          variant="red"
          actions={[
            {
              label: "Cancel",
              onClick: () => setShowDeleteConfirm(false),
              variant: "secondary",
            },
            {
              label: "Delete",
              onClick: () => {
                handleDeleteTask();
                setShowDeleteConfirm(false);
              },
              variant: "danger",
            },
          ]}
        >
          <p>
            Are you sure you want to delete this task? This action cannot be
            undone.
          </p>
        </Popup>
      )}
    </div>
  );
};

export default AddTask;
