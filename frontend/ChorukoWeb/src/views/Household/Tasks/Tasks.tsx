import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Tasks.scss";
import Button from "../../../components/Button";
import BottomNav from "../../../components/BottomNav/BottomNav";
import Popup from "../../../components/Popup/Popup";
import { useUser } from "../../../contexts/UserContext";
import { useTask } from "../../../contexts/TaskContext";
import { useThemeDir } from "../../../utils/iconUtils";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import type { HouseholdTask } from "../../../services/taskService";

import timerIconLight from "../../../assets/svg/light/timer.svg";
import timerIconDark from "../../../assets/svg/dark/timer.svg";
import profileIconLight from "../../../assets/svg/light/profile-icon.svg";
import profileIconDark from "../../../assets/svg/dark/profile-icon.svg";
import starIconLight from "../../../assets/svg/light/star-black.svg";
import starIconDark from "../../../assets/svg/dark/star-black.svg";
import pencilLight from "../../../assets/svg/light/pencil-light.svg";
import pencilDark from "../../../assets/svg/dark/pencil-light.svg";

interface Assignee {
  name: string;
  avatar: string;
}
interface Author {
  name: string;
  avatar: string | null;
}
interface Task {
  id: number;
  backendId: string; // Store the original backend GUID
  name: string;
  description: string;
  assignees: Assignee[];
  timeLeft: string;
  deadline?: string;
  status: "overtime" | "todo" | "done";
  author?: Author;
}

