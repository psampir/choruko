import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute/ProtectedRoute";
import AuthContainer from "./views/Auth/AuthContainer";
import Tasks from "./views/Household/Tasks/Tasks";
import Expenses from "./views/Household/Expenses/Expenses";
import NavbarMobile from "./components/NavbarMobile/NavbarMobile";
import BottomNav from "./components/BottomNav/BottomNav";
import { useAuth } from "./contexts/AuthContext";
import JoinOrCreateHousehold from "./views/JoinOrCreateHousehold/JoinOrCreateHousehold";
import Invitations from "./views/JoinOrCreateHousehold/Invitations/Invitations";
import Leaderboard from "./views/Household/Leaderboard/Leaderboard";
import ManageAccount from "./views/Profile/ManageAccount/ManageAccount";
import ManageHousehold from "./views/Profile/ManageHousehold/ManageHousehold";
import Activities from "./views/Profile/ManageHousehold/Activities/Activities";

// Layout component for protected pages with top navbar
function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app">
      <main className="main-content">{children}</main>
      <NavbarMobile />
    </div>
  );
}

// Layout component for main app pages with bottom navigation
function MainAppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="app">
      <main className="main-content">{children}</main>
      <NavbarMobile />
      <BottomNav />
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
      <Route
        path="/joinOrCreateHousehold"
        element={
          <ProtectedRoute>
            <AppLayout>
              <JoinOrCreateHousehold
                onJoinHousehold={() => {}}
                onCreateHousehold={() => {}}
              />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/tasks"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <Tasks />
            </MainAppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/invitations"
        element={
          <ProtectedRoute>
            <AppLayout>
              <Invitations />
            </AppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/expenses"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <Expenses />
            </MainAppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/leaderboard"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <Leaderboard />
            </MainAppLayout>
          </ProtectedRoute>
        }
      />

      <Route
        path="/manageAccount"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <ManageAccount />
            </MainAppLayout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/manageHousehold"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <ManageHousehold />
            </MainAppLayout>
          </ProtectedRoute>
        }
      />

       <Route
        path="/activities"
        element={
          <ProtectedRoute>
            <MainAppLayout>
              <Activities />
            </MainAppLayout>
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
