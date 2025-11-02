import React, { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import authService from "../services/authService";

// Define the User interface based on your backend
interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  role?: string;
  householdId?: string | null;
  profilePicturePath?: string | null;
  // Add other user properties from your backend as needed
}

// Define the context interface
interface AuthContextType {
  user: User | null;
  loading: boolean;
  isAuthenticated: boolean;
  hasHousehold: boolean;
  login: (
    email: string,
    password: string
  ) => Promise<{ success: boolean; message?: string; hasHousehold?: boolean }>;
  register: (
    userData: RegisterData
  ) => Promise<{ success: boolean; message?: string }>;
  logout: () => Promise<void>;
  clearAuth: () => void;
  checkAuth: (showLoading?: boolean) => Promise<void>;
  error: string | null;
  clearError: () => void;
  onBeforeLogout: (callback: () => Promise<void> | void) => () => void;
}

interface RegisterData {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

// Create the context
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Custom hook to use the auth context
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

// AuthProvider component props
interface AuthProviderProps {
  children: ReactNode;
}

// AuthProvider component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [beforeLogoutCallbacks] = useState<Set<() => Promise<void> | void>>(new Set());

  // Derived state
  const isAuthenticated = user !== null;
  const hasHousehold = user?.householdId !== null && user?.householdId !== undefined;

  // Clear error function
  const clearError = () => {
    setError(null);
  };

  // Check authentication status
  const checkAuth = async (showLoading = true) => {
    try {
      if (showLoading) setLoading(true);
      const result = await authService.checkStatus();

      if (result.authenticated && result.user) {
        setUser(result.user);
      } else {
        setUser(null);
      }
    } catch (error) {
      console.error("Auth check failed:", error);
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  // Login function
  const login = async (email: string, password: string) => {
    try {
      setLoading(true);
      setError(null);

      console.log("🔐 Starting login process...");
      const result = await authService.login(email, password);
      console.log("📨 Login response:", result);

      if (result.success) {
        console.log("✅ Login successful, waiting before checking auth...");
        // Wait a moment for the cookie to be set properly
        await new Promise(resolve => setTimeout(resolve, 100));

        console.log("🔍 Now checking authentication status...");
        const authResult = await authService.checkStatus();

        if (authResult.authenticated && authResult.user) {
          setUser(authResult.user);
          return {
            success: true,
            message: result.message,
            hasHousehold: authResult.user.householdId !== null && authResult.user.householdId !== undefined
          };
        } else {
          return { success: false, message: "Authentication check failed" };
        }
      } else {
        setError(result.message || "Login failed");
        return { success: false, message: result.message };
      }
    } catch (error) {
      const errorMessage = "An unexpected error occurred during login";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Register function
  const register = async (userData: RegisterData) => {
    console.log("🔄 AuthContext register called with:");
    try {
      setLoading(true);
      setError(null);

      console.log("📡 Calling authService.register...");
      const result = await authService.register(userData);
      console.log("📨 AuthService response:", result);

      if (result.success) {
        console.log("✅ Registration successful in AuthContext");
        // Optionally auto-login after registration
        await login(userData.email, userData.password);
        return { success: true, message: result.message };
      } else {
        console.log("❌ Registration failed:", result.message);
        setError(result.message || "Registration failed");
        return { success: false, message: result.message };
      }
    } catch (error) {
      console.error("💥 AuthContext register error:", error);
      const errorMessage = "An unexpected error occurred during registration";
      setError(errorMessage);
      return { success: false, message: errorMessage };
    } finally {
      setLoading(false);
    }
  };

  // Register callback to be called before logout
  const onBeforeLogout = (callback: () => Promise<void> | void) => {
    beforeLogoutCallbacks.add(callback);
    // Return cleanup function
    return () => {
      beforeLogoutCallbacks.delete(callback);
    };
  };

  // Logout function
  const logout = async () => {
    try {
      setLoading(true);
      
      // Call all registered callbacks before logout
      console.log('🔄 Running pre-logout callbacks...');
      const callbacks = Array.from(beforeLogoutCallbacks);
      await Promise.all(callbacks.map(cb => Promise.resolve(cb())));
      console.log('✅ Pre-logout callbacks completed');
      
      await authService.logout();
      setUser(null);
      setError(null);

      // Optionally redirect to login page
      // window.location.href = '/';
    } catch (error) {
      console.error("Logout error:", error);
      // Even if logout fails on server, clear local state
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  // Clear auth state without calling API (useful for account deletion)
  const clearAuth = () => {
    setUser(null);
    setError(null);
  };

  // Check auth on component mount
  useEffect(() => {
    checkAuth();
  }, []);

  // Context value
  const value: AuthContextType = {
    user,
    loading,
    isAuthenticated,
    hasHousehold,
    login,
    register,
    logout,
    clearAuth,
    checkAuth,
    error,
    clearError,
    onBeforeLogout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

// Optional: Protected Route component
interface ProtectedRouteProps {
  children: ReactNode;
  fallback?: ReactNode;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  fallback = <div>Please log in to access this page.</div>,
}) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          height: "100vh",
        }}
      >
        <div>Loading...</div>
      </div>
    );
  }

  return isAuthenticated ? <>{children}</> : <>{fallback}</>;
};

// Optional: User role checker component
interface RoleProtectedProps {
  children: ReactNode;
  allowedRoles: string[];
  fallback?: ReactNode;
}

export const RoleProtected: React.FC<RoleProtectedProps> = ({
  children,
  allowedRoles,
  fallback = <div>You don't have permission to access this page.</div>,
}) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user?.role) {
    return <>{fallback}</>;
  }

  const hasPermission = allowedRoles.includes(user.role);
  return hasPermission ? <>{children}</> : <>{fallback}</>;
};
