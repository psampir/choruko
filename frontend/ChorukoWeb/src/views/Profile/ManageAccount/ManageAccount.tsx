import React, { useEffect, useRef } from "react";
import "./ManageAccount.scss";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import Popup from "../../../components/Popup";

const ManageAccount: React.FC = () => {
  const accountContentRef = useRef<HTMLDivElement>(null);
  const scrollableContentRef = useRef<HTMLDivElement>(null);
  const accountItemRef = useRef<HTMLDivElement>(null);


  // Calculate dynamic height after component mounts
  useEffect(() => {
    const calculateDynamicHeight = () => {
      if (
        accountItemRef.current &&
        accountContentRef.current &&
        scrollableContentRef.current
      ) {
        const itemHeight = accountItemRef.current.offsetHeight;
        const desiredVisibleCount = 3;
        const maxHeight = itemHeight * desiredVisibleCount;

        // Set CSS custom property for dynamic height
        accountContentRef.current.style.setProperty(
          "--dynamic-item-height",
          `${itemHeight}px`
        );
        accountContentRef.current.style.setProperty(
          "--max-visible-items",
          `${desiredVisibleCount}`
        );

        // Set max height on both containers
        accountContentRef.current.style.maxHeight = `${maxHeight}px`;
        scrollableContentRef.current.style.maxHeight = `${maxHeight}px`;
      }
    };

    // Calculate after a small delay to ensure DOM is fully rendered
    const timer = setTimeout(calculateDynamicHeight, 100);

    // Recalculate on window resize
    window.addEventListener("resize", calculateDynamicHeight);

    return () => {
      clearTimeout(timer);
      window.removeEventListener("resize", calculateDynamicHeight);
    };
  }, []);

  return (
    <div className="manage-account">
      <h1 className="manage-account__template-name">Manage Account</h1>
      <div className="account-header">
        <img
          className="account-image"
          src="/src/assets/svg/householdDefault.svg"
          alt="Account"
        />
        <Button variant="primary">Change Picture</Button>
      </div>
      <div className="manage-account__main-section">
        <div className="manage-account__main-section change-name">
          <Input variant="edit" value="Vin Diesel"/>
          <Button variant="primary">Change Name</Button>
        </div>
        <div className="manage-account__main-section change-password">
          <Input variant="password" placeholder="New Password"/>
          <Input variant="password" placeholder="Confirm New Password"/>
          <Button variant="primary">Change Name</Button>
        </div>
        <Button variant="red" className="delete-account__btn">Delete Account</Button>
      </div>
      <Popup variant="red"/>
    </div>
  );
};

export default ManageAccount;
