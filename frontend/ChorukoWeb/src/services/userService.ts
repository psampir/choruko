const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface ChangeNamesData {
  firstName: string;
  lastName: string;
}

class UserService {
  // Change user's first and last name
  async changeNames(firstName: string, lastName: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/User/change-names`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          firstName,
          lastName,
        }),
      });

      if (response.ok) {
        return { success: true, message: "Name changed successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to change name" };
      }
    } catch (error) {
      console.error("Error changing name:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Get user info (already handled in authService, but can be added here for completeness)
  async getUserInfo() {
    try {
      const response = await fetch(`${apiBaseUrl}/User/user-info`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        if (data.isSuccessful && data.result) {
          return { success: true, user: data.result };
        } else {
          return { success: false, message: "Failed to fetch user info" };
        }
      } else {
        return { success: false, message: "Failed to fetch user info" };
      }
    } catch (error) {
      console.error("Error fetching user info:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

// Export a single instance
export default new UserService();
