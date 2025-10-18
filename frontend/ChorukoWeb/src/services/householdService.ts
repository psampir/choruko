const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface CreateHouseholdData {
  name: string;
  profilePicture?: File;
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
}

// Export a single instance
export default new HouseholdService();
