import React, { useEffect, useState } from "react";
import { useHousehold } from "../contexts/HouseholdContext";
import { useAuth } from "../contexts/AuthContext";
import Popup from "./Popup";

const AdminPromotionHandler: React.FC = () => {
  const { showAdminPromotionPopup, dismissAdminPromotionPopup } = useHousehold();
  const { logout } = useAuth();
  const [countdown, setCountdown] = useState(10);

  useEffect(() => {
    if (showAdminPromotionPopup) {
      setCountdown(10);

      const timer = setInterval(() => {
        setCountdown((prev) => {
          if (prev <= 1) {
            clearInterval(timer);
            handleLogout();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [showAdminPromotionPopup]);

  const handleLogout = async () => {
    dismissAdminPromotionPopup();
    await logout();
    window.location.href = "/login";
  };

  if (!showAdminPromotionPopup) return null;

  return (
    <Popup
      title="You're Now the Household Admin!"
      open={showAdminPromotionPopup}
      onClose={() => { }}
      showCloseButton={false}
      actions={[
        {
          label: "Log Out Now",
          onClick: handleLogout,
          variant: "primary",
        },
      ]}
    >
      <p style={{ textAlign: "center", marginBottom: "1rem" }}>
        Congratulations! You've been promoted to household administrator.
      </p>
      <p style={{ textAlign: "center", marginBottom: "1rem" }}>
        To apply your new permissions, you need to log in again.
      </p>
      <p style={{ textAlign: "center", fontWeight: "600" }}>
        Automatically logging out in {countdown} seconds...
      </p>
    </Popup>
  );
};

export default AdminPromotionHandler;
