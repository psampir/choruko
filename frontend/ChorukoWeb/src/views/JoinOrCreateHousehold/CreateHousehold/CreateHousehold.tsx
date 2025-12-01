import "./CreateHousehold.scss";
import React, { useState, useEffect } from "react";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import NavbarMobile from "../../../components/NavbarMobile";
import NotificationBar from "../../../components/NotificationBar";
import { useNavigate } from "react-router-dom";
import { useUser } from "../../../contexts/UserContext";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import { POPULAR_CURRENCIES } from "../../../utils/currencies";
import householdIconLight from "../../../assets/svg/light/householdDefault.svg";
import householdIconDark from "../../../assets/svg/dark/householdDefault.svg";
import arrowLeftIconLight from "../../../assets/svg/light/arrow-left.svg";
import arrowLeftIconDark from "../../../assets/svg/dark/arrow-left.svg";
import checkIconLight from "../../../assets/svg/light/check.svg";
import checkIconDark from "../../../assets/svg/dark/check.svg";
import householdService from "../../../services/householdService";

// Create new household with name and optional profile picture
function CreateHousehold() {
  const navigate = useNavigate();
  const { refreshUser } = useUser();
  const { refreshHousehold } = useHousehold();
  const isDark = useIsDarkTheme();
  const householdIcon = isDark ? householdIconDark : householdIconLight;
  const arrowLeftIcon = isDark ? arrowLeftIconDark : arrowLeftIconLight;
  const checkIcon = isDark ? checkIconDark : checkIconLight;
  const [householdName, setHouseholdName] = useState("");
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>(householdIcon);
  const [selectedCurrency, setSelectedCurrency] = useState<string>("USD");
  const [currencyCollapsed, setCurrencyCollapsed] = useState(true);
  const [loading, setLoading] = useState(false);
  const [notification, setNotification] = useState<{
    message: string;
    isVisible: boolean;
    type: "success" | "error";
  }>({
    message: "",
    isVisible: false,
    type: "success",
  });

  const showNotification = (
    message: string,
    type: "success" | "error" = "success"
  ) => {
    setNotification({ message, isVisible: true, type });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false, type: "success" });
    }, 3000);
  };

  useEffect(() => {
    if (!selectedImage) {
      setImagePreview(householdIcon);
    }
  }, [householdIcon, selectedImage]);

  // Trigger file input click for image selection
  const handleImageSelect = () => {
    document.getElementById("household-image-input")?.click();
  };

  // Preview selected image before upload
  const handleImageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedImage(file);
      const reader = new FileReader();
      reader.onload = (e) => {
        const result = e.target?.result as string;
        setImagePreview(result);
      };
      reader.readAsDataURL(file);
    }
  };

  // Validate and create household, then navigate to leaderboard
  const handleCreateHousehold = async () => {
    if (!householdName.trim()) {
      showNotification("Please enter a household name", "error");
      return;
    }

    if (householdName.trim().length < 2) {
      showNotification("Household name must be at least 2 characters", "error");
      return;
    }

    setLoading(true);

    try {
      const result = await householdService.createHousehold({
        name: householdName.trim(),
        currencyCode: selectedCurrency,
        profilePicture: selectedImage || undefined,
      });

      if (result.success) {
        showNotification("Household created successfully!", "success");
        // Update user state with household info before navigating
        await refreshUser();
        await refreshHousehold();
        setTimeout(() => {
          navigate("/leaderboard");
        }, 1500);
      } else {
        showNotification(
          result.message || "Failed to create household",
          "error"
        );
      }
    } catch (error) {
      showNotification("An unexpected error occurred", "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="create-household">
      <NavbarMobile />
      <NotificationBar
        message={notification.message}
        isVisible={notification.isVisible}
        type={notification.type}
      />
      <img
        className="back-arrow"
        src={arrowLeftIcon}
        alt="back-arrow"
        onClick={() => navigate(-1)}
      />

      <h1>Create Household</h1>
      <img
        src={imagePreview}
        alt="Household"
        className="create-household__icon"
      />
      <Button
        className="create-household__select-image"
        variant="primary"
        onClick={handleImageSelect}
      >
        Select an Image
      </Button>
      <input
        id="household-image-input"
        type="file"
        accept="image/*"
        onChange={handleImageChange}
        style={{ display: "none" }}
      />
      <Input
        variant="edit"
        placeholder="Household Name"
        value={householdName}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
          setHouseholdName(e.target.value)
        }
      />

      {/* Currency Selection */}
      <div
        className={`participants-list ${
          currencyCollapsed ? "participants-list--collapsed" : ""
        }`}
      >
        <div
          className="participants-list__header"
          onClick={() => setCurrencyCollapsed(!currencyCollapsed)}
        >
          SELECT CURRENCY
          <span className="dropdown__arrow">&#9660;</span>
        </div>
        {!currencyCollapsed && (
          <div className="participants-list__content">
            {POPULAR_CURRENCIES.map((currency) => (
              <div
                className={`activity ${
                  selectedCurrency === currency.code ? "activity--selected" : ""
                }`}
                key={currency.code}
                onClick={() => setSelectedCurrency(currency.code)}
              >
                <div className="activity-data">
                  <span className="currency-code">{currency.code}</span>
                  <p>{currency.name}</p>
                </div>
                {selectedCurrency === currency.code && (
                  <img
                    src={checkIcon}
                    alt="Selected"
                    className="check-icon"
                  />
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <Button
        variant="primary"
        className="create-btn"
        onClick={handleCreateHousehold}
        disabled={loading}
      >
        {loading ? "Creating..." : "Create"}
      </Button>
    </div>
  );
}

export default CreateHousehold;
