const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface CreateHouseholdData {
  name: string;
  profilePicture?: File;
}

interface HouseholdMember {
  id: string;
  firstName: string;
  lastName: string;
  profilePicturePath: string | null;
  points: number;
}

interface HouseholdInfo {
  id: string;
  name: string;
  adminId: string;
  adminFirstName: string;
  adminLastName: string;
  participantsCount: number;
  profilePictureUrl: string | null;
  members: HouseholdMember[];
}

class HouseholdService {
  // Create a new household
  async createHousehold(data: CreateHouseholdData) {
    try {
      const formData = new FormData();
      formData.append("Name", data.name);

      if (data.profilePicture) {
        formData.append("ProfilePicture", data.profilePicture);
      }

      const response = await fetch(`${apiBaseUrl}/Household/create`, {
        method: "POST",
        credentials: "include",
        body: formData,
      });

      if (response.ok) {
        const result = await response.json();
        return { success: true, data: result, message: "Household created successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to create household" };
      }
    } catch (error) {
      console.error("Error creating household:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get household information
  async getHouseholdInfo() {
    try {
      const response = await fetch(`${apiBaseUrl}/Household/household-info`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, data: data.result as HouseholdInfo };
        } else {
          return { success: false, message: "Failed to fetch household info" };
        }
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to fetch household info" };
      }
    } catch (error) {
      console.error("Error fetching household info:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Rename household
  async renameHousehold(newName: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Household/rename`, {
        method: "PATCH",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ newName }),
      });

      if (response.ok) {
        return { success: true, message: "Household renamed successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to rename household" };
      }
    } catch (error) {
      console.error("Error renaming household:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Change household picture
  async changeHouseholdPicture(file: File) {
    try {
      const formData = new FormData();
      formData.append("ProfilePicture", file);

      const response = await fetch(`${apiBaseUrl}/Household/change-picture`, {
        method: "PATCH",
        credentials: "include",
        body: formData,
      });

      if (response.ok) {
        return { success: true, message: "Household picture changed successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to change household picture" };
      }
    } catch (error) {
      console.error("Error changing household picture:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Delete household
  async deleteHousehold() {
    try {
      const response = await fetch(`${apiBaseUrl}/Household/delete`, {
        method: "DELETE",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true, message: "Household deleted successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to delete household" };
      }
    } catch (error) {
      console.error("Error deleting household:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

// Export a single instance
export default new HouseholdService();
export type { HouseholdInfo, HouseholdMember };
