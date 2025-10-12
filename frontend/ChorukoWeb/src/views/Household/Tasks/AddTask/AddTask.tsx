import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./AddTask.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIcon from "../../../../assets/svg/arrow-left.svg";
import pencilIcon from "../../../../assets/svg/pencil.svg";
import checkIcon from "../../../../assets/svg/check.svg";

// Activity type definition
interface Activity {
  id: number;
  name: string;
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

  // Mock data for activities
  const activities: Activity[] = [
    { id: 1, name: "Vacuuming the floor", points: 64 },
    { id: 2, name: "Vacuuming the floor", points: 64 },
    { id: 3, name: "Vacuuming the floor", points: 64 },
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
    // TODO: Implement task creation logic
    console.log({
      taskDescription,
      selectedActivity,
      selectedAssignees,
      deadline,
    });
    navigate("/tasks");
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
            variant="primary"
            placeholder="Task description"
            value={taskDescription}
            onChange={(e) => setTaskDescription(e.target.value)}
            className="add-task__input"
          />
          <button className="add-task__edit-icon" aria-label="Edit description">
            <img src={pencilIcon} alt="Edit" />
          </button>
        </div>

        {/* Choose Activity Section */}
        <div className="add-task__section add-task__section--bordered">
          <div className="add-task__section-header">
            <h2 className="add-task__section-title">CHOOSE AN ACTIVITY</h2>
          </div>
          <div className="add-task__list">
            {activities.map((activity) => (
              <div
                key={activity.id}
                className={`add-task__item ${
                  selectedActivity === activity.id
                    ? "add-task__item--selected"
                    : ""
                }`}
                onClick={() => handleActivitySelect(activity.id)}
              >
                <div className="add-task__item-info">
                  <span className="add-task__points">★ {activity.points}</span>
                  <span className="add-task__activity-name">
                    {activity.name}
                  </span>
                </div>
                <button
                  className="add-task__item-action"
                  aria-label="Edit activity"
                >
                  <img src={pencilIcon} alt="Edit" />
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Select Assignees Section */}
        <div className="add-task__section add-task__section--bordered">
          <div className="add-task__section-header">
            <h2 className="add-task__section-title">SELECT ASSIGNEES</h2>
          </div>
          <div className="add-task__list">
            {assignees.map((assignee) => (
              <div
                key={assignee.id}
                className={`add-task__item ${
                  selectedAssignees.includes(assignee.id)
                    ? "add-task__item--selected"
                    : ""
                }`}
                onClick={() => handleAssigneeToggle(assignee.id)}
              >
                <div className="add-task__item-info">
                  <img
                    src={assignee.avatar}
                    alt={assignee.name}
                    className="add-task__avatar"
                  />
                  <span className="add-task__assignee-name">
                    {assignee.name}
                  </span>
                </div>
                {selectedAssignees.includes(assignee.id) && (
                  <img
                    src={checkIcon}
                    alt="Selected"
                    className="add-task__check"
                  />
                )}
              </div>
            ))}
          </div>
        </div>

        {/* Deadline */}
        <div className="add-task__section">
          <Input
            type="date"
            variant="primary"
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
