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
        // Backend returned error (401, 403, etc.) - user is not authenticated
        return { authenticated: false };
      }
    } catch (error) {
      // Network error (offline, timeout, DNS error, etc.)
      // Return error flag so AuthContext can keep cached user
      return { authenticated: false, networkError: true };
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

  // Forgot password - sends reset email
  async forgotPassword(email: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Auth/forgot-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email }),
      });

      const responseText = await response.text();

      if (response.ok) {
        try {
          const data = JSON.parse(responseText);
          return { 
            success: true, 
            message: data.message || "If an account exists for this email, a password reset link has been sent."
          };
        } catch (parseError) {
          return { 
            success: true, 
            message: "If an account exists for this email, a password reset link has been sent."
          };
        }
      } else {
        return { success: false, message: responseText };
      }
    } catch (error) {
      console.error("Forgot password request error:", error);
      return { success: false, message: "Network error occurred" };
    }
  }

  // Reset password with token
  async resetPassword(email: string, token: string, newPassword: string, confirmPassword: string) {
    try {
      const response = await fetch(`${apiBaseUrl}/Auth/reset-password`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ 
          email, 
          token, 
          newPassword, 
          confirmPassword 
        }),
      });

      const responseText = await response.text();

      if (response.ok) {
        try {
          const data = JSON.parse(responseText);
          return { 
            success: true, 
            message: data.message || "Password has been successfully reset."
          };
        } catch (parseError) {
          return { 
            success: true, 
            message: "Password has been successfully reset."
          };
        }
      } else {
        return { success: false, message: responseText };
      }
    } catch (error) {
      console.error("Reset password request error:", error);
      return { success: false, message: "Network error occurred" };
    }
  }
}

// Export a single instance
export default new AuthService();
