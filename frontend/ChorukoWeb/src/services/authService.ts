const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface UserData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

class AuthService {
  // Register a new user
  async register(userData: UserData) {
    try {
      const response = await fetch(`${apiBaseUrl}/auth/register`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          firstName: userData.firstName,
          lastName: userData.lastName,
          email: userData.email,
          password: userData.password,
          confirmPassword: userData.confirmPassword,
        }),
      });

      const message = await response.text();

      if (response.ok) {
        return { success: true, message };
      } else {
        return { success: false, message };
      }
    } catch (error) {
      return { success: false, message: "Network error occurred" };
    }
  }

  // Login user
  async login(email: string, password: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email, password }),
      });

      const responseText = await response.text();

      if (response.ok) {
        try {
          const data = JSON.parse(responseText);

          // Check if the actual login succeeded
          if (data.result && data.result.succeeded) {
            return { success: true, message: "Login successful" };
          } else {
            return {
              success: false,
              message: data.message || "Invalid email or password"
            };
          }
        } catch (parseError) {
          console.error("💥 Failed to parse login response:", parseError);
          return { success: false, message: "Failed to parse server response" };
        }
      } else {
        return { success: false, message: responseText };
      }
    } catch (error) {
      console.error("💥 Login request error:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Check authentication status
  async checkStatus() {
    try {
      const response = await fetch(`${apiBaseUrl}/User/user-info`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        // The API returns { isSuccessful: boolean, result: UserInfoResponse }
        if (data.isSuccessful && data.result) {
          return {
            authenticated: true,
            user: data.result
          };
        } else {
          return { authenticated: false };
        }
      } else {
        return { authenticated: false };
      }
    } catch (error) {
      return { authenticated: false };
    }
  }

  // Logout user
  async logout() {
    try {
      const response = await fetch(`${apiBaseUrl}/Auth/logout`, {
        method: "POST",
        credentials: "include",
      });

      if (response.ok) {
        return { success: true };
      } else {
        return { success: false };
      }
    } catch (error) {
      return { success: false };
    }
  }
}

// Export a single instance
export default new AuthService();
