import React, { useState, useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./Settlements.scss";
import Button from "../../../components/Button";
import BottomNav from "../../../components/BottomNav/BottomNav";
import Popup from "../../../components/Popup/Popup";
import { useUser } from "../../../contexts/UserContext";
import profileIcon from "../../../assets/svg/light/profile-icon.svg";
import starIcon from "../../../assets/svg/dark/star.svg";
import pencilLight from "../../../assets/svg/dark/pencil.svg";
import checkLight from "../../../assets/svg/light/check.svg";
import settlementIcon from "../../../assets/svg/light/expenses.svg";

interface Assignee {
  name: string;
  avatar: string;
}
interface Author {
  name: string;
  avatar: string | null;
}
interface Settlement {
  id: number;
  name: string;
  description: string;
  assignees: Assignee[];
  status: "todo" | "done";
  author?: Author;
  amountSpent?: string;
}

const Settlements: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();

  const [collapsedSections, setCollapsedSections] = useState({
    todo: false,
    done: false,
  });

  // Load settlements from localStorage or return default settlements
  const getInitialSettlements = (): Settlement[] => {
    const saved = localStorage.getItem("choruko-settlements");
    if (saved) {
      return JSON.parse(saved);
    }
    return [
      {
        id: 1,
        name: "Groceries",
        description: "Weekly grocery shopping",
        assignees: [
          {
            name: "Andreas Toretto",
            avatar: "https://randomuser.me/api/portraits/men/1.jpg",
          },
          {
            name: "John Doe",
            avatar: "https://randomuser.me/api/portraits/men/2.jpg",
          },
        ],
        status: "todo",
        author: {
          name: "Andreas Toretto",
          avatar: "https://randomuser.me/api/portraits/men/1.jpg",
        },
        amountSpent: "150.50",
      },
      {
        id: 2,
        name: "Electricity Bill",
        description: "Monthly electricity payment",
        assignees: [
          {
            name: "Anna Lee",
            avatar: "https://randomuser.me/api/portraits/women/2.jpg",
          },
        ],
        status: "todo",
        author: {
          name: "Gabby Mayers",
          avatar: "https://randomuser.me/api/portraits/women/2.jpg",
        },
        amountSpent: "85.00",
      },
      {
        id: 3,
        name: "Rent Payment",
        description: "Monthly rent for apartment",
        assignees: [
          {
            name: "Andreas Toretto",
            avatar: "https://randomuser.me/api/portraits/men/1.jpg",
          },
          {
            name: "John Doe",
            avatar: "https://randomuser.me/api/portraits/men/2.jpg",
          },
          {
            name: "Jane Smith",
            avatar: "https://randomuser.me/api/portraits/men/3.jpg",
          },
        ],
        status: "done",
        author: {
          name: "Ann Huang",
          avatar: "https://randomuser.me/api/portraits/women/1.jpg",
        },
        amountSpent: "1200.00",
      },
    ];
  };

  const [settlements, setSettlements] = useState<Settlement[]>(
    getInitialSettlements()
  );

  // Persist settlements to localStorage on every change
  useEffect(() => {
    localStorage.setItem("choruko-settlements", JSON.stringify(settlements));
  }, [settlements]);

  const hasAddedSettlementRef = useRef(false);
  const hasUpdatedSettlementRef = useRef(false);
  const hasDeletedSettlementRef = useRef(false);

  // Handle settlement operations from AddSettlement page via navigation state
  useEffect(() => {
    const state = location.state as {
      newSettlement?: {
        name: string;
        description: string;
        assignees: string[];
        author?: string;
        amountSpent?: string;
      };
      updatedSettlement?: {
        id: number;
        name: string;
        description: string;
        assignees: any[];
        author?: any;
        amountSpent?: string;
      };
      deleteSettlementId?: number;
    };

    if (state?.deleteSettlementId && !hasDeletedSettlementRef.current) {
      hasDeletedSettlementRef.current = true;
      setSettlements((prevSettlements) =>
        prevSettlements.filter(
          (settlement) => settlement.id !== state.deleteSettlementId
        )
      );
      navigate(location.pathname, { replace: true, state: {} });
    }

    if (state?.updatedSettlement && !hasUpdatedSettlementRef.current) {
      hasUpdatedSettlementRef.current = true;
      const { updatedSettlement } = state;

      setSettlements((prevSettlements) =>
        prevSettlements.map((settlement) => {
          if (settlement.id === updatedSettlement.id) {
            return {
              ...settlement,
              name: updatedSettlement.name,
              description: updatedSettlement.description,
              assignees: Array.isArray(updatedSettlement.assignees)
                ? updatedSettlement.assignees.map((a: any) =>
                    typeof a === "object"
                      ? { name: a.name, avatar: a.avatar }
                      : { name: a, avatar: a }
                  )
                : [],
              status: settlement.status,
              author: updatedSettlement.author || settlement.author,
              amountSpent: updatedSettlement.amountSpent,
            };
          }
          return settlement;
        })
      );
      navigate(location.pathname, { replace: true, state: {} });
    }

    if (state?.newSettlement && !hasAddedSettlementRef.current) {
      hasAddedSettlementRef.current = true;
      const { newSettlement } = state;
      console.log("Received newSettlement:", newSettlement);

      setSettlements((prevSettlements) => {
        const settlementToAdd: Settlement = {
          id: prevSettlements.length + 1,
          name: newSettlement.name,
          description: newSettlement.description,
          assignees: Array.isArray(newSettlement.assignees)
            ? newSettlement.assignees.map((a: any) =>
                typeof a === "object"
                  ? { name: a.name, avatar: a.avatar }
                  : { name: a, avatar: a }
              )
            : [],
          status: "todo",
          author:
            newSettlement.author && typeof newSettlement.author === "object"
              ? (newSettlement.author as Author)
              : undefined,
          amountSpent: newSettlement.amountSpent,
        };
        return [...prevSettlements, settlementToAdd];
      });

      navigate(location.pathname, { replace: true, state: {} });
    }

    if (
      !state?.newSettlement &&
      !state?.updatedSettlement &&
      !state?.deleteSettlementId
    ) {
      hasAddedSettlementRef.current = false;
      hasUpdatedSettlementRef.current = false;
      hasDeletedSettlementRef.current = false;
    }
  }, [location.state, navigate, location.pathname]);

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  const addSettlement = () => {
    navigate("/settlements/add");
  };

  const markSettlementAsDone = (settlementId: number) => {
    setSettlements((prevSettlements) =>
      prevSettlements.map((settlement) =>
        settlement.id === settlementId
          ? { ...settlement, status: "done" as const }
          : settlement
      )
    );
  };

  const todoSettlements = settlements.filter(
    (settlement) => settlement.status === "todo"
  );
  const doneSettlements = settlements.filter(
    (settlement) => settlement.status === "done"
  );

  const [selectedSettlement, setSelectedSettlement] =
    useState<Settlement | null>(null);
  const [showPopup, setShowPopup] = useState(false);

  const renderSettlement = (
    settlement: Settlement,
    isLast: boolean,
    isFirst: boolean
  ) => {
    return (
      <div
        className={`settlement ${isFirst ? "settlement--first" : ""} ${
          isLast ? "settlement--last" : ""
        }`}
        key={settlement.id}
        onClick={() => {
          setSelectedSettlement(settlement);
          setShowPopup(true);
        }}
        style={{ cursor: "pointer" }}
      >
        <div className="settlement --container">
          <div className="settlement__author-avatar">
            <img
              src={settlement.author?.avatar || profileIcon}
              alt={settlement.author?.name || "Author"}
              className="settlement__avatar-img"
            />
          </div>
          <div className="settlement__info">
            <p className="settlement__name">
              {settlement.amountSpent
                ? `₴${settlement.amountSpent}`
                : "No amount"}
            </p>
            <p className="settlement__description">{settlement.description}</p>
          </div>
          <span className="settlement__icon-container">
            <img
              src={settlement.status === "todo" ? settlementIcon : checkLight}
              alt="Check light icon"
            />
          </span>
        </div>
      </div>
    );
  };

  return (
    <div className="settlements">
      <div className="settlements__header">
        <h1 className="settlements__title">Settlements</h1>
        <Button variant="primary" onClick={addSettlement}>
          Add
        </Button>
      </div>

      <div className="settlements__content">
        {/* To-Do Section */}
        <div
          className={`settlements__section settlements__section--todo ${
            collapsedSections.todo ? "settlements__section--collapsed" : ""
          }`}
        >
          <h2
            className="settlements__section-title"
            onClick={() => toggleSection("todo")}
          >
            AMOUNTS DUE
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("todo")}
          >
            &#9660;
          </span>
          {!collapsedSections.todo && (
            <div className="settlements__list">
              {todoSettlements.length > 0 ? (
                todoSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === todoSettlements.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="settlements__empty-message">
                  No settlements to do
                </p>
              )}
            </div>
          )}
        </div>

        {/* Done Section */}
        <div
          className={`settlements__section settlements__section--done ${
            collapsedSections.done ? "settlements__section--collapsed" : ""
          }`}
        >
          <h2
            className="settlements__section-title"
            onClick={() => toggleSection("done")}
          >
            AMOUNTS
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("done")}
          >
            &#9660;
          </span>
          {!collapsedSections.done && (
            <div className="settlements__list">
              {doneSettlements.length > 0 ? (
                doneSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === doneSettlements.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="settlements__empty-message">
                  No completed settlements
                </p>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Settlement Details Popup */}
      {showPopup && selectedSettlement && (
        <Popup
          open={showPopup}
          onClose={() => {
            setShowPopup(false);
            setSelectedSettlement(null);
          }}
          title={selectedSettlement.name}
          variant="primary"
          actions={[
            {
              label: "Mark as done",
              onClick: () => {
                markSettlementAsDone(selectedSettlement.id);
                setShowPopup(false);
                setSelectedSettlement(null);
              },
              variant: "primary",
            },
          ]}
          customActions={
            // Show edit button only for settlement author
            user &&
            selectedSettlement.author?.name ===
              `${user.firstName} ${user.lastName}` ? (
              <div
                className="settlement-edit-button"
                onClick={() => {
                  navigate("/settlements/add", {
                    state: {
                      editSettlement: {
                        id: selectedSettlement.id,
                        name: selectedSettlement.name,
                        description: selectedSettlement.description,
                        assignees: selectedSettlement.assignees,
                        author: selectedSettlement.author,
                      },
                    },
                  });
                  setShowPopup(false);
                  setSelectedSettlement(null);
                }}
              >
                <img src={pencilLight} alt="Edit settlement" />
              </div>
            ) : null
          }
          className="settlement-details-popup settlement-details-popup--button-top"
        >
          <div className="settlement-details-content">
            <div className="settlement-details-description">
              {selectedSettlement.description}
            </div>
            <div className="participants-list assignor-list">
              <div className="participants-list__header">ASSIGNOR</div>
              <div className="participants-list__content">
                <div className="activity activity--selected">
                  <div className="activity-data">
                    <img
                      src={selectedSettlement.author?.avatar || profileIcon}
                      alt={selectedSettlement.author?.name || "User"}
                      className="assignee-avatar"
                    />
                    <p>{selectedSettlement.author?.name || "Unknown"}</p>
                  </div>
                </div>
              </div>
            </div>
            <div className="participants-list assignees-list">
              <div className="participants-list__header">ASSIGNEES</div>
              <div className="participants-list__content">
                {selectedSettlement.assignees.map((a, idx) => (
                  <div className="activity activity--selected" key={idx}>
                    <div className="activity-data">
                      <img
                        src={a.avatar}
                        alt={a.name}
                        className="assignee-avatar"
                      />
                      <p>{a.name}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
            <div className="settlement-details-footer">
              <div className="settlement-details-footer-item first">
                <div>
                  <span className="settlement-details-footer-icon">
                    <img src={starIcon} alt="star icon" />
                  </span>
                  <span className="settlement-details-footer-label">
                    POINTS
                  </span>
                </div>
              </div>
              <div className="settlement-details-footer-item second">
                <div className="settlement-details-footer-value">5 pts</div>
              </div>
            </div>
          </div>
        </Popup>
      )}

      <BottomNav />
    </div>
  );
};

export default Settlements;
