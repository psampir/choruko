import "./NavbarMobile.scss";
import { useState } from "react";
import Button from "../Button/Button";

// Import all SVG assets
import lightThemeIcon from "../../assets/svg/light-theme.svg";
import profileIcon from "../../assets/svg/profile-icon.svg";
import darkThemeIcon from "../../assets/svg/dark-theme.svg";
import defaultThemeIcon from "../../assets/svg/default-theme.svg";
import profileIconLight from "../../assets/svg/profile-icon-light.svg";
import houseIcon from "../../assets/svg/house.svg";
import leaveIcon from "../../assets/svg/leave.svg";
import closeIconLight from "../../assets/svg/close-icon-light.svg";

const NavbarMobile = () => {
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [isProfileOpen, setProfileOpen] = useState(false);

  const dropdownToggle = () => {
    setDropdownOpen(!isDropdownOpen);
  };

  const profileClickHandler = () => {
    setProfileOpen(!isProfileOpen);
  };

  // Add this function to prevent event bubbling
  const handlePopupClick = (e: React.MouseEvent) => {
    e.stopPropagation();
  };

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
        <span className="profile-icon">
          <img
            src={profileIcon}
            alt="Profile"
            className="nav-profile-icon"
            onClick={profileClickHandler}
          />
        </span>
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

      <div
        className={`profile__popup-container ${isProfileOpen ? "active" : ""}`}
        onClick={profileClickHandler}
      >
        {/* Add onClick={handlePopupClick} to prevent bubbling */}
        <div className="profile-popup" onClick={handlePopupClick}>
          <i
            className="profile-popup__close-icon"
            onClick={profileClickHandler}
          ></i>
          <img
            src={profileIcon}
            alt="Profile"
            className="profile-popup__icon"
          />
          <h5>Username</h5>
          <p>Household Name</p>
          <ul>
            <li>
              <Button variant="primary">
                <img
                  src={profileIconLight}
                  alt="Profile Management"
                  className="profile-management__icon"
                />
                Manage Account
              </Button>
            </li>
            <li>
              <Button variant="primary">
                <img src={houseIcon} alt="Household Management" />
                Manage Household
              </Button>
            </li>
            <li>
              <Button variant="red">
                <img src={leaveIcon} alt="" />
                Leave Household
              </Button>
            </li>
            <li>
              <Button variant="red">
                <img src={closeIconLight} alt="" />
                Log Out
              </Button>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default NavbarMobile;
