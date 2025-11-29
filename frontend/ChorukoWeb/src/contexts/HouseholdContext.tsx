import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from "react";
import type { ReactNode } from "react";
import { useAuth } from "./AuthContext";
import householdService from "../services/householdService";
import type { HouseholdInfo, HouseholdParticipant } from "../services/householdService";

// Define the context interface
interface HouseholdContextType {
  householdInfo: HouseholdInfo | null;
  participants: HouseholdParticipant[];
  loading: boolean;
  error: string | null;
  refreshHousehold: () => Promise<void>;
  clearError: () => void;
  showAdminPromotionPopup: boolean;
  dismissAdminPromotionPopup: () => void;
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
  const { hasHousehold, checkAuth, user } = useAuth();
  const [householdInfo, setHouseholdInfo] = useState<HouseholdInfo | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showAdminPromotionPopup, setShowAdminPromotionPopup] = useState(false);
  const hasDataRef = useRef(false);
  const previousAdminIdRef = useRef<string | null>(null);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Dismiss admin promotion popup
  const dismissAdminPromotionPopup = () => {
    setShowAdminPromotionPopup(false);
  };

  // Fetch household data from the server
  const refreshHousehold = useCallback(async () => {
    if (!hasHousehold) {
      setHouseholdInfo(null);
      hasDataRef.current = false;
      return;
    }

    try {
      // Only show loading state if we don't have data yet
      if (!hasDataRef.current) {
        setLoading(true);
      }
      setError(null);

      const result = await householdService.getHouseholdInfo();

      if (result.success && result.data) {
        // Check if current user just became admin
        const newAdminId = result.data.adminId;
        const wasNotAdminBefore = previousAdminIdRef.current && previousAdminIdRef.current !== user?.id;
        const isAdminNow = newAdminId === user?.id;
        
        if (wasNotAdminBefore && isAdminNow) {
          // User just became admin, show popup
          setShowAdminPromotionPopup(true);
        }
        
        previousAdminIdRef.current = newAdminId;
        setHouseholdInfo(result.data);
        hasDataRef.current = true;
      } else {
        // If backend indicates user has no household, refresh auth state
        if (result.message?.includes("not part of any household")) {
          await checkAuth(false);
        }
        setError(result.message || "Failed to fetch household info");
        setHouseholdInfo(null);
        hasDataRef.current = false;
      }
    } catch (err) {
      console.error("Failed to fetch household data:", err);
      setError("Failed to fetch household data");
      setHouseholdInfo(null);
      hasDataRef.current = false;
    } finally {
      setLoading(false);
    }
  }, [hasHousehold, checkAuth]);

  // Fetch household data when user has a household
  useEffect(() => {
    if (hasHousehold) {
      refreshHousehold();
    } else {
      setHouseholdInfo(null);
    }
  }, [hasHousehold]);

  // Extract participants from householdInfo
  const participants = householdInfo?.participants ?? [];

  const value: HouseholdContextType = {
    householdInfo,
    participants,
    loading,
    error,
    refreshHousehold,
    clearError,
    showAdminPromotionPopup,
    dismissAdminPromotionPopup,
  };

  return (
    <HouseholdContext.Provider value={value}>
      {children}
    </HouseholdContext.Provider>
  );
};
