import React, { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import { useAuth } from "./AuthContext";
import householdService from "../services/householdService";
import type { HouseholdInfo, HouseholdMember } from "../services/householdService";

// Define the context interface
interface HouseholdContextType {
  householdInfo: HouseholdInfo | null;
  members: HouseholdMember[];
  loading: boolean;
  error: string | null;
  refreshHousehold: () => Promise<void>;
  clearError: () => void;
}

// Create the context
const HouseholdContext = createContext<HouseholdContextType | undefined>(undefined);

// Custom hook to use the household context
export const useHousehold = () => {
  const context = useContext(HouseholdContext);
  if (context === undefined) {
    throw new Error("useHousehold must be used within a HouseholdProvider");
  }
  return context;
};

// HouseholdProvider component props
interface HouseholdProviderProps {
  children: ReactNode;
}

// HouseholdProvider component
export const HouseholdProvider: React.FC<HouseholdProviderProps> = ({ children }) => {
  const { hasHousehold, checkAuth } = useAuth();
  const [householdInfo, setHouseholdInfo] = useState<HouseholdInfo | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Fetch household data from the server
  const refreshHousehold = async () => {
    if (!hasHousehold) {
      setHouseholdInfo(null);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      const result = await householdService.getHouseholdInfo();
      
      if (result.success && result.data) {
        setHouseholdInfo(result.data);
      } else {
        // If backend indicates user has no household, refresh auth state
        if (result.message?.includes("not part of any household")) {
          console.log("Backend indicates user has no household, refreshing auth state...");
          await checkAuth(false);
        }
        setError(result.message || "Failed to fetch household info");
        setHouseholdInfo(null);
      }
    } catch (err) {
      console.error("Failed to fetch household data:", err);
      setError("Failed to fetch household data");
      setHouseholdInfo(null);
    } finally {
      setLoading(false);
    }
  };

  // Fetch household data when user has a household
  useEffect(() => {
    if (hasHousehold) {
      refreshHousehold();
    } else {
      setHouseholdInfo(null);
    }
  }, [hasHousehold]);

  // Extract members from householdInfo
  const members = householdInfo?.members ?? [];

  const value: HouseholdContextType = {
    householdInfo,
    members,
    loading,
    error,
    refreshHousehold,
    clearError,
  };

  return (
    <HouseholdContext.Provider value={value}>
      {children}
    </HouseholdContext.Provider>
  );
};
