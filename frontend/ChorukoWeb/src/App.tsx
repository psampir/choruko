import { lazy, Suspense } from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import { AuthProvider } from "./contexts/AuthContext";
import { UserProvider, useUser } from "./contexts/UserContext";
import { HouseholdProvider } from "./contexts/HouseholdContext";
import { ActivityProvider } from "./contexts/ActivityContext";
import { TaskProvider } from "./contexts/TaskContext";
import { SettlementProvider } from "./contexts/SettlementContext";
import { ThemeProvider } from "./contexts/ThemeContext";
import { PushNotificationProvider } from "./contexts/PushNotificationContext";
import ProtectedRoute from "./components/ProtectedRoute/ProtectedRoute";
import NavbarMobile from "./components/NavbarMobile/NavbarMobile";
import BottomNav from "./components/BottomNav/BottomNav";
import { useAuth } from "./contexts/AuthContext";
import PushNotificationHandler from "./components/PushNotificationHandler";

// Lazy load route components
const AuthContainer = lazy(() => import("./views/Auth/AuthContainer"));
const Tasks = lazy(() => import("./views/Household/Tasks/Tasks"));
const AddTask = lazy(() => import("./views/Household/Tasks/AddTask/AddTask"));
const Settlements = lazy(() => import("./views/Household/Settlements/Settlements"));
const AddSettlement = lazy(() => import("./views/Household/Settlements/AddSettlement/AddSettlement"));
const JoinOrCreateHousehold = lazy(() => import("./views/JoinOrCreateHousehold/JoinOrCreateHousehold"));
const Invitations = lazy(() => import("./views/JoinOrCreateHousehold/Invitations/Invitations"));
const CreateHousehold = lazy(() => import("./views/JoinOrCreateHousehold/CreateHousehold/CreateHousehold"));
const Leaderboard = lazy(() => import("./views/Household/Leaderboard/Leaderboard"));
const ManageAccount = lazy(() => import("./views/Profile/ManageAccount/ManageAccount"));
const ManageHousehold = lazy(() => import("./views/Profile/ManageHousehold/ManageHousehold"));
const Activities = lazy(() => import("./views/Profile/ManageHousehold/Activities/Activities"));
const TestNotifications = lazy(() => import("./views/TestNotifications"));

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
  const { user } = useUser();

  return (
    <div className="app">
      <main className="main-content">{children}</main>
      <NavbarMobile />
      {user?.householdId && <BottomNav />}
    </div>
  );
}

// Component to handle the root route intelligently
function RootRedirect() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return <Navigate to={isAuthenticated ? "/leaderboard" : "/login"} replace />;
}

function AppRoutes() {
  return (
    <Suspense fallback={<div className="loading">Loading...</div>}>
      <Routes>
        {/* Root route - must be first */}
        <Route path="/" element={<RootRedirect />} />

        {/* Public routes */}
        <Route path="/login" element={<AuthContainer />} />

        {/* Protected routes with layout */}
        <Route
          path="/joinOrCreateHousehold"
          element={
            <ProtectedRoute>
              <AppLayout>
                <JoinOrCreateHousehold />
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
          path="/tasks/add"
          element={
            <ProtectedRoute>
              <AddTask />
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
          path="/createHousehold"
          element={
            <ProtectedRoute>
              <AppLayout>
                <CreateHousehold />
              </AppLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/expenses"
          element={
            <ProtectedRoute>
              <MainAppLayout>
                <Settlements />
              </MainAppLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/settlements"
          element={
            <ProtectedRoute>
              <MainAppLayout>
                <Settlements />
              </MainAppLayout>
            </ProtectedRoute>
          }
        />

        <Route
          path="/settlements/add"
          element={
            <ProtectedRoute>
              <AddSettlement />
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

        {/* Test route for push notifications */}
        <Route
          path="/test-notifications"
          element={
            <ProtectedRoute>
              <AppLayout>
                <TestNotifications />
              </AppLayout>
            </ProtectedRoute>
          }
        />

        {/* Catch-all for unknown routes */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Suspense>
  );
}

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <UserProvider>
          <HouseholdProvider>
            <ActivityProvider>
              <TaskProvider>
                <SettlementProvider>
                  <PushNotificationProvider>
                    <Router>
                      <PushNotificationHandler />
                      <AppRoutes />
                    </Router>
                  </PushNotificationProvider>
                </SettlementProvider>
              </TaskProvider>
            </ActivityProvider>
          </HouseholdProvider>
        </UserProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}
export default App;
