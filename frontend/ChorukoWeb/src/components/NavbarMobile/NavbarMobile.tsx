import "./NavbarMobile.scss";
import { useState } from "react";
import Button from "../Button/Button";
import Popup from "../Popup";
import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { useUser } from "../../contexts/UserContext";
import { useHousehold } from "../../contexts/HouseholdContext";
import { useTheme } from "../../contexts/ThemeContext";
import { useIsDarkTheme } from "../../utils/iconUtils";
import { getProfilePictureUrl } from "../../utils/imageUtils";
import householdService from "../../services/householdService";

// Light theme icons
import lightThemeIconLight from "../../assets/svg/light/light-theme.svg";
import profileIconLight from "../../assets/svg/light/profile-icon.svg";
import darkThemeIconLight from "../../assets/svg/light/dark-theme.svg";
import systemThemeIconLight from "../../assets/svg/light/system-theme.svg";
import houseIconLight from "../../assets/svg/light/house.svg";
import leaveIconLight from "../../assets/svg/light/leave.svg";
import householdDefaultIconLight from "../../assets/svg/light/householdDefault.svg";
import profileIconLightVariant from "../../assets/svg/light/profile-icon-light.svg";
import closeIconLightVariant from "../../assets/svg/light/close-icon-light.svg";

// Dark theme icons
import lightThemeIconDark from "../../assets/svg/dark/light-theme.svg";
import profileIconDark from "../../assets/svg/dark/profile-icon.svg";
import darkThemeIconDark from "../../assets/svg/dark/dark-theme.svg";
import systemThemeIconDark from "../../assets/svg/dark/system-theme.svg";
import houseIconDark from "../../assets/svg/dark/house.svg";
import leaveIconDark from "../../assets/svg/dark/leave.svg";
import householdDefaultIconDark from "../../assets/svg/dark/householdDefault.svg";
import profileIconDarkVariant from "../../assets/svg/dark/profile-icon-light.svg";
import closeIconDarkVariant from "../../assets/svg/dark/close-icon-light.svg";

import type { ThemeType } from "../../contexts/ThemeContext";

