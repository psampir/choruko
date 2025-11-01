import { parseErrorMessage } from "../utils/errorUtils";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface CreateActivityData {
  name: string;
  pointsAwarded: number;
}

interface EditActivityData {
  name: string;
  pointsAwarded: number;
}

interface Activity {
  id: string;
  name: string;
  pointsAwarded: number;
}

class ActivityService {
  // Create a new activity
  async createActivity(householdId: string, data: CreateActivityData) {
    try {
      const response = await fetch(`${apiBaseUrl}/Activity/create/${householdId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          name: data.name,
          pointsAwarded: data.pointsAwarded,
        }),
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, data: result, message: "Activity created successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to create activity" };
      }
    } catch (error) {
      console.error("Error creating activity:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Edit an activity
  async editActivity(householdId: string, activityId: string, data: EditActivityData) {
    try {
      const response = await fetch(`${apiBaseUrl}/Activity/edit/${householdId}/${activityId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          name: data.name,
          pointsAwarded: data.pointsAwarded,
        }),
      });

      if (response.ok) {
        return { success: true, message: "Activity updated successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to update activity" };
      }
    } catch (error) {
      console.error("Error updating activity:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Delete an activity
  async deleteActivity(activityId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Activity/delete/${activityId}`, {
        method: "DELETE",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Activity deleted successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to delete activity" };
      }
    } catch (error) {
      console.error("Error deleting activity:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get all activities for a household
  async getActivities(householdId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Activity/${householdId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as Activity[] };
        } else {
          return { success: false, message: "Failed to fetch activities" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch activities" };
      }
    } catch (error) {
      console.error("Error fetching activities:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get a specific activity by ID
  async getActivity(householdId: string, activityId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Activity/${householdId}/${activityId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as Activity };
        } else {
          return { success: false, message: "Failed to fetch activity" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch activity" };
      }
    } catch (error) {
      console.error("Error fetching activity:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

// Export a single instance
export default new ActivityService();
export type { Activity, CreateActivityData, EditActivityData };
