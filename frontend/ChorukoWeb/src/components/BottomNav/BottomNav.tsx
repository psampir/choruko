import "./BottomNav.scss";
import { useNavigate, useLocation } from "react-router-dom";
import { useIsDarkTheme } from "../../utils/iconUtils";

// Light theme icons
import houseBlackIconLight from "../../assets/svg/light/house-black.svg";
import houseBlackFillIconLight from "../../assets/svg/light/house-black-fill.svg";
import taskIconLight from "../../assets/svg/light/task.svg";
import taskFillIconLight from "../../assets/svg/light/task-fill.svg";
import expensesIconLight from "../../assets/svg/light/expenses.svg";
import expensesFillIconLight from "../../assets/svg/light/expenses-fill.svg";

// Dark theme icons
import houseBlackIconDark from "../../assets/svg/dark/house-black.svg";
import houseBlackFillIconDark from "../../assets/svg/dark/house-black-fill.svg";
import taskIconDark from "../../assets/svg/dark/task.svg";
import taskFillIconDark from "../../assets/svg/dark/task-fill.svg";
import expensesIconDark from "../../assets/svg/dark/expenses.svg";
import expensesFillIconDark from "../../assets/svg/dark/expenses-fill.svg";

interface BottomNavProps {
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}

const BottomNav: React.FC<BottomNavProps> = ({ activeTab, onTabChange }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const isDark = useIsDarkTheme();

  // Select themed icons
  const houseBlackIcon = isDark ? houseBlackIconDark : houseBlackIconLight;
  const houseBlackFillIcon = isDark
    ? houseBlackFillIconDark
    : houseBlackFillIconLight;
  const taskIcon = isDark ? taskIconDark : taskIconLight;
  const taskFillIcon = isDark ? taskFillIconDark : taskFillIconLight;
  const expensesIcon = isDark ? expensesIconDark : expensesIconLight;
  const expensesFillIcon = isDark
    ? expensesFillIconDark
    : expensesFillIconLight;

  // Determine active tab from current route
  const getCurrentTab = () => {
    const path = location.pathname;
    if (path.includes("/tasks")) return "tasks";
    if (path.includes("/expenses")) return "expenses";
    if (path.includes("/leaderboard")) return "leaderboard";
    if (path.includes("/profile")) return "profile";
    return activeTab || "leaderboard";
  };

  const currentTab = getCurrentTab();
  const navItems = [
    {
      id: "tasks",
      icon: taskIcon,
      iconActive: taskFillIcon,
      label: "Tasks",
      route: "/tasks",
    },
    {
      id: "leaderboard",
      icon: houseBlackIcon,
      iconActive: houseBlackFillIcon,
      label: "Home",
      route: "/leaderboard",
    },
    {
      id: "expenses",
      icon: expensesIcon,
      iconActive: expensesFillIcon,
      label: "Expenses",
      route: "/expenses",
    },
  ];

  const handleTabClick = (tabId: string, route: string) => {
    // Call the optional onTabChange callback
    onTabChange?.(tabId);

    // Navigate to the route
    navigate(route);
  };

  return (
    <nav className="bottom-nav">
      {navItems.map((item) => {
        const isActive = currentTab === item.id;
        return (
          <button
            key={item.id}
            className={`bottom-nav__item ${
              isActive ? "bottom-nav__item--active" : ""
            }`}
            onClick={() => handleTabClick(item.id, item.route)}
            type="button"
          >
            <img
              src={isActive ? item.iconActive : item.icon}
              alt={item.label}
              className="bottom-nav__icon"
            />
          </button>
        );
      })}
    </nav>
  );
};

export default BottomNav;
