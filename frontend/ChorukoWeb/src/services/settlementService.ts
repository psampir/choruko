import { parseErrorMessage } from "../utils/errorUtils";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface SettlementUser {
  id: string;
  firstName: string;
  lastName: string;
  profilePicturePath: string | null;
}

// Response from /all endpoint (without users list)
export interface SettlementListItem {
  id: string;
  householdId: string;
  creatorId: string;
  creator: SettlementUser;
  dateOfCreation: string;
  description: string;
  amount: number;
  status: "Pending" | "Completed";
  settledUserIds?: string[];
  unsettledUserIds?: string[];
}

// Response from single settlement endpoint (with users list)
export interface Settlement {
  id: string;
  householdId: string;
  creatorId: string;
  creator: SettlementUser;
  dateOfCreation: string;
  description: string;
  amount: number;
  status: "Pending" | "Completed";
  unsettledUsers?: SettlementUser[];
  settledUsers?: SettlementUser[];
}

interface CreateSettlementData {
  description: string;
  amount: number;
  unsettledUsers: string[];
}

interface EditSettlementData {
  description: string;
  amount: number;
  unsettledUsers: string[];
}

class SettlementService {
  // Get all settlements for a household
  async getAllSettlements(householdId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/all/${householdId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as SettlementListItem[] };
        } else {
          return { success: false, message: "Failed to fetch settlements" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch settlements" };
      }
    } catch (error) {
      console.error("Error fetching settlements:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get a specific settlement by ID
  async getSettlement(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/${settlementId}`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as Settlement };
        } else {
          return { success: false, message: "Failed to fetch settlement" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to fetch settlement" };
      }
    } catch (error) {
      console.error("Error fetching settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Create a new settlement
  async createSettlement(householdId: string, data: CreateSettlementData) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/create/${householdId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Settlement created successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to create settlement" };
      }
    } catch (error) {
      console.error("Error creating settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Edit an existing settlement
  async editSettlement(settlementId: string, data: EditSettlementData) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/edit/${settlementId}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(data),
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Settlement updated successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to update settlement" };
      }
    } catch (error) {
      console.error("Error updating settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Delete a settlement
  async deleteSettlement(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/delete/${settlementId}`, {
        method: "DELETE",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Settlement deleted successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to delete settlement" };
      }
    } catch (error) {
      console.error("Error deleting settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Mark settlement as paid by current user
  async markAsPaid(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/pay/${settlementId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Marked as paid" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to mark as paid" };
      }
    } catch (error) {
      console.error("Error marking as paid:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Mark settlement as unpaid by current user
  async markAsUnpaid(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/unPay/${settlementId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Marked as unpaid" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to mark as unpaid" };
      }
    } catch (error) {
      console.error("Error marking as unpaid:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Mark settlement as done (completed) by creator
  async markAsDone(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/markDone/${settlementId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Settlement completed" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to complete settlement" };
      }
    } catch (error) {
      console.error("Error completing settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Unmark settlement as done (reopen) by creator
  async unmarkAsDone(settlementId: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Settlement/unmarkDone/${settlementId}`, {
        method: "PUT",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, message: result.message || "Settlement reopened" };
      } else {
        const errorText = await response.text();
        return { success: false, message: parseErrorMessage(errorText) || "Failed to reopen settlement" };
      }
    } catch (error) {
      console.error("Error reopening settlement:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

const settlementService = new SettlementService();
export default settlementService;
export type { CreateSettlementData, EditSettlementData };
