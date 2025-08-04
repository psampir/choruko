import "./NavbarMobile.scss";
import { useState } from "react";
import Button from "../Button/Button";
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
          <img src="/src/assets/svg/light-theme.svg" alt="Light Theme" className="nav-theme-icon light-theme-icon" />
        </span>
        <span className="profile-icon">
          <img
            src="/src/assets/svg/profile-icon.svg"
            alt="Profile"
            className="nav-profile-icon"
            onClick={profileClickHandler}
          />
        </span>
      </nav>

      <div className={`theme-dropdown${isDropdownOpen ? " open" : ""}`}>
        <img
          src="/src/assets/svg/dark-theme.svg"
          alt="Dark Theme"
          className=" nav-theme-icon dark-theme-icon"
        />
        <img
          src="/src/assets/svg/default-theme.svg"
          alt="Default Theme"
          className="theme-icon default-theme-icon"
        />
      </div>

      <div
        className={`profile__popup-container ${isProfileOpen ? "active" : ""}`}
        onClick={profileClickHandler}
      >
        <div className="profile-popup">
          <img src="/src/assets/svg/profile-icon.svg" alt="Profile" className="profile-popup__icon" />
          <h5>Username</h5>
          <p>Household Name</p>
          <ul>
            <li>
              <Button variant="primary">
                <img src="/src/assets/svg/profile-icon-light.svg" alt="Profile Management" className="profile-management__icon" />
                Manage Account
              </Button>
            </li>
            <li>
              <Button variant="primary">
                <img src="/src/assets/svg/house.svg" alt="Household Management" />
                Manage Household
              </Button>
            </li>
            <li>
              <Button variant="red">
                <img src="/src/assets/svg/leave.svg" alt="" />
                Leave Household
              </Button>
            </li>
            <li>
              <Button variant="red">
                <img src="/src/assets/svg/close-icon-light.svg" alt="" />
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
