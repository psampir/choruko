import React, { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useUser } from "../../../../contexts/UserContext";
import "./AddSettlement.scss";
import Button from "../../../../components/Button";
import Input from "../../../../components/Input";
import NavbarMobile from "../../../../components/NavbarMobile";
import arrowLeftIcon from "../../../../assets/svg/arrow-left.svg";
import Popup from "../../../../components/Popup/Popup";
import checkIcon from "../../../../assets/svg/check.svg";

// Assignee type definition
interface Assignee {
  id: number;
  name: string;
  avatar: string;
}

const AddSettlement: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();

  // Check if we're in edit mode
  const editSettlement = location.state?.editSettlement;
  const isEditMode = !!editSettlement;

  const [settlementDescription, setSettlementDescription] = useState("");
  const [selectedAssignees, setSelectedAssignees] = useState<number[]>([]);
  const [amountSpent, setAmountSpent] = useState("");
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  // Pre-fill form when in edit mode
  useEffect(() => {
    if (editSettlement) {
      setSettlementDescription(editSettlement.description || "");
      setAmountSpent(editSettlement.amountSpent || "");

      // Select assignees by matching both name AND avatar
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
    // Validate that required fields are filled
    if (
      !settlementDescription ||
      selectedAssignees.length === 0 ||
      !amountSpent
    ) {
      console.log("Please fill all required fields");
      return;
    }

    // Get the selected assignees' objects
    const selectedAssigneeObjs = assignees.filter((assignee) =>
      selectedAssignees.includes(assignee.id)
    );

    if (isEditMode && editSettlement) {
      // Update existing settlement
      const updatedSettlement = {
        id: editSettlement.id,
        name: settlementDescription,
        description: settlementDescription,
        assignees: selectedAssigneeObjs,
        amountSpent: amountSpent,
        author: editSettlement.author,
      };

      // Navigate back to settlements with the updated settlement data
      navigate("/settlements", { state: { updatedSettlement } });
    } else {
      // Create new settlement
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

      // Navigate back to settlements with the new settlement data
      navigate("/settlements", { state: { newSettlement } });
    }
  };

  const handleDeleteSettlement = () => {
    if (editSettlement) {
      // Navigate back with delete flag
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
            {isEditMode ? "Edit settlement" : "Add a new settlement"}
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
            SELECT ASSIGNEES
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
        <div className="add-settlement__section">
          <Input
            type="number"
            variant="currency"
            currencySymbol="₴"
            placeholder="Amount spent"
            value={amountSpent}
            onChange={(e) => setAmountSpent(e.target.value)}
            className="add-settlement__input"
          />
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
