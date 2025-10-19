import "./NavbarMobile.scss";
import { useState } from "react";
import Button from "../Button/Button";
import Popup from "../Popup";
import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../../contexts/AuthContext";
import { useUser } from "../../contexts/UserContext";
import { useHousehold } from "../../contexts/HouseholdContext";
import { getProfilePictureUrl } from "../../utils/imageUtils";
import householdService from "../../services/householdService";

// Import all SVG assets
import lightThemeIcon from "../../assets/svg/light-theme.svg";
import profileIcon from "../../assets/svg/profile-icon.svg";
import darkThemeIcon from "../../assets/svg/dark-theme.svg";
import defaultThemeIcon from "../../assets/svg/default-theme.svg";
import profileIconLight from "../../assets/svg/profile-icon-light.svg";
import houseIcon from "../../assets/svg/house.svg";
import leaveIcon from "../../assets/svg/leave.svg";
import closeIconLight from "../../assets/svg/close-icon-light.svg";
import householdDefaulIcon from "../../assets/svg/householdDefault.svg";

const NavbarMobile = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, hasHousehold, checkAuth } = useAuth();
  const { user } = useUser();
  const { householdInfo } = useHousehold();
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [isProfileOpen, setProfileOpen] = useState(false);
  const [openPopup, setOpenPopup] = useState<boolean>(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>("");

  // Check if we're on manage pages where profile popup should be hidden
  const hideProfilePopup = location.pathname === "/manageAccount" || location.pathname === "/manageHousehold";

  const dropdownToggle = () => {
    setDropdownOpen(!isDropdownOpen);
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

  return (
    <div className="navbar-mobile">
      <nav className="navbar-mobile-container">
        <span className="theme-toggle" onClick={dropdownToggle}>
          <img
            src={lightThemeIcon}
            alt="Light Theme"
            className="nav-theme-icon light-theme-icon"
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
        <img
          src={darkThemeIcon}
          alt="Dark Theme"
          className=" nav-theme-icon dark-theme-icon"
          onClick={dropdownToggle}
        />
        <img
          src={defaultThemeIcon}
          alt="Default Theme"
          className="theme-icon default-theme-icon"
          onClick={dropdownToggle}
        />
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
                    src={profileIconLight}
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
                  <img src={closeIconLight} alt="" />
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
          <p>Are you sure you want to leave this household?</p>
          <img 
            src={householdInfo?.profilePictureUrl || householdDefaulIcon} 
            alt="Household Image" 
          />
          <p>{householdInfo?.name || "Household"}</p>
          {error && <p style={{ color: 'var(--color-error)', marginTop: '10px' }}>{error}</p>}
          <Button variant="red" onClick={handleLeaveHousehold} disabled={loading}>
            {loading ? "Leaving..." : "Leave"}
          </Button>
        </Popup>
      )}
    </div>
  );
};

export default NavbarMobile;
