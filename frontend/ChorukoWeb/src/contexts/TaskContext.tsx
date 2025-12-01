import React, { createContext, useContext, useState, useCallback, useEffect, useRef } from "react";
import type { ReactNode } from "react";
import { useHousehold } from "./HouseholdContext";
import taskService from "../services/taskService";
import type { HouseholdTask, CreateTaskData, EditTaskData } from "../services/taskService";

// Define the context interface
interface TaskContextType {
  tasks: HouseholdTask[];
  loading: boolean;
  error: string | null;
  refreshTasks: () => Promise<void>;
  createTask: (data: CreateTaskData) => Promise<{ success: boolean; message?: string }>;
  editTask: (taskId: string, data: EditTaskData) => Promise<{ success: boolean; message?: string }>;
  deleteTask: (taskId: string) => Promise<{ success: boolean; message?: string }>;
  assignToTask: (taskId: string) => Promise<{ success: boolean; message?: string }>;
  unassignFromTask: (taskId: string) => Promise<{ success: boolean; message?: string }>;
  markTaskAsCompleted: (taskId: string) => Promise<{ success: boolean; message?: string }>;
  markTaskAsUncompleted: (taskId: string) => Promise<{ success: boolean; message?: string }>;
  clearError: () => void;
}

// Create the context
const TaskContext = createContext<TaskContextType | undefined>(undefined);

// Custom hook to use the task context
export const useTask = () => {
  const context = useContext(TaskContext);
  if (context === undefined) {
    throw new Error("useTask must be used within a TaskProvider");
  }
  return context;
};

// TaskProvider component props
interface TaskProviderProps {
  children: ReactNode;
}

// TaskProvider component
export const TaskProvider: React.FC<TaskProviderProps> = ({ children }) => {
  const { householdInfo } = useHousehold();
  const [tasks, setTasks] = useState<HouseholdTask[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const hasDataRef = useRef(false);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Fetch tasks from the server
  const refreshTasks = useCallback(async () => {
    if (!householdInfo?.id) {
      setTasks([]);
      hasDataRef.current = false;
      return;
    }

    try {
      // Only show loading state if we don't have data yet
      if (!hasDataRef.current) {
        setLoading(true);
      }
      setError(null);

      const result = await taskService.getAllTasks(householdInfo.id);

      if (result.success && result.data) {
        setTasks(result.data);
        hasDataRef.current = true;
      } else {
        setError(result.message || "Failed to fetch tasks");
        setTasks([]);
        hasDataRef.current = false;
      }
    } catch (err) {
      console.error("Failed to fetch tasks:", err);
      setError("Failed to fetch tasks");
      setTasks([]);
      hasDataRef.current = false;
    } finally {
      setLoading(false);
    }
  }, [householdInfo?.id]);

  // Create a new task
  const createTask = async (data: CreateTaskData) => {
    if (!householdInfo?.id) {
      return { success: false, message: "No household found" };
    }

    try {
      setLoading(true);
      const result = await taskService.createTask(householdInfo.id, data);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to create task");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to create task:", err);
      const message = "Failed to create task";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Edit an existing task
  const editTask = async (taskId: string, data: EditTaskData) => {
    try {
      setLoading(true);
      const result = await taskService.editTask(taskId, data);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to update task");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to update task:", err);
      const message = "Failed to update task";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Delete a task
  const deleteTask = async (taskId: string) => {
    try {
      setLoading(true);
      const result = await taskService.deleteTask(taskId);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to delete task");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to delete task:", err);
      const message = "Failed to delete task";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Assign current user to a task
  const assignToTask = async (taskId: string) => {
    try {
      setLoading(true);
      const result = await taskService.assignToTask(taskId);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to assign to task");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to assign to task:", err);
      const message = "Failed to assign to task";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Unassign current user from a task
  const unassignFromTask = async (taskId: string) => {
    try {
      setLoading(true);
      const result = await taskService.unassignFromTask(taskId);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to unassign from task");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to unassign from task:", err);
      const message = "Failed to unassign from task";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Mark task as completed
  const markTaskAsCompleted = async (taskId: string) => {
    try {
      setLoading(true);
      const result = await taskService.markTaskAsCompleted(taskId);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to mark task as completed");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to mark task as completed:", err);
      const message = "Failed to mark task as completed";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Mark task as uncompleted
  const markTaskAsUncompleted = async (taskId: string) => {
    try {
      setLoading(true);
      const result = await taskService.markTaskAsUncompleted(taskId);

      if (result.success) {
        await refreshTasks();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to mark task as uncompleted");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to mark task as uncompleted:", err);
      const message = "Failed to mark task as uncompleted";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Fetch tasks when household info changes
  useEffect(() => {
    if (householdInfo?.id) {
      refreshTasks();
    } else {
      setTasks([]);
    }
  }, [householdInfo?.id]);

  const value: TaskContextType = {
    tasks,
    loading,
    error,
    refreshTasks,
    createTask,
    editTask,
    deleteTask,
    assignToTask,
    unassignFromTask,
    markTaskAsCompleted,
    markTaskAsUncompleted,
    clearError,
  };

  return (
    <TaskContext.Provider value={value}>
      {children}
    </TaskContext.Provider>
  );
};
