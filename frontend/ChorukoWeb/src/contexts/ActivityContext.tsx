import React, { createContext, useContext, useState, useCallback } from "react";
import type { ReactNode } from "react";
import { useHousehold } from "./HouseholdContext";
import activityService from "../services/activityService";
import type { Activity } from "../services/activityService";

// Define the context interface
interface ActivityContextType {
  activities: Activity[];
  loading: boolean;
  error: string | null;
  refreshActivities: () => Promise<void>;
  createActivity: (name: string, pointsAwarded: number) => Promise<{ success: boolean; message?: string }>;
  editActivity: (activityId: string, name: string, pointsAwarded: number) => Promise<{ success: boolean; message?: string }>;
  deleteActivity: (activityId: string) => Promise<{ success: boolean; message?: string }>;
  clearError: () => void;
}

// Create the context
const ActivityContext = createContext<ActivityContextType | undefined>(undefined);

// Custom hook to use the activity context
export const useActivity = () => {
  const context = useContext(ActivityContext);
  if (context === undefined) {
    throw new Error("useActivity must be used within an ActivityProvider");
  }
  return context;
};

// ActivityProvider component props
interface ActivityProviderProps {
  children: ReactNode;
}

// ActivityProvider component
export const ActivityProvider: React.FC<ActivityProviderProps> = ({ children }) => {
  const { householdInfo } = useHousehold();
  const [activities, setActivities] = useState<Activity[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Fetch activities from the server
  const refreshActivities = useCallback(async () => {
    if (!householdInfo?.id) {
      setActivities([]);
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const result = await activityService.getActivities(householdInfo.id);

      if (result.success && result.data) {
        setActivities(result.data);
      } else {
        setError(result.message || "Failed to fetch activities");
        setActivities([]);
      }
    } catch (err) {
      console.error("Failed to fetch activities:", err);
      setError("Failed to fetch activities");
      setActivities([]);
    } finally {
      setLoading(false);
    }
  }, [householdInfo?.id]);

  // Create a new activity
  const createActivity = async (name: string, pointsAwarded: number) => {
    if (!householdInfo?.id) {
      return { success: false, message: "No household found" };
    }

    try {
      setLoading(true);
      const result = await activityService.createActivity(householdInfo.id, { name, pointsAwarded });

      if (result.success) {
        await refreshActivities();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to create activity");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to create activity:", err);
      const message = "Failed to create activity";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Edit an existing activity
  const editActivity = async (activityId: string, name: string, pointsAwarded: number) => {
    if (!householdInfo?.id) {
      return { success: false, message: "No household found" };
    }

    try {
      setLoading(true);
      const result = await activityService.editActivity(householdInfo.id, activityId, { name, pointsAwarded });

      if (result.success) {
        await refreshActivities();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to update activity");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to update activity:", err);
      const message = "Failed to update activity";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Delete an activity
  const deleteActivity = async (activityId: string) => {
    try {
      setLoading(true);
      const result = await activityService.deleteActivity(activityId);

      if (result.success) {
        await refreshActivities();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to delete activity");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to delete activity:", err);
      const message = "Failed to delete activity";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  const value: ActivityContextType = {
    activities,
    loading,
    error,
    refreshActivities,
    createActivity,
    editActivity,
    deleteActivity,
    clearError,
  };

  return (
    <ActivityContext.Provider value={value}>
      {children}
    </ActivityContext.Provider>
  );
};
