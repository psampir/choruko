import React, { createContext, useContext, useState, useCallback, useEffect, useRef } from "react";
import type { ReactNode } from "react";
import { useHousehold } from "./HouseholdContext";
import settlementService from "../services/settlementService";
import type { SettlementListItem, CreateSettlementData, EditSettlementData } from "../services/settlementService";

// Define the context interface
interface SettlementContextType {
  settlements: SettlementListItem[];
  loading: boolean;
  error: string | null;
  refreshSettlements: () => Promise<void>;
  createSettlement: (data: CreateSettlementData) => Promise<{ success: boolean; message?: string }>;
  editSettlement: (settlementId: string, data: EditSettlementData) => Promise<{ success: boolean; message?: string }>;
  deleteSettlement: (settlementId: string) => Promise<{ success: boolean; message?: string }>;
  markAsPaid: (settlementId: string) => Promise<{ success: boolean; message?: string }>;
  markAsUnpaid: (settlementId: string) => Promise<{ success: boolean; message?: string }>;
  markAsDone: (settlementId: string) => Promise<{ success: boolean; message?: string }>;
  unmarkAsDone: (settlementId: string) => Promise<{ success: boolean; message?: string }>;
  clearError: () => void;
}

// Create the context
const SettlementContext = createContext<SettlementContextType | undefined>(undefined);

// Custom hook to use the settlement context
export const useSettlement = () => {
  const context = useContext(SettlementContext);
  if (context === undefined) {
    throw new Error("useSettlement must be used within a SettlementProvider");
  }
  return context;
};

// SettlementProvider component props
interface SettlementProviderProps {
  children: ReactNode;
}

// SettlementProvider component
export const SettlementProvider: React.FC<SettlementProviderProps> = ({ children }) => {
  const { householdInfo } = useHousehold();
  const [settlements, setSettlements] = useState<SettlementListItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const hasDataRef = useRef(false);

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Fetch settlements from the server
  const refreshSettlements = useCallback(async () => {
    if (!householdInfo?.id) {
      setSettlements([]);
      hasDataRef.current = false;
      return;
    }

    try {
      // Only show loading state if we don't have data yet
      if (!hasDataRef.current) {
        setLoading(true);
      }
      setError(null);

      const result = await settlementService.getAllSettlements(householdInfo.id);

      if (result.success && result.data) {
        setSettlements(result.data);
        hasDataRef.current = true;
      } else {
        setError(result.message || "Failed to fetch settlements");
        setSettlements([]);
        hasDataRef.current = false;
      }
    } catch (err) {
      console.error("Failed to fetch settlements:", err);
      setError("Failed to fetch settlements");
      setSettlements([]);
      hasDataRef.current = false;
    } finally {
      setLoading(false);
    }
  }, [householdInfo?.id]);

  // Create a new settlement
  const createSettlement = async (data: CreateSettlementData) => {
    if (!householdInfo?.id) {
      return { success: false, message: "No household found" };
    }

    try {
      setLoading(true);
      const result = await settlementService.createSettlement(householdInfo.id, data);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to create settlement");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to create settlement:", err);
      const message = "Failed to create settlement";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Edit an existing settlement
  const editSettlement = async (settlementId: string, data: EditSettlementData) => {
    try {
      setLoading(true);
      const result = await settlementService.editSettlement(settlementId, data);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to update settlement");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to update settlement:", err);
      const message = "Failed to update settlement";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Delete a settlement
  const deleteSettlement = async (settlementId: string) => {
    try {
      setLoading(true);
      const result = await settlementService.deleteSettlement(settlementId);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to delete settlement");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to delete settlement:", err);
      const message = "Failed to delete settlement";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Mark settlement as paid by current user
  const markAsPaid = async (settlementId: string) => {
    try {
      setLoading(true);
      const result = await settlementService.markAsPaid(settlementId);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to mark as paid");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to mark as paid:", err);
      const message = "Failed to mark as paid";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Mark settlement as unpaid by current user
  const markAsUnpaid = async (settlementId: string) => {
    try {
      setLoading(true);
      const result = await settlementService.markAsUnpaid(settlementId);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to mark as unpaid");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to mark as unpaid:", err);
      const message = "Failed to mark as unpaid";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Mark settlement as done (completed) by creator
  const markAsDone = async (settlementId: string) => {
    try {
      setLoading(true);
      const result = await settlementService.markAsDone(settlementId);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to complete settlement");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to complete settlement:", err);
      const message = "Failed to complete settlement";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Unmark settlement as done (reopen) by creator
  const unmarkAsDone = async (settlementId: string) => {
    try {
      setLoading(true);
      const result = await settlementService.unmarkAsDone(settlementId);

      if (result.success) {
        await refreshSettlements();
        return { success: true, message: result.message };
      } else {
        setError(result.message || "Failed to reopen settlement");
        return { success: false, message: result.message };
      }
    } catch (err) {
      console.error("Failed to reopen settlement:", err);
      const message = "Failed to reopen settlement";
      setError(message);
      return { success: false, message };
    } finally {
      setLoading(false);
    }
  };

  // Fetch settlements when household info changes
  useEffect(() => {
    if (householdInfo?.id) {
      refreshSettlements();
    } else {
      setSettlements([]);
    }
  }, [householdInfo?.id, refreshSettlements]);

  const value: SettlementContextType = {
    settlements,
    loading,
    error,
    refreshSettlements,
    createSettlement,
    editSettlement,
    deleteSettlement,
    markAsPaid,
    markAsUnpaid,
    markAsDone,
    unmarkAsDone,
    clearError,
  };

  return (
    <SettlementContext.Provider value={value}>
      {children}
    </SettlementContext.Provider>
  );
};
