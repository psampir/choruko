import React, { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import { useAuth } from "./AuthContext";
import userService from "../services/userService";

// Define the User interface (matching the backend)
interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  role?: string;
  householdId?: string | null;
  profilePicturePath?: string | null;
}

// Define the context interface
interface UserContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  updateUserNames: (firstName: string, lastName: string) => Promise<{ success: boolean; message?: string }>;
  updatePassword: (newPassword: string, confirmNewPassword: string) => Promise<{ success: boolean; message?: string }>;
  updateProfilePicture: (file: File) => Promise<{ success: boolean; message?: string }>;
  deleteAccount: () => Promise<{ success: boolean; message?: string }>;
  refreshUser: () => Promise<void>;
  clearError: () => void;
}

// Create the context
const UserContext = createContext<UserContextType | undefined>(undefined);

// Custom hook to use the user context
export const useUser = () => {
  const context = useContext(UserContext);
  if (context === undefined) {
    throw new Error("useUser must be used within a UserProvider");
  }
  return context;
};

// UserProvider component props
interface UserProviderProps {
  children: ReactNode;
}

// UserProvider component
export const UserProvider: React.FC<UserProviderProps> = ({ children }) => {
  const { user: authUser, checkAuth, clearAuth } = useAuth();
  const [user, setUser] = useState<User | null>(authUser);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Sync user state with AuthContext
  useEffect(() => {
    setUser(authUser);
  }, [authUser]);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Refresh user data from the server
  const refreshUser = async () => {
    try {
      setLoading(true);
      setError(null);
      // Call checkAuth without showing loading state to avoid UI flicker
      await checkAuth(false);
    } catch (err) {
      console.error("Failed to refresh user data:", err);
      setError("Failed to refresh user data");
    } finally {
      setLoading(false);
    }
  };

  // Update user's first and last name
  const updateUserNames = async (firstName: string, lastName: string) => {
    try {
      setLoading(true);
      setError(null);

      const result = await userService.changeNames(firstName, lastName);

      if (result.success) {
        // Refresh user data after successful update
        await refreshUser();
        return { success: true, message: "Name updated successfully" };
      } else {
        setError(result.message);
        return { success: false, message: result.message };
      }
    } catch (err) {
      const errorMessage = "An error occurred while updating your name";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Update user's password
  const updatePassword = async (newPassword: string, confirmNewPassword: string) => {
    try {
      setLoading(true);
      setError(null);

      const result = await userService.changePassword(newPassword, confirmNewPassword);

      if (result.success) {
        return { success: true, message: "Password updated successfully" };
      } else {
        setError(result.message);
        return { success: false, message: result.message };
      }
    } catch (err) {
      const errorMessage = "An error occurred while updating your password";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Update user's profile picture
  const updateProfilePicture = async (file: File) => {
    try {
      setLoading(true);
      setError(null);

      const result = await userService.changePicture(file);

      if (result.success) {
        // Refresh user data after successful update
        await refreshUser();
        return { success: true, message: "Profile picture updated successfully" };
      } else {
        setError(result.message);
        return { success: false, message: result.message };
      }
    } catch (err) {
      const errorMessage = "An error occurred while updating your profile picture";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Delete user account
  const deleteAccount = async () => {
    try {
      setLoading(true);
      setError(null);

      const result = await userService.deleteSelf();

      if (result.success) {
        // Clear auth state without calling logout API (user is already deleted)
        clearAuth();
        return { success: true, message: "Account deleted successfully" };
      } else {
        setError(result.message);
        return { success: false, message: result.message };
      }
    } catch (err) {
      const errorMessage = "An error occurred while deleting your account";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Context value
  const value: UserContextType = {
    user,
    loading,
    error,
    updateUserNames,
    updatePassword,
    updateProfilePicture,
    deleteAccount,
    refreshUser,
    clearError,
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
};