const NavbarMobile = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, hasHousehold, checkAuth } = useAuth();
  const { user } = useUser();
  const { householdInfo } = useHousehold();
  const { theme, setTheme } = useTheme();
  const isDark = useIsDarkTheme();
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [isProfileOpen, setProfileOpen] = useState(false);
  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>("");

  // Select themed icons
  const lightThemeIcon = isDark ? lightThemeIconDark : lightThemeIconLight;
  const darkThemeIcon = isDark ? darkThemeIconDark : darkThemeIconLight;
  const systemThemeIcon = isDark ? systemThemeIconDark : systemThemeIconLight;
  const profileIcon = isDark ? profileIconDark : profileIconLight;
  const houseIcon = isDark ? houseIconDark : houseIconLight;
  const leaveIcon = isDark ? leaveIconDark : leaveIconLight;
  const householdDefaultIcon = isDark ? householdDefaultIconDark : householdDefaultIconLight;
  
  // Icons for popup buttons - inverted (light icons on dark popup, dark icons on light popup)
  const profileIconForButton = isDark ? profileIconDarkVariant : profileIconLightVariant;
  const closeIconForButton = isDark ? closeIconDarkVariant : closeIconLightVariant;

  // Check if we're on manage pages where profile popup should be hidden
  const hideProfilePopup = location.pathname === "/manageAccount" || location.pathname === "/manageHousehold";

  const dropdownToggle = () => {
    setDropdownOpen(!isDropdownOpen);
  };

  const handleThemeSelect = (newTheme: ThemeType) => {
    setTheme(newTheme);
    setDropdownOpen(false);
  };

  const getThemeIcon = (themeType: ThemeType) => {
    switch (themeType) {
      case "light":
        return { src: lightThemeIcon, alt: "Light Theme" };
      case "dark":
        return { src: darkThemeIcon, alt: "Dark Theme" };
      case "system":
        return { src: systemThemeIcon, alt: "System Theme" };
    }
  };

  const getAvailableThemes = (): ThemeType[] => {
    const allThemes: ThemeType[] = ["light", "dark", "system"];
    return allThemes.filter((t) => t !== theme);
  };

  const profileClickHandler = () => {
    // Don't open profile popup on manage pages
    if (hideProfilePopup) return;
    setProfileOpen(!isProfileOpen);
  };

  // Add this function to prevent event bubbling
  const handlePopupClick = (e: React.MouseEvent) => {
    e.stopPropagation();
  };

  // Logout handler
  const handleLogout = async () => {
    try {
      await logout();
      setProfileOpen(false); // Close the profile popup
      navigate("/auth"); // Navigate to auth page after logout
    } catch (error) {
      console.error("Logout failed:", error);
    }
  };

  // Leave household handler
  const handleLeaveHousehold = async () => {
    setLoading(true);
    setError("");
    
    try {
      const result = await householdService.leaveHousehold();
      
      if (result.success) {
        setOpenPopup(false);
        setProfileOpen(false);
        // Refresh auth state to update hasHousehold flag
        await checkAuth(false);
        // Redirect to join or create household page
        navigate("/joinOrCreateHousehold");
      } else {
        setError(result.message || "Failed to leave household");
      }
    } catch (error) {
      console.error("Error leaving household:", error);
      setError("An unexpected error occurred");
    } finally {
      setLoading(false);
    }
  };

  // Get profile picture URL
  const profilePictureUrl = getProfilePictureUrl(user?.profilePicturePath);
  
  // Get current theme icon and available themes
  const currentThemeIcon = getThemeIcon(theme);
  const availableThemes = getAvailableThemes();

  return (
    <div className="navbar-mobile">
      <nav className="navbar-mobile-container">
        <span className="theme-toggle" onClick={dropdownToggle}>
          <img
            src={currentThemeIcon.src}
            alt={currentThemeIcon.alt}
            className="nav-theme-icon"
          />
        </span>
        {!hideProfilePopup && (
          <span className="profile-icon">
            {profilePictureUrl ? (
              <img
                src={profilePictureUrl}
                alt="Profile"
                className="nav-profile-icon"
                onClick={profileClickHandler}
                style={{ width: '46px', height: '46px', objectFit: 'cover', borderRadius: '50%' }}
              />
            ) : (
              <img
                src={profileIcon}
                alt="Profile"
                className="nav-profile-icon"
                onClick={profileClickHandler}
              />
            )}
          </span>
        )}
      </nav>

      <div className={`theme-dropdown${isDropdownOpen ? " open" : ""}`}>
        {availableThemes.map((themeOption) => {
          const themeIcon = getThemeIcon(themeOption);
          return (
            <img
              key={themeOption}
              src={themeIcon.src}
              alt={themeIcon.alt}
              className="nav-theme-icon"
              onClick={() => handleThemeSelect(themeOption)}
            />
          );
        })}
      </div>

      {!hideProfilePopup && (
        <div
          className={`profile__popup-container ${isProfileOpen ? "active" : ""}`}
          onClick={profileClickHandler}
        >
          <div className="profile-popup" onClick={handlePopupClick}>
            <i
              className="profile-popup__close-icon"
              onClick={profileClickHandler}
            ></i>
            {profilePictureUrl ? (
              <img
                src={profilePictureUrl}
                alt="Profile"
                className="profile-popup__icon"
                style={{ width: '46px', height: '46px', objectFit: 'cover', borderRadius: '50%' }}
              />
            ) : (
              <img
                src={profileIcon}
                alt="Profile"
                className="profile-popup__icon"
              />
            )}
            <h5>{user ? `${user.firstName} ${user.lastName}` : "Username"}</h5>
            {hasHousehold && <p>{householdInfo?.name || "Loading..."}</p>}
            <ul>
              <li>
                <Button
                  variant="primary"
                  onClick={() => (
                    navigate("/manageAccount"), setProfileOpen(false)
                  )}
                >
                  <img
                    src={profileIconForButton}
                    alt="Profile Management"
                    className="profile-management__icon"
                  />
                  Manage Account
                </Button>
              </li>
              {hasHousehold && (
                <>
                  {user?.id === householdInfo?.adminId && (
                    <li>
                      <Button
                        variant="primary"
                        onClick={() => (
                          navigate("/manageHousehold"), setProfileOpen(false)
                        )}
                      >
                        <img src={houseIcon} alt="Household Management" />
                        Manage Household
                      </Button>
                    </li>
                  )}
                  {user?.id !== householdInfo?.adminId && (
                    <li>
                      <Button variant="red" onClick={() => setOpenPopup(true)}>
                        <img src={leaveIcon} alt="" />
                        Leave Household
                      </Button>
                    </li>
                  )}
                </>
              )}
              <li>
                <Button variant="red" onClick={handleLogout}>
                  <img src={closeIconForButton} alt="" />
                  Log Out
                </Button>
              </li>
            </ul>
          </div>
        </div>
      )}

      {openPopup && (
        <Popup
          variant="red"
          onClose={() => setOpenPopup(false)}
          className="popup__leave-household"
          title=""
        >
          <p style={{ fontSize: 'var(--font-size-h4)' }}>Are you sure you want to leave this household?</p>
          <img 
            src={householdInfo?.profilePictureUrl || householdDefaultIcon} 
            alt="Household Image" 
          />
          <p style={{ fontSize: 'var(--font-size-h4)' }}>{householdInfo?.name || "Household"}</p>
          {error && <p style={{ color: 'var(--color-error)', marginTop: '10px', fontSize: 'var(--font-size-h4)' }}>{error}</p>}
          <Button variant="red" onClick={handleLeaveHousehold} disabled={loading}>
            {loading ? "Leaving..." : "Leave"}
          </Button>
        </Popup>
      )}
    </div>
  );
};

export default NavbarMobile;
