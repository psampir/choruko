import "./BottomNav.scss";
import { useNavigate, useLocation } from "react-router-dom";
import houseBlackIcon from "../../assets/svg/house-black.svg";
import houseBlackFillIcon from "../../assets/svg/house-black-fill.svg";
import taskIcon from "../../assets/svg/task.svg";
import taskFillIcon from "../../assets/svg/task-fill.svg";
import settlementsIcon from "../../assets/svg/expenses.svg";
import settlementsFillIcon from "../../assets/svg/expenses-fill.svg";

interface BottomNavProps {
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}

const BottomNav: React.FC<BottomNavProps> = ({ activeTab, onTabChange }) => {
  const navigate = useNavigate();
  const location = useLocation();

  // Determine active tab from current route
  const getCurrentTab = () => {
    const path = location.pathname;
    if (path.includes("/tasks")) return "tasks";
    if (path.includes("/settlements")) return "settlements";
    if (path.includes("/profile")) return "home";
    return activeTab || "home";
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
      id: "home",
      icon: houseBlackIcon,
      iconActive: houseBlackFillIcon,
      label: "Home",
      route: "/leaderboard",
    },
    {
      id: "settlements",
      icon: settlementsIcon,
      iconActive: settlementsFillIcon,
      label: "settlements",
      route: "/settlements",
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
