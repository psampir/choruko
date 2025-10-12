import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Tasks.scss";
import Button from "../../../components/Button";
import BottomNav from "../../../components/BottomNav/BottomNav";
import timerIcon from "../../../assets/svg/timer.svg";

// Task type definition
interface Task {
  id: number;
  name: string;
  description: string;
  assignees: string[];
  timeLeft: string;
  status: "overtime" | "todo" | "done";
}

const Tasks: React.FC = () => {
  const navigate = useNavigate();

  // State to manage tasks
  const [tasks, setTasks] = useState<Task[]>([
    {
      id: 1,
      name: "Activity name",
      description: "Activity description",
      assignees: [
        "https://randomuser.me/api/portraits/men/1.jpg",
        "https://randomuser.me/api/portraits/men/2.jpg",
        "https://randomuser.me/api/portraits/men/3.jpg",
        "https://randomuser.me/api/portraits/women/1.jpg",
      ],
      timeLeft: "-4h",
      status: "overtime",
    },
    {
      id: 2,
      name: "Second task",
      description: "Another task description",
      assignees: ["https://randomuser.me/api/portraits/women/2.jpg"],
      timeLeft: "2h",
      status: "todo",
    },
    {
      id: 3,
      name: "Activity name",
      description: "Activity description",
      assignees: [
        "https://randomuser.me/api/portraits/men/1.jpg",
        "https://randomuser.me/api/portraits/men/2.jpg",
        "https://randomuser.me/api/portraits/men/3.jpg",
        "https://randomuser.me/api/portraits/women/1.jpg",
      ],
      timeLeft: "-4h",
      status: "overtime",
    },
  ]);

  // Add new task function
  const addTask = () => {
    // const newTask: Task = {
    //   id: tasks.length + 1,
    //   name: "New Task",
    //   description: "Task description",
    //   assignees: [
    //     "https://randomuser.me/api/portraits/men/" +
    //       (tasks.length + 1) +
    //       ".jpg",
    //     "https://randomuser.me/api/portraits/women/" +
    //       (tasks.length + 1) +
    //       ".jpg",
    //   ],
    //   timeLeft: "0h",
    //   status: "todo",
    // };
    // setTasks([...tasks, newTask]);

    navigate("/tasks/add");
  };

  // Determine which avatars to show
  const getVisibleAvatars = (avatars: string[]) => {
    if (avatars.length <= 2) {
      // Show all if 2 or less
      return avatars.map((src, index) => ({ src, isActive: true, index }));
    } else {
      // Show first one + hide the rest
      return avatars.map((src, index) => ({
        src,
        isActive: index === 0, // Only first is active
        index,
      }));
    }
  };

  // Filter tasks by status
  const overtimeTasks = tasks.filter((task) => task.status === "overtime");
  const todoTasks = tasks.filter((task) => task.status === "todo");
  const doneTasks = tasks.filter((task) => task.status === "done");

  // Render individual task component
  const renderTask = (task: Task, isLast: boolean, isFirst: boolean) => {
    const visibleAvatars = getVisibleAvatars(task.assignees);
    const hiddenCount =
      task.assignees.length > 2 ? task.assignees.length - 1 : 0;

    return (
      <div
        className={`task ${isFirst ? "task--first" : ""} ${
          isLast ? "task--last" : ""
        }`}
        key={task.id}
      >
        <div className="task__info">
          <p className="task__name">{task.name}</p>
          <p className="task__description">{task.description}</p>
        </div>
        <div className="task__meta">
          <div className="task__assignees">
            <div className="task__assignees-avatars">
              {visibleAvatars.map((avatar, idx) => (
                <img
                  key={idx}
                  className={`task__assignee-avatar ${
                    avatar.isActive ? "task__assignee-avatar--active" : ""
                  }`}
                  src={avatar.src}
                  alt={`assignee ${idx + 1}`}
                />
              ))}
              {hiddenCount > 0 && (
                <span className="task__assignee-avatar task__assignee-avatar--active task__assignee-avatar--counter">
                  +{hiddenCount}
                </span>
              )}
            </div>
            <div className="task__time-left">
              <p className="task__time-left-amount">{task.timeLeft}</p>
              <span>
                <img src={timerIcon} alt="timer icon" />
              </span>
            </div>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div className="tasks">
      <div className="tasks__header">
        <h1 className="tasks__title">Tasks</h1>
        <Button variant="primary" onClick={addTask}>
          Add
        </Button>
      </div>

      <div className="tasks__content">
        {/* Overtime Section */}
        <div className="tasks__section tasks__section--overtime">
          <h2 className="tasks__section-title">OVERTIME</h2>
          {overtimeTasks.length > 0 ? (
            overtimeTasks.map((task, index) =>
              renderTask(task, index === overtimeTasks.length - 1, index === 0)
            )
          ) : (
            <p className="tasks__empty-message">No overtime tasks</p>
          )}
        </div>

        {/* To-Do Section */}
        <div className="tasks__section tasks__section--todo">
          <h2 className="tasks__section-title">TO DO</h2>
          {todoTasks.length > 0 ? (
            todoTasks.map((task, index) =>
              renderTask(task, index === todoTasks.length - 1, index === 0)
            )
          ) : (
            <p className="tasks__empty-message">No tasks to do</p>
          )}
        </div>

        {/* Done Section */}
        <div className="tasks__section tasks__section--done">
          <h2 className="tasks__section-title">DONE</h2>
          {doneTasks.length > 0 ? (
            doneTasks.map((task, index) =>
              renderTask(task, index === doneTasks.length - 1, index === 0)
            )
          ) : (
            <p className="tasks__empty-message">No completed tasks</p>
          )}
        </div>
      </div>

      <BottomNav />
    </div>
  );
};

export default Tasks;
