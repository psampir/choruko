import { parseErrorMessage } from "../utils/errorUtils";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface InvitationResponse {
  id: string;
  householdId: string;
  householdName: string;
  inviterId: string;
  inviterFirstName: string;
  inviterLastName: string;
  inviteeEmail: string;
  createdAt: string;
  receiverId: string;
  householdImagePath: string;
}

class InvitationService {
  async createInvitation(householdId: string, userEmail: string) {
    try {
      const response = await fetch(
        `${apiBaseUrl}/Invitations/${householdId}/create?userEmail=${encodeURIComponent(userEmail)}`,
        {
          method: "POST",
          credentials: "include",
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      if (response.ok) {
        const result = await response.json();
        return {
          success: true,
          data: result,
          message: "Invitation sent successfully",
        };
      } else {
        const errorData = await response.json();
        return {
          success: false,
          message: parseErrorMessage(errorData),
        };
      }
    } catch (error) {
      return {
        success: false,
        message: "Network error. Please check your connection.",
      };
    }
  }

  async getAllInvitations() {
    try {
      const response = await fetch(`${apiBaseUrl}/Invitations/all`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const result = await response.json();
        return {
          success: true,
          data: result.result as InvitationResponse[],
        };
      } else {
        const errorData = await response.json();
        return {
          success: false,
          message: parseErrorMessage(errorData),
        };
      }
    } catch (error) {
      return {
        success: false,
        message: "Network error. Please check your connection.",
      };
    }
  }

  async getInvitation(invitationId: string) {
    try {
      const response = await fetch(
        `${apiBaseUrl}/Invitations/${invitationId}`,
        {
          method: "GET",
          credentials: "include",
        }
      );

      if (response.ok) {
        const result = await response.json();
        return {
          success: true,
          data: result.result as InvitationResponse,
        };
      } else {
        const errorData = await response.json();
        return {
          success: false,
          message: parseErrorMessage(errorData),
        };
      }
    } catch (error) {
      return {
        success: false,
        message: "Network error. Please check your connection.",
      };
    }
  }

  async acceptInvitation(invitationId: string) {
    try {
      const response = await fetch(
        `${apiBaseUrl}/Invitations/${invitationId}/accept`,
        {
          method: "PUT",
          credentials: "include",
        }
      );

      if (response.ok) {
        const result = await response.json();
        return {
          success: true,
          data: result,
          message: "Invitation accepted successfully",
        };
      } else {
        const errorData = await response.json();
        return {
          success: false,
          message: parseErrorMessage(errorData),
        };
      }
    } catch (error) {
      return {
        success: false,
        message: "Network error. Please check your connection.",
      };
    }
  }

  async declineInvitation(invitationId: string) {
    try {
      const response = await fetch(
        `${apiBaseUrl}/Invitations/${invitationId}/decline`,
        {
          method: "PUT",
          credentials: "include",
        }
      );

      if (response.ok) {
        const result = await response.json();
        return {
          success: true,
          data: result,
          message: "Invitation declined successfully",
        };
      } else {
        const errorData = await response.json();
        return {
          success: false,
          message: parseErrorMessage(errorData),
        };
      }
    } catch (error) {
      return {
        success: false,
        message: "Network error. Please check your connection.",
      };
    }
  }
}

export default new InvitationService();
export type { InvitationResponse };
