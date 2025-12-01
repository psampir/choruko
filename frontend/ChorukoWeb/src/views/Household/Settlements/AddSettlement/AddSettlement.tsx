import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useUser } from "../../../../contexts/UserContext";
import { useSettlement } from "../../../../contexts/SettlementContext";
import { useHousehold } from "../../../../contexts/HouseholdContext";
import { useIsDarkTheme } from "../../../../utils/iconUtils";
import { getProfilePictureUrl } from "../../../../utils/imageUtils";
import { formatCurrency } from "../../../../utils/currencyFormatter";
import "./AddSettlement.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIconLight from "../../../../assets/svg/light/arrow-left.svg";
import arrowLeftIconDark from "../../../../assets/svg/dark/arrow-left.svg";
import Popup from "../../../../components/Popup/Popup";
import NotificationBar from "../../../../components/NotificationBar";
import checkIconLight from "../../../../assets/svg/light/check.svg";
import checkIconDark from "../../../../assets/svg/dark/check.svg";
import profileDefaultIconLight from "../../../../assets/svg/light/profile-icon.svg";
import profileDefaultIconDark from "../../../../assets/svg/dark/profile-icon.svg";

// Assignee type definition (from household participants)
interface Assignee {
  id: string;
  name: string;
  avatar: string | null;
}

