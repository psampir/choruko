import "./CreateHousehold.scss";
import React, { useState } from "react";
import Button from "../../../components/Button";
import Input from "../../../components/Input";
import NavbarMobile from "../../../components/NavbarMobile";
import { useNavigate } from "react-router-dom";
import householdIcon from "../../../assets/svg/householdDefault.svg";
import arrowLeftIcon from "../../../assets/svg/arrow-left.svg";
import householdService from "../../../services/householdService";

function CreateHousehold() {
  const navigate = useNavigate();
  const [householdName, setHouseholdName] = useState("");
  const [selectedImage, setSelectedImage] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string>(householdIcon);
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

  const showNotification = (message: string, type: "success" | "error" = "success") => {
    setNotification({ message, isVisible: true, type });

    setTimeout(() => {
      setNotification({ message: "", isVisible: false, type: "success" });
    }, 3000);
  };

  const handleImageSelect = () => {
    document.getElementById("household-image-input")?.click();
  };

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
        profilePicture: selectedImage || undefined,
      });

      if (result.success) {
        showNotification("Household created successfully!", "success");
        setTimeout(() => {
          navigate("/leaderboard");
        }, 1500);
      } else {
        showNotification(result.message || "Failed to create household", "error");
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
      <img
        className="create-household back-arrow"
        src={arrowLeftIcon}
        alt="back-arrow"
        onClick={() => navigate(-1)}
      />
      <div
        className={`notification-bar notification-bar--${notification.type} ${
          notification.isVisible ? "notification-bar--visible" : ""
        }`}
      >
        <span>{notification.message}</span>
      </div>

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
