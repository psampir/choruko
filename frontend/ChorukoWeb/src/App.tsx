import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute/ProtectedRoute";
import AuthContainer from "./views/Auth/AuthContainer";
// import Dashboard from "./views/Dashboard/Dashboard";
import Profile from "./views/Profile/Profile";
import Tasks from "./views/Tasks/Tasks";
import Expenses from "./views/Expenses/Expenses";
// import NavbarMobile from "./components/NavbarMobile/NavbarMobile";
import { useAuth } from "./contexts/AuthContext";

// Layout component for protected pages
function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app">
      <main className="main-content">{children}</main>
      {/* <NavbarMobile /> */}
    </div>
  );
}

// Component to handle the root route intelligently
function RootRedirect() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return <Navigate to={isAuthenticated ? "/profile" : "/login"} replace />;
}

function AppRoutes() {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<AuthContainer />} />
      <Route path="/signup" element={<AuthContainer />} />

      {/* Protected routes with layout */}
      {/* <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Dashboard />
            </AppLayout>
          </ProtectedRoute>
        }
      /> */}

      <Route
        path="/tasks"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Tasks />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/expenses"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Expenses />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/profile"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Profile />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      {/* Smart default redirect */}
      <Route path="/" element={<RootRedirect />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <AppRoutes />
      </Router>
    </AuthProvider>
  );
}
export default App;
