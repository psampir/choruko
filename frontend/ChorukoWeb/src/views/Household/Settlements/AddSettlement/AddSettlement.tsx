import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useUser } from "../../../../contexts/UserContext";
import { useIsDarkTheme } from "../../../../utils/iconUtils";
import "./AddSettlement.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIconLight from "../../../../assets/svg/light/arrow-left.svg";
import arrowLeftIconDark from "../../../../assets/svg/dark/arrow-left.svg";
import Popup from "../../../../components/Popup/Popup";
import ErrorMessage from "../../../../components/ErrorMessage";
import checkIconLight from "../../../../assets/svg/light/check.svg";
import checkIconDark from "../../../../assets/svg/dark/check.svg";

interface Assignee {
  id: number;
  name: string;
  avatar: string;
}

// Create or edit settlement with assignee selection and amount calculation
const AddSettlement: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();
  const isDark = useIsDarkTheme();

  const arrowLeftIcon = isDark ? arrowLeftIconDark : arrowLeftIconLight;
  const checkIcon = isDark ? checkIconDark : checkIconLight;

  // Determine if editing existing settlement
  const editSettlement = location.state?.editSettlement;
  const isEditMode = !!editSettlement;

  const [settlementDescription, setSettlementDescription] = useState("");
  const [selectedAssignees, setSelectedAssignees] = useState<number[]>([]);
  const [amountSpent, setAmountSpent] = useState("");
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const currencySymbol = "₴";

  // Pre-fill form when editing
  useEffect(() => {
    if (editSettlement) {
      setSettlementDescription(editSettlement.description || "");
      setAmountSpent(editSettlement.amountSpent || "");

      // Match assignees by name and avatar to handle duplicates
      if (editSettlement.assignees && Array.isArray(editSettlement.assignees)) {
        const assigneeIds = assignees
          .filter((assignee) =>
            editSettlement.assignees.some(
              (ea: any) =>
                ea.name === assignee.name && ea.avatar === assignee.avatar
            )
          )
          .map((assignee) => assignee.id);
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

  // Mock data for assignees
  const assignees: Assignee[] = [
    {
      id: 1,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/1.jpg",
    },
    {
      id: 2,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/2.jpg",
    },
    {
      id: 3,
      name: "Andreas Toretto",
      avatar: "https://randomuser.me/api/portraits/men/3.jpg",
    },
  ];

  const handleAssigneeToggle = (assigneeId: number) => {
    setSelectedAssignees((prev) =>
      prev.includes(assigneeId)
        ? prev.filter((id) => id !== assigneeId)
        : [...prev, assigneeId]
    );
  };

  const handleAddSettlement = () => {
    // Validate required fields before submission
    const missingFields = [];
    if (!settlementDescription) missingFields.push("Settlement description");
    if (selectedAssignees.length === 0) missingFields.push("Assignees");
    if (!amountSpent) missingFields.push("Amount spent");

    if (missingFields.length > 0) {
      setErrorMessage(`Please fill in: ${missingFields.join(", ")}`);
      return;
    }

    // Get selected assignee objects
    const selectedAssigneeObjs = assignees.filter((assignee) =>
      selectedAssignees.includes(assignee.id)
    );

    if (isEditMode && editSettlement) {
      const updatedSettlement = {
        id: editSettlement.id,
        name: settlementDescription,
        description: settlementDescription,
        assignees: selectedAssigneeObjs,
        amountSpent: amountSpent,
        author: editSettlement.author,
      };

      setErrorMessage("");

      // Navigate back with updated settlement
      navigate("/settlements", { state: { updatedSettlement } });
    } else {
      const newSettlement = {
        name: settlementDescription,
        description: settlementDescription,
        assignees: selectedAssigneeObjs,
        amountSpent: amountSpent,
        author: user
          ? {
              name: `${user.firstName} ${user.lastName}`,
              avatar: user.profilePicturePath || null,
            }
          : null,
      };

      setErrorMessage("");

      navigate("/settlements", { state: { newSettlement } });
    }
  };

  // Navigate back with delete ID to remove settlement
  const handleDeleteSettlement = () => {
    if (editSettlement) {
      navigate("/settlements", {
        state: { deleteSettlementId: editSettlement.id },
      });
    }
  };

  const handleBack = () => {
    navigate("/settlements");
  };

  return (
    <div className="add-settlement">
      <NavbarMobile />

      <ErrorMessage
        message={errorMessage}
        onClose={() => setErrorMessage("")}
        variant="overlay"
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
          className={`participants-list ${
            collapsedSections.assignees ? "participants-list--collapsed" : ""
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
                  className={`activity ${
                    selectedAssignees.includes(assignee.id)
                      ? "activity--selected"
                      : ""
                  }`}
                  onClick={() => handleAssigneeToggle(assignee.id)}
                >
                  <div className="activity-data">
                    <img
                      src={assignee.avatar}
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
              = {currencySymbol}
              {(parseFloat(amountSpent) / selectedAssignees.length).toFixed(
                2
              )}{" "}
              per person
            </p>
          )}
        </div>

        {/* Add/Save Button */}
        <div className="add-settlement__footer">
          <Button variant="primary" onClick={handleAddSettlement}>
            {isEditMode ? "Save Changes" : "Add"}
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
              variant: "secondary",
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
