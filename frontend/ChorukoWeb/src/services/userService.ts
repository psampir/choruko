const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface ChangeNamesData {
  firstName: string;
  lastName: string;
}

interface ChangePasswordData {
  newPassword: string;
  confirmNewPassword: string;
}

class UserService {
  // Change user's first and last name
  async changeNames(firstName: string, lastName: string) {
    try {
      const requestBody: ChangeNamesData = {
        firstName,
        lastName,
      };

      const response = await fetch(`${apiBaseUrl}/User/change-names`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(requestBody),
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

  // Change user's password
  async changePassword(newPassword: string, confirmNewPassword: string) {
    try {
      const requestBody: ChangePasswordData = {
        newPassword,
        confirmNewPassword,
      };

      const response = await fetch(`${apiBaseUrl}/User/change-password`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(requestBody),
      });

      if (response.ok) {
        return { success: true, message: "Password changed successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to change password" };
      }
    } catch (error) {
      console.error("Error changing password:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Change user's profile picture
  async changePicture(file: File) {
    try {
      const formData = new FormData();
      formData.append("ProfilePicture", file);

      const response = await fetch(`${apiBaseUrl}/User/change-picture`, {
        method: "PUT",
        credentials: "include",
        body: formData,
      });

      if (response.ok) {
        return { success: true, message: "Profile picture changed successfully" };
      } else {
        const errorText = await response.text();
        return { success: false, message: errorText || "Failed to change profile picture" };
      }
    } catch (error) {
      console.error("Error changing profile picture:", error);
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
