const API_BASE_URL = "http://localhost:5000/api/Auth";

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
      const response = await fetch(`${API_BASE_URL}/register`, {
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
      const response = await fetch(`${API_BASE_URL}/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ email, password }),
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

  // Check authentication status
  async checkStatus() {
    try {
      const response = await fetch(`${API_BASE_URL}/userInfo`, {
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
      const response = await fetch(`${API_BASE_URL}/logOut`, {
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
      const response = await fetch(`${API_BASE_URL}/reset-password`, {
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