const AddSettlement: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();
  const { createSettlement, editSettlement: editSettlementContext, deleteSettlement } = useSettlement();
  const { participants, householdInfo } = useHousehold();
  const isDark = useIsDarkTheme();

  const arrowLeftIcon = isDark ? arrowLeftIconDark : arrowLeftIconLight;
  const checkIcon = isDark ? checkIconDark : checkIconLight;
  const profileDefaultIcon = isDark ? profileDefaultIconDark : profileDefaultIconLight;

  // Check if we're in edit mode
  const editSettlement = location.state?.editSettlement;
  const isEditMode = !!editSettlement;

  const [settlementDescription, setSettlementDescription] = useState("");
  const [selectedAssignees, setSelectedAssignees] = useState<string[]>([]);
  const [amountSpent, setAmountSpent] = useState("");
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Get currency symbol from household info
  const currencySymbol = householdInfo?.currencySymbol || "$";

  // Pre-fill form when in edit mode
  useEffect(() => {
    if (editSettlement) {
      setSettlementDescription(editSettlement.description || "");
      setAmountSpent(editSettlement.amount?.toString() || "");

      // Set selected assignees (unsettled users)
      if (editSettlement.unsettledUsers && Array.isArray(editSettlement.unsettledUsers)) {
        const assigneeIds = editSettlement.unsettledUsers.map((u: any) => u.id);
        setSelectedAssignees(assigneeIds);
      }
    }
  }, [editSettlement]);

  // State for collapsed sections
  const [collapsedSections, setCollapsedSections] = useState({
    assignees: false,
  });

  // Toggle section collapse
  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  // Get assignees from household participants (excluding current user - creator pays upfront)
  const assignees: Assignee[] = (participants || [])
    .filter(p => p.id !== user?.id) // Exclude creator
    .map(p => ({
      id: p.id,
      name: `${p.firstName} ${p.lastName}`,
      avatar: getProfilePictureUrl(p.profilePicturePath),
    }));

  const handleAssigneeToggle = (assigneeId: string) => {
    setSelectedAssignees((prev) =>
      prev.includes(assigneeId)
        ? prev.filter((id) => id !== assigneeId)
        : [...prev, assigneeId]
    );
  };

  const handleAddSettlement = async () => {
    // Validate that required fields are filled
    const missingFields = [];
    if (!settlementDescription) missingFields.push("Settlement description");
    if (selectedAssignees.length === 0) missingFields.push("Assignees");
    if (!amountSpent) missingFields.push("Amount spent");

    if (missingFields.length > 0) {
      setErrorMessage(`Please fill in: ${missingFields.join(", ")}`);
      return;
    }

    // Validate amount
    const amountNum = parseFloat(amountSpent);
    if (amountNum <= 0 || isNaN(amountNum)) {
      setErrorMessage("Amount must be greater than 0");
      return;
    }

    setIsSubmitting(true);
    setErrorMessage("");

    try {
      if (isEditMode && editSettlement) {
        // Update existing settlement
        const result = await editSettlementContext(editSettlement.id, {
          description: settlementDescription,
          amount: amountNum,
          unsettledUsers: selectedAssignees,
        });

        if (result.success) {
          navigate("/settlements");
        } else {
          setErrorMessage(result.message || "Failed to update settlement");
        }
      } else {
        // Create new settlement
        const result = await createSettlement({
          description: settlementDescription,
          amount: amountNum,
          unsettledUsers: selectedAssignees,
        });

        if (result.success) {
          navigate("/settlements");
        } else {
          setErrorMessage(result.message || "Failed to create settlement");
        }
      }
    } catch (error) {
      console.error("Error saving settlement:", error);
      setErrorMessage("An unexpected error occurred");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteSettlement = async () => {
    if (editSettlement) {
      setIsSubmitting(true);
      const result = await deleteSettlement(editSettlement.id);

      if (result.success) {
        navigate("/settlements");
      } else {
        console.error("Delete failed:", result.message);
        setErrorMessage(result.message || "Failed to delete settlement");
        setIsSubmitting(false);
      }
    }
  };

  const handleBack = () => {
    navigate("/settlements");
  };

  return (
    <div className="add-settlement">
      <NavbarMobile />

      <NotificationBar
        message={errorMessage}
        isVisible={!!errorMessage}
        type="error"
        onClose={() => setErrorMessage("")}
        autoDismiss={true}
        dismissTime={5000}
      />

      <div className="add-settlement__container">
        {/* Header */}
        <div className="add-settlement__header">
          <button
            className="add-settlement__back-btn"
            onClick={handleBack}
            aria-label="Go back"
          >
            <img src={arrowLeftIcon} alt="Back" />
          </button>
          <h1 className="add-settlement__title">
            {isEditMode ? "Edit settlement" : "Add a settlement"}
          </h1>
        </div>

        {/* Settlement Description */}
        <div className="add-settlement__section">
          <Input
            variant="edit"
            placeholder="Settlement description"
            value={settlementDescription}
            onChange={(e) => setSettlementDescription(e.target.value)}
            className="add-settlement__input"
          />
        </div>

        {/* Select Assignees Section */}
        <div
          className={`participants-list ${collapsedSections.assignees ? "participants-list--collapsed" : ""
            }`}
        >
          <div
            className="participants-list__header"
            onClick={() => toggleSection("assignees")}
          >
            SELECT PAYERS
            <span className="dropdown__arrow">&#9660;</span>
          </div>
          {!collapsedSections.assignees && (
            <div className="participants-list__content">
              {assignees.map((assignee) => (
                <div
                  key={assignee.id}
                  className={`activity ${selectedAssignees.includes(assignee.id)
                    ? "activity--selected"
                    : ""
                    }`}
                  onClick={() => handleAssigneeToggle(assignee.id)}
                >
                  <div className="activity-data">
                    <img
                      src={assignee.avatar || profileDefaultIcon}
                      alt={assignee.name}
                      className="assignee-avatar"
                    />
                    <p>{assignee.name}</p>
                  </div>
                  {selectedAssignees.includes(assignee.id) && (
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

        {/* Amount Spent */}
        <div className="add-settlement__section add-settlement__section--no-margin">
          <Input
            type="number"
            variant="currency"
            currencySymbol={currencySymbol}
            placeholder="Amount spent"
            value={amountSpent}
            onChange={(e) => {
              const value = e.target.value;
              // Allow only numbers and decimal point
              if (value === "" || /^\d*\.?\d*$/.test(value)) {
                setAmountSpent(value);
              }
            }}
            className="add-settlement__input"
          />

          {/* Amount per person calculation */}
          {amountSpent && selectedAssignees.length > 0 && (
            <p className="amount-per-person">
              = {formatCurrency(
                parseFloat(amountSpent) / (selectedAssignees.length + 1),
                currencySymbol
              )}{" "}
              per person
            </p>
          )}
        </div>

        {/* Add/Save Button */}
        <div className="add-settlement__footer">
          <Button 
            variant="primary" 
            onClick={handleAddSettlement}
            disabled={isSubmitting}
          >
            {isSubmitting ? "Saving..." : (isEditMode ? "Save Changes" : "Add")}
          </Button>
          {isEditMode && (
            <Button
              variant="red"
              onClick={() => setShowDeleteConfirm(true)}
              className="add-settlement__delete-btn"
            >
              Delete settlement
            </Button>
          )}
        </div>
      </div>

      {/* Delete Confirmation Popup */}
      {showDeleteConfirm && (
        <Popup
          open={showDeleteConfirm}
          onClose={() => setShowDeleteConfirm(false)}
          title="Delete Settlement"
          variant="red"
          actions={[
            {
              label: "Cancel",
              onClick: () => setShowDeleteConfirm(false),
              variant: "primary",
            },
            {
              label: "Delete",
              onClick: () => {
                handleDeleteSettlement();
                setShowDeleteConfirm(false);
              },
              variant: "danger",
            },
          ]}
        >
          <p>
            Are you sure you want to delete this settlement? This action cannot
            be undone.
          </p>
        </Popup>
      )}
    </div>
  );
};

export default AddSettlement;
