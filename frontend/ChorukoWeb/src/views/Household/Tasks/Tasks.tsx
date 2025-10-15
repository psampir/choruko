import React, { useState, useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
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
  const location = useLocation();

  // State to manage collapsed sections
  const [collapsedSections, setCollapsedSections] = useState({
    overtime: false,
    todo: false,
    done: false,
  });

  // Toggle section collapse
  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

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

  // Ref to track if we've already processed the new task
  const hasAddedTaskRef = useRef(false);

  // Effect to handle new task from AddTask page
  useEffect(() => {
    const state = location.state as {
      newTask?: {
        name: string;
        description: string;
        assignees: string[];
        deadline: string;
      };
    };

    // Only process if we have a new task and haven't already added it
    if (state?.newTask && !hasAddedTaskRef.current) {
      hasAddedTaskRef.current = true;
      const { newTask } = state;

      // Calculate time left based on deadline
      const calculateTimeLeft = (deadline: string) => {
        if (!deadline) return "0h";

        const deadlineDate = new Date(deadline);
        const now = new Date();
        const diffMs = deadlineDate.getTime() - now.getTime();
        const diffHours = Math.round(diffMs / (1000 * 60 * 60));

        return diffHours > 0 ? `${diffHours}h` : `${diffHours}h`;
      };

      const timeLeft = calculateTimeLeft(newTask.deadline);

      // Create the task with proper format
      setTasks((prevTasks) => {
        const taskToAdd: Task = {
          id: prevTasks.length + 1,
          name: newTask.name,
          description: newTask.description,
          assignees: newTask.assignees,
          timeLeft: timeLeft,
          status: "todo",
        };
        return [...prevTasks, taskToAdd];
      });

      // Clear the location state to prevent re-adding on refresh
      navigate(location.pathname, { replace: true, state: {} });
    }

    // Reset the ref when state is cleared
    if (!state?.newTask) {
      hasAddedTaskRef.current = false;
    }
  }, [location.state, navigate, location.pathname]);

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
        <div
          className={`tasks__section tasks__section--overtime ${
            collapsedSections.overtime ? "tasks__section--collapsed" : ""
          }`}
        >
          <h2
            className="tasks__section-title"
            onClick={() => toggleSection("overtime")}
          >
            OVERTIME
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("overtime")}
          >
            &#9660;
          </span>
          {!collapsedSections.overtime && (
            <div className="tasks__list">
              {overtimeTasks.length > 0 ? (
                overtimeTasks.map((task, index) =>
                  renderTask(
                    task,
                    index === overtimeTasks.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="tasks__empty-message">No overtime tasks</p>
              )}
            </div>
          )}
        </div>

        {/* To-Do Section */}
        <div
          className={`tasks__section tasks__section--todo ${
            collapsedSections.todo ? "tasks__section--collapsed" : ""
          }`}
        >
          <h2
            className="tasks__section-title"
            onClick={() => toggleSection("todo")}
          >
            TO DO
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("todo")}
          >
            &#9660;
          </span>
          {!collapsedSections.todo && (
            <div className="tasks__list">
              {todoTasks.length > 0 ? (
                todoTasks.map((task, index) =>
                  renderTask(task, index === todoTasks.length - 1, index === 0)
                )
              ) : (
                <p className="tasks__empty-message">No tasks to do</p>
              )}
            </div>
          )}
        </div>

        {/* Done Section */}
        <div
          className={`tasks__section tasks__section--done ${
            collapsedSections.done ? "tasks__section--collapsed" : ""
          }`}
        >
          <h2
            className="tasks__section-title"
            onClick={() => toggleSection("done")}
          >
            DONE
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("done")}
          >
            &#9660;
          </span>
          {!collapsedSections.done && (
            <div className="tasks__list">
              {doneTasks.length > 0 ? (
                doneTasks.map((task, index) =>
                  renderTask(task, index === doneTasks.length - 1, index === 0)
                )
              ) : (
                <p className="tasks__empty-message">No completed tasks</p>
              )}
            </div>
          )}
        </div>
      </div>

      <BottomNav />
    </div>
  );
};

export default Tasks;
