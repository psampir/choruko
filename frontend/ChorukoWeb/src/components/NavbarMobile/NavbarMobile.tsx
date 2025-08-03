import "./NavbarMobile.scss";
import { useState } from "react";

const NavbarMobile = () => {
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const [isProfileOpen, setProfileOpen] = useState(false);

  const dropdownToggle = () => {
    setDropdownOpen(!isDropdownOpen);
  };

  const profileClickHandler = () => {
    setProfileOpen(!isProfileOpen);
  };
  return (
    <div className="navbar-mobile">
      <nav className="navbar-mobile-container">
        <span className="theme-toggle" onClick={dropdownToggle}>
          <img src="/src/assets/svg/light-theme.svg" alt="Light Theme" />
        </span>
        <span className="profile-icon">
          <img
            src="/src/assets/svg/profile-icon.svg"
            alt="Profile"
            onClick={profileClickHandler}
          />
        </span>
      </nav>

      <div className={`theme-dropdown${isDropdownOpen ? " open" : ""}`}>
        <img
          src="/src/assets/svg/dark-theme.svg"
          alt="Dark Theme"
          className="dark-theme-icon"
        />
        <img
          src="/src/assets/svg/default-theme.svg"
          alt="Default Theme"
          className="default-theme-icon"
        />
      </div>

      <div className={`profile-popup${isProfileOpen ? " open" : ""}`}>
        <img src="/src/assets/svg/profile-icon.svg" alt="Profile" />
        <h5>Username</h5>
        <p>Household Name</p>
        <ul>
          <li>Profile Settings</li>
          <li>Logout</li>
        </ul>
      </div>
    </div>
  );
};

export default NavbarMobile;
