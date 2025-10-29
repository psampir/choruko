import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useUser } from "../../../../contexts/UserContext";
import "./AddTask.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIcon from "../../../../assets/svg/arrow-left.svg";
import Popup from "../../../../components/Popup/Popup";

import checkIcon from "../../../../assets/svg/check.svg";
import star from "../../../../assets/svg/star.svg";

// Activity type definition
interface Activity {
  id: number;
  name: string;
  info: string;
  points: number;
}

// Assignee type definition
interface Assignee {
  id: number;
  name: string;
  avatar: string;
}

const AddTask: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();

  // Check if we're in edit mode
  const editTask = location.state?.editTask;
  const isEditMode = !!editTask;

  const [taskDescription, setTaskDescription] = useState("");
  const [selectedActivity, setSelectedActivity] = useState<number | null>(null);
  const [selectedAssignees, setSelectedAssignees] = useState<number[]>([]);
  const [deadline, setDeadline] = useState<Date | null>(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // Pre-fill form when in edit mode
  useEffect(() => {
    if (editTask) {
      setTaskDescription(editTask.description || "");
      // Convert deadline string to Date object
      if (editTask.deadline) {
        setDeadline(new Date(editTask.deadline));
      }

      // Find and select the activity by name
      const activity = activities.find((a) => a.name === editTask.name);
      if (activity) {
        setSelectedActivity(activity.id);
      }

      // Select assignees by matching both name AND avatar
      if (editTask.assignees && Array.isArray(editTask.assignees)) {
        const assigneeIds = assignees
          .filter((assignee) =>
            editTask.assignees.some(
              (ea: any) =>
                ea.name === assignee.name && ea.avatar === assignee.avatar
            )
          )
          .map((assignee) => assignee.id);
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

  // Mock data for activities
  const activities: Activity[] = [
    {
      id: 1,
      name: "Vacuuming the floor",
      info: "Vacuuming the floor",
      points: 64,
    },
    { id: 2, name: "Clean dishes", info: "Clean dishes", points: 64 },
    { id: 3, name: "Make breakfast", info: "Make breakfast", points: 64 },
  ];

  // Mock data for assignees
  const assignees: Assignee[] = [
    {
      id: 1,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/1.jpg",
    },
    {
      id: 2,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/2.jpg",
    },
    {
      id: 3,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/3.jpg",
    },
  ];

  const handleActivitySelect = (activityId: number) => {
    setSelectedActivity(activityId);
  };

  const handleAssigneeToggle = (assigneeId: number) => {
    setSelectedAssignees((prev) =>
      prev.includes(assigneeId)
        ? prev.filter((id) => id !== assigneeId)
        : [...prev, assigneeId]
    );
  };

  const handleAddTask = () => {
    // Validate that required fields are filled
    if (
      !taskDescription ||
      !selectedActivity ||
      selectedAssignees.length === 0
    ) {
      console.log("Please fill all required fields");
      return;
    }

    // Get the selected activity details
    const activity = activities.find((a) => a.id === selectedActivity);

    // Get the selected assignees' objects
    const selectedAssigneeObjs = assignees.filter((assignee) =>
      selectedAssignees.includes(assignee.id)
    );

    if (isEditMode && editTask) {
      // Update existing task
      const updatedTask = {
        id: editTask.id,
        name: activity?.name || taskDescription,
        description: taskDescription,
        assignees: selectedAssigneeObjs,
        deadline: deadline ? deadline.toISOString() : "",
        author: editTask.author,
      };

      // Navigate back to tasks with the updated task data
      navigate("/tasks", { state: { updatedTask } });
    } else {
      // Create new task
      const newTask = {
        name: activity?.name || taskDescription,
        description: taskDescription,
        assignees: selectedAssigneeObjs,
        deadline: deadline ? deadline.toISOString() : "",
        author: user
          ? {
              name: `${user.firstName} ${user.lastName}`,
              avatar: user.profilePicturePath || null,
            }
          : null,
      };

      // Navigate back to tasks with the new task data
      navigate("/tasks", { state: { newTask } });
    }
  };

  const handleDeleteTask = () => {
    if (editTask) {
      // Navigate back with delete flag
      navigate("/tasks", { state: { deleteTaskId: editTask.id } });
    }
  };

  const handleBack = () => {
    navigate("/tasks");
  };

  return (
    <div className="add-task">
      <NavbarMobile />

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
        {activities && (
          <div
            className={`participants-list ${
              collapsedSections.activities ? "participants-list--collapsed" : ""
            }`}
          >
            <div
              className="participants-list__header"
              onClick={() => toggleSection("activities")}
            >
              ACTIVITIES
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
                        {activity.points}
                      </span>
                      <p>{activity.info}</p>
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
        )}

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
                      src={assignee.avatar}
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
          <Button variant="primary" onClick={handleAddTask}>
            {isEditMode ? "Save Changes" : "Add"}
          </Button>
          {isEditMode && (
            <Button
              variant="red"
              onClick={() => setShowDeleteConfirm(true)}
              className="add-task__delete-btn"
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
