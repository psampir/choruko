import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./AddTask.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIcon from "../../../../assets/svg/arrow-left.svg";

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
  const [taskDescription, setTaskDescription] = useState("");
  const [selectedActivity, setSelectedActivity] = useState<number | null>(null);
  const [selectedAssignees, setSelectedAssignees] = useState<number[]>([]);
  const [deadline, setDeadline] = useState("");

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

    // Get the selected assignees' avatars
    const assigneeAvatars = assignees
      .filter((assignee) => selectedAssignees.includes(assignee.id))
      .map((assignee) => assignee.avatar);

    // Create the new task object
    const newTask = {
      name: activity?.name || taskDescription,
      description: taskDescription,
      assignees: assigneeAvatars,
      deadline: deadline,
    };

    // Navigate back to tasks with the new task data
    navigate("/tasks", { state: { newTask } });
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
          <h1 className="add-task__title">Add a new task</h1>
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
            type="date"
            variant="datetime"
            placeholder="Deadline"
            value={deadline}
            onChange={(e) => setDeadline(e.target.value)}
            className="add-task__input add-task__input--date"
          />
        </div>

        {/* Add Button */}
        <div className="add-task__footer">
          <Button variant="primary" onClick={handleAddTask}>
            Add
          </Button>
        </div>
      </div>
    </div>
  );
};

export default AddTask;
