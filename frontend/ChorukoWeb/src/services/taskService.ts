import { parseErrorMessage } from "../utils/errorUtils";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface TaskAssignee {
  id: string;
  firstName: string;
  lastName: string;
  profilePicturePath: string | null;
}

interface TaskCreator {
  id: string;
  firstName: string;
  lastName: string;
  profilePicturePath: string | null;
}

interface TaskActivity {
  id: string;
  name: string;
  pointsAwarded: number;
}

export interface HouseholdTask {
  id: string;
  householdId: string;
  activityId: string;
  activity: TaskActivity;
  creatorId: string;
  creator: TaskCreator;
  creationDate: string;
  deadLine: string;
  status: "Unassigned" | "Assigned" | "Completed";
  participantsAmount: number;
  name: string;
  description: string;
  assignees: TaskAssignee[];
}

interface CreateTaskData {
  name: string;
  description: string;
  activityId: string;
  deadLine: string;
  participantsAmount: number;
  assigneeIds?: string[];
}

interface EditTaskData {
  name: string;
  description: string;
  activityId: string;
  deadLine: string;
  participantsAmount: number;
  assigneeIds?: string[];
}

class TaskService {
  // Get all tasks for a household
  async getAllTasks(householdId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/all/${householdId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as HouseholdTask[] };
        } else {
          return { success: false, message: "Failed to fetch tasks" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch tasks" };
      }
    } catch (error) {
      console.error("Error fetching tasks:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get a specific task by ID
  async getTask(taskId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/${taskId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as HouseholdTask };
        } else {
          return { success: false, message: "Failed to fetch task" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch task" };
      }
    } catch (error) {
      console.error("Error fetching task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Create a new task
  async createTask(householdId: string, data: CreateTaskData) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/create/${householdId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          name: data.name,
          description: data.description,
          activityId: data.activityId,
          deadLine: data.deadLine,
          participantsAmount: data.participantsAmount,
          assigneeIds: data.assigneeIds || [],
        }),
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, data: result, message: "Task created successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to create task" };
      }
    } catch (error) {
      console.error("Error creating task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Edit a task
  async editTask(taskId: string, data: EditTaskData) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/edit/${taskId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          name: data.name,
          description: data.description,
          activityId: data.activityId,
          deadLine: data.deadLine,
          participantsAmount: data.participantsAmount,
          assigneeIds: data.assigneeIds || [],
        }),
      });

      if (response.ok) {
        return { success: true, message: "Task updated successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to update task" };
      }
    } catch (error) {
      console.error("Error updating task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Delete a task
  async deleteTask(taskId: string) {
    try {
      console.log(`Attempting to delete task with ID: ${taskId}`);
      
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/delete/${taskId}`, {
        method: "DELETE",
        credentials: "include",
      });


      if (response.ok) {
        return { success: true, message: "Task deleted successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to delete task" };
      }
    } catch (error) {
      console.error("Error deleting task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Assign current user to a task
  async assignToTask(taskId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/assign/${taskId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Assigned to task successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to assign to task" };
      }
    } catch (error) {
      console.error("Error assigning to task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Unassign current user from a task
  async unassignFromTask(taskId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/unassign/${taskId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Unassigned from task successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to unassign from task" };
      }
    } catch (error) {
      console.error("Error unassigning from task:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Mark task as completed
  async markTaskAsCompleted(taskId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/completed/${taskId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Task marked as completed" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to mark task as completed" };
      }
    } catch (error) {
      console.error("Error marking task as completed:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Mark task as uncompleted
  async markTaskAsUncompleted(taskId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/HouseholdTask/uncompleted/${taskId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Task marked as uncompleted" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to mark task as uncompleted" };
      }
    } catch (error) {
      console.error("Error marking task as uncompleted:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

// Export a single instance
export default new TaskService();
export type { CreateTaskData, EditTaskData, TaskAssignee, TaskCreator, TaskActivity };