const Tasks: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useUser();
  const { tasks: backendTasks, loading, error, refreshTasks, markTaskAsCompleted, markTaskAsUncompleted, assignToTask, unassignFromTask } = useTask();
  const themeDir = useThemeDir();

  // Theme-aware icon selection
  const timerIcon = themeDir === "dark" ? timerIconDark : timerIconLight;
  const profileIcon = themeDir === "dark" ? profileIconDark : profileIconLight;
  const starIcon = themeDir === "dark" ? starIconDark : starIconLight;
  const pencilIcon = themeDir === "dark" ? pencilDark : pencilLight;

  const [collapsedSections, setCollapsedSections] = useState({
    overtime: false,
    todo: false,
    done: false,
  });

  const [tasks, setTasks] = useState<Task[]>([]);

  // Determine task status based on backend status and deadline
  const determineTaskStatus = (backendTask: HouseholdTask): "overtime" | "todo" | "done" => {
    if (backendTask.status === "Completed") {
      return "done";
    }
    
    const deadline = new Date(backendTask.deadLine);
    const now = new Date();
    
    if (deadline < now) {
      return "overtime";
    }
    
    return "todo";
  };

  // Convert backend tasks to UI format
  useEffect(() => {
    const convertedTasks: Task[] = backendTasks.map((backendTask: HouseholdTask, index: number) => {
      return {
        id: index, // Use array index for UI display key
        backendId: backendTask.id, // Store original GUID
        name: backendTask.name,
        description: backendTask.description,
        assignees: backendTask.assignees.map(a => ({
          name: `${a.firstName} ${a.lastName}`,
          avatar: getProfilePictureUrl(a.profilePicturePath) || profileIcon,
        })),
        timeLeft: formatTimeLeft(backendTask.deadLine),
        deadline: backendTask.deadLine,
        status: determineTaskStatus(backendTask),
        author: {
          name: `${backendTask.creator.firstName} ${backendTask.creator.lastName}`,
          avatar: getProfilePictureUrl(backendTask.creator.profilePicturePath),
        },
      };
    });
    setTasks(convertedTasks);
  }, [backendTasks]);

  const [, setCurrentTime] = useState(Date.now());

  // Dynamic interval updates based on closest deadline
  useEffect(() => {
    const getUpdateInterval = () => {
      const now = Date.now();
      let minHoursToDeadline = Infinity;

      tasks.forEach((task) => {
        if (task.deadline) {
          const deadlineDate = new Date(task.deadline).getTime();
          const diffMs = Math.abs(deadlineDate - now);
          const diffHours = diffMs / (1000 * 60 * 60);

          if (diffHours < minHoursToDeadline) {
            minHoursToDeadline = diffHours;
          }
        }
      });

      if (minHoursToDeadline < 1) {
        return 60000;
      } else if (minHoursToDeadline < 24) {
        return 3600000;
      } else {
        return 86400000;
      }
    };

    const interval = setInterval(() => {
      setCurrentTime(Date.now());
    }, getUpdateInterval());

    return () => clearInterval(interval);
  }, [tasks]);

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  const formatTimeLeft = (deadline?: string) => {
    if (!deadline) return "N/A";

    const deadlineDate = new Date(deadline);
    const now = new Date();
    const diffMs = deadlineDate.getTime() - now.getTime();
    const diffHours = Math.round(diffMs / (1000 * 60 * 60));

    // Show date/time format if more than 24 hours
    if (Math.abs(diffHours) > 24) {
      const day = deadlineDate.getDate().toString().padStart(2, "0");
      const month = (deadlineDate.getMonth() + 1).toString().padStart(2, "0");
      const hours = deadlineDate.getHours().toString().padStart(2, "0");
      const minutes = deadlineDate.getMinutes().toString().padStart(2, "0");
      return `${day}.${month} ${hours}:${minutes}`;
    }

    return diffHours > 0 ? `${diffHours}h` : `${diffHours}h`;
  };



  const addTask = () => {
    navigate("/tasks/add");
  };

  const getVisibleAvatars = (avatars: string[]) => {
    if (avatars.length <= 2) {
      return avatars.map((src, index) => ({ src, isActive: true, index }));
    } else {
      // Show only first avatar, hide rest with counter
      return avatars.map((src, index) => ({
        src,
        isActive: index === 0,
        index,
      }));
    }
  };

  const overtimeTasks = tasks.filter((task) => task.status === "overtime");
  const todoTasks = tasks.filter((task) => task.status === "todo");
  const doneTasks = tasks.filter((task) => task.status === "done");

  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [selectedBackendTask, setSelectedBackendTask] = useState<HouseholdTask | null>(null);
  const [showPopup, setShowPopup] = useState(false);

  const renderTask = (task: Task, isLast: boolean, isFirst: boolean) => {
    // Only show avatars if there are assignees
    const hasAssignees = task.assignees.length > 0;
    const visibleAvatars = hasAssignees 
      ? getVisibleAvatars(task.assignees.map((a) => a.avatar))
      : [];
    const hiddenCount = task.assignees.length > 2 ? task.assignees.length - 1 : 0;

    return (
      <div
        className={`task ${isFirst ? "task--first" : ""} ${
          isLast ? "task--last" : ""
        }`}
        key={task.id}
        onClick={() => {
          // Find the corresponding backend task using the stored GUID
          const backendTask = backendTasks.find(bt => bt.id === task.backendId);
          console.log('Task clicked:', {
            taskId: task.id,
            backendId: task.backendId,
            taskName: task.name,
            backendTask: backendTask,
            backendTaskId: backendTask?.id,
            creatorId: backendTask?.creatorId
          });
          setSelectedTask(task);
          setSelectedBackendTask(backendTask || null);
          setShowPopup(true);
        }}
        style={{ cursor: "pointer" }}
      >
        <div className="task__info">
          <p className="task__name">{task.name}</p>
          <p className="task__description">{task.description}</p>
        </div>
        <div className="task__meta">
          <div className="task__assignees">
            <div className="task__assignees-avatars">
              {hasAssignees ? (
                <>
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
                    <span
                      className={`task__assignee-avatar task__assignee-avatar--active task__assignee-avatar--counter task__assignee-avatar--counter-${task.status}`}
                    >
                      +{hiddenCount}
                    </span>
                  )}
                </>
              ) : (
                <span className="task__no-assignees">No assignees</span>
              )}
            </div>
            <div className="task__time-left">
              <p className="task__time-left-amount">
                {formatTimeLeft(task.deadline)}
              </p>
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

      {loading && (
        <div className="tasks__loading">
          <p>Loading tasks...</p>
        </div>
      )}

      {error && (
        <div className="tasks__error">
          <p>Error: {error}</p>
          <Button variant="primary" onClick={refreshTasks}>
            Retry
          </Button>
        </div>
      )}

      {!loading && !error && (
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
      )}

      {/* Task Details Popup */}
      {showPopup && selectedTask && (
        <Popup
          open={showPopup}
          onClose={() => {
            setShowPopup(false);
            setSelectedTask(null);
            setSelectedBackendTask(null);
          }}
          title={selectedTask.name}
          variant={selectedTask.status === "overtime" ? "red" : "primary"}
          actions={[
            {
              label: selectedTask.status === "done" ? "Mark as undone" : "Mark as done",
              onClick: async () => {
                if (selectedBackendTask) {
                  if (selectedTask.status === "done") {
                    await markTaskAsUncompleted(selectedBackendTask.id);
                  } else {
                    await markTaskAsCompleted(selectedBackendTask.id);
                  }
                }
                setShowPopup(false);
                setSelectedTask(null);
                setSelectedBackendTask(null);
              },
              variant:
                selectedTask.status === "overtime" ? ("danger" as const) : ("primary" as const),
            },
          ]}
          customActions={
            // Show edit button only for task author (compare user ID with creator ID)
            (() => {
              const canEdit = user && selectedBackendTask && user.id === selectedBackendTask.creatorId;
              console.log('Edit button check:', {
                userId: user?.id,
                creatorId: selectedBackendTask?.creatorId,
                canEdit,
                user,
                selectedBackendTask
              });
              return canEdit;
            })() ? (
              <div
                className="task-edit-button"
                onClick={() => {
                  navigate("/tasks/add", {
                    state: {
                      editTask: selectedBackendTask,
                    },
                  });
                  setShowPopup(false);
                  setSelectedTask(null);
                  setSelectedBackendTask(null);
                }}
              >
                <img src={pencilIcon} alt="Edit task" />
              </div>
            ) : null
          }
          className="task-details-popup task-details-popup--button-top"
        >
          <div className="task-details-content">
            <div className="task-details-description">
              {selectedTask.description}
            </div>
            <div className="participants-list assignor-list">
              <div className="participants-list__header">ASSIGNOR</div>
              <div className="participants-list__content">
                <div className="activity activity--selected">
                  <div className="activity-data">
                    <img
                      src={selectedTask.author?.avatar || profileIcon}
                      alt={selectedTask.author?.name || "User"}
                      className="assignee-avatar"
                    />
                    <p>{selectedTask.author?.name || "Unknown"}</p>
                  </div>
                </div>
              </div>
            </div>
            <div className="participants-list assignees-list">
              <div className="participants-list__header">ASSIGNEES</div>
              <div className="participants-list__content">
                {selectedTask.assignees.map((a, idx) => (
                  <div className="activity activity--selected" key={idx}>
                    <div className="activity-data">
                      <img
                        src={a.avatar}
                        alt={a.name}
                        className="assignee-avatar"
                      />
                      <p>{a.name}</p>
                    </div>
                  </div>
                ))}
                
                {/* Assign/Unassign button */}
                {(() => {
                  const isAssigned = selectedBackendTask?.assignees?.some(a => a.id === user?.id);
                  const currentAssignees = selectedBackendTask?.assignees?.length || 0;
                  const maxAssignees = selectedBackendTask?.participantsAmount || 1;
                  const isMaxReached = currentAssignees >= maxAssignees;
                  
                  if (isAssigned) {
                    return (
                      <Button
                        variant={selectedTask.status === "overtime" ? "red" : "primary"}
                        onClick={async () => {
                          if (selectedBackendTask) {
                            const result = await unassignFromTask(selectedBackendTask.id);
                            if (!result.success) {
                              alert(result.message || "Failed to unassign from task");
                              return;
                            }
                          }
                          setShowPopup(false);
                          setSelectedTask(null);
                          setSelectedBackendTask(null);
                        }}
                        className="task-assign-button"
                      >
                        Unassign yourself
                      </Button>
                    );
                  } else if (!isMaxReached) {
                    // Only show assign button if maximum not reached
                    return (
                      <Button
                        variant={selectedTask.status === "overtime" ? "red" : "primary"}
                        onClick={async () => {
                          if (selectedBackendTask) {
                            const result = await assignToTask(selectedBackendTask.id);
                            if (!result.success) {
                              alert(result.message || "Failed to assign to task");
                              return;
                            }
                          }
                          setShowPopup(false);
                          setSelectedTask(null);
                          setSelectedBackendTask(null);
                        }}
                        className="task-assign-button"
                      >
                        Assign yourself
                      </Button>
                    );
                  }
                  // Don't show anything if not assigned and max reached
                  return null;
                })()}
              </div>
            </div>
            <div className="task-details-footer">
              <div className="task-details-footer-item first">
                <div>
                  <span className="task-details-footer-icon">
                    <img src={timerIcon} alt="date icon" />
                  </span>
                  <span className="task-details-footer-label">DEADLINE</span>
                </div>
                <div>
                  <span className="task-details-footer-icon">
                    <img src={starIcon} alt="star icon" />
                  </span>
                  <span className="task-details-footer-label">POINTS</span>
                </div>
              </div>
              <div className="task-details-footer-item second">
                <span className="task-details-footer-value">
                  {formatTimeLeft(selectedTask.deadline)}
                </span>
                <div className="task-details-footer-value">
                  {selectedBackendTask?.activity?.pointsAwarded || 0} pts
                </div>
              </div>
            </div>
          </div>
        </Popup>
      )}

      <BottomNav />
    </div>
  );
};

export default Tasks;
