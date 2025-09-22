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
      console.log("📡 Sending login request...");
      const response = await fetch(`${apiBaseUrl}/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email, password }),
      });

      console.log("📨 Login response status:", response.status);
      console.log("📨 Login response headers:", Object.fromEntries(response.headers.entries()));

      const responseText = await response.text();
      console.log("📨 Login response body:", responseText);

      if (response.ok) {
        try {
          const data = JSON.parse(responseText);
          console.log("📨 Parsed login response:", data);

          // Check if the actual login succeeded
          if (data.result && data.result.succeeded) {
            console.log("✅ Login successful!");
            return { success: true, message: "Login successful" };
          } else {
            console.log("❌ Login failed - invalid credentials");
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
        console.log("❌ Login request failed with status:", response.status);
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
      const response = await fetch(`${apiBaseUrl}/userInfo`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const userInfo = await response.json();
        return { authenticated: true, user: userInfo };
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
      const response = await fetch(`${apiBaseUrl}/logOut`, {
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

  // Reset password (if you have this endpoint)
  async resetPassword(email: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/reset-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email }),
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
}

// Export a single instance
export default new AuthService();
