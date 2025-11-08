import React, { useState, useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import "./Settlements.scss";
import Button from "../../../components/Button";
import BottomNav from "../../../components/BottomNav/BottomNav";
import Popup from "../../../components/Popup/Popup";
import { useUser } from "../../../contexts/UserContext";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import profileIconLight from "../../../assets/svg/light/profile-icon.svg";
import profileIconDark from "../../../assets/svg/dark/profile-icon.svg";
import pencilLight from "../../../assets/svg/light/pencil-light.svg";
import pencilDark from "../../../assets/svg/dark/pencil-light.svg";
import checkLight from "../../../assets/svg/light/check.svg";
import checkDark from "../../../assets/svg/dark/check.svg";
import settlementIconLight from "../../../assets/svg/light/expenses.svg";
import settlementIconDark from "../../../assets/svg/dark/expenses.svg";

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
  status: "unpaid" | "paid";
  author?: Author;
  amountSpent?: string;
  paidBy?: string[]; // Array of assignee names who have paid
}

const Settlements: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useUser();
  const isDark = useIsDarkTheme();

  const profileIcon = isDark ? profileIconDark : profileIconLight;
  const pencilIcon = isDark ? pencilDark : pencilLight;
  const checkIcon = isDark ? checkDark : checkLight;
  const settlementIcon = isDark ? settlementIconDark : settlementIconLight;

  // Currency symbol - can be made dynamic based on user preferences/household settings
  const currencySymbol = "₴";

  const [collapsedSections, setCollapsedSections] = useState({
    unpaid: false,
    paid: false,
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
        status: "unpaid",
        author: {
          name: "Andreas Toretto",
          avatar: "https://randomuser.me/api/portraits/men/1.jpg",
        },
        amountSpent: "150.50",
        paidBy: ["Andreas Toretto"], // John Doe hasn't paid yet
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
        status: "unpaid",
        author: {
          name: "Gabby Mayers",
          avatar: "https://randomuser.me/api/portraits/women/2.jpg",
        },
        amountSpent: "85.00",
        paidBy: ["Anna Lee"], // Anna Lee has paid
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
        status: "paid",
        author: {
          name: "Ann Huang",
          avatar: "https://randomuser.me/api/portraits/women/1.jpg",
        },
        amountSpent: "1200.00",
        paidBy: ["Andreas Toretto", "John Doe"], // Jane Smith hasn't paid yet
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
          status: "unpaid",
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

  const markSettlementAsPaid = (settlementId: number) => {
    setSettlements((prevSettlements) =>
      prevSettlements.map((settlement) =>
        settlement.id === settlementId
          ? { ...settlement, status: "paid" as const }
          : settlement
      )
    );
  };

  const unpaidSettlements = settlements.filter(
    (settlement) => settlement.status === "unpaid"
  );
  const paidSettlements = settlements.filter(
    (settlement) => settlement.status === "paid"
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
        className={`settlement ${isFirst ? "settlement--first" : ""} ${isLast ? "settlement--last" : ""
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
                ? `${currencySymbol}${settlement.amountSpent}`
                : "No amount"}
            </p>
            <p className="settlement__description">{settlement.description}</p>
          </div>
          <span className="settlement__icon-container">
            <img
              src={settlement.status === "unpaid" ? settlementIcon : checkIcon}
              alt="Settlement status icon"
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
        {/* Unpaid Section */}
        <div
          className={`settlements__section settlements__section--unpaid ${collapsedSections.unpaid ? "settlements__section--collapsed" : ""
            }`}
        >
          <h2
            className="settlements__section-title"
            onClick={() => toggleSection("unpaid")}
          >
            AMOUNTS DUE
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("unpaid")}
          >
            &#9660;
          </span>
          {!collapsedSections.unpaid && (
            <div className="settlements__list">
              {unpaidSettlements.length > 0 ? (
                unpaidSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === unpaidSettlements.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="settlements__empty-message">
                  No unpaid settlements
                </p>
              )}
            </div>
          )}
        </div>

        {/* Paid Section */}
        <div
          className={`settlements__section settlements__section--paid ${collapsedSections.paid ? "settlements__section--collapsed" : ""
            }`}
        >
          <h2
            className="settlements__section-title"
            onClick={() => toggleSection("paid")}
          >
            AMOUNTS PAID
          </h2>
          <span
            className="dropdown__arrow"
            onClick={() => toggleSection("paid")}
          >
            &#9660;
          </span>
          {!collapsedSections.paid && (
            <div className="settlements__list">
              {paidSettlements.length > 0 ? (
                paidSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === paidSettlements.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="settlements__empty-message">
                  No paid settlements
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
          title={
            <div>
              <div className="amount-per-person">
                {currencySymbol}
                {selectedSettlement.amountSpent &&
                  selectedSettlement.assignees.length > 0
                  ? (
                    parseFloat(selectedSettlement.amountSpent) /
                    selectedSettlement.assignees.length
                  ).toFixed(2)
                  : "0.00"}{" "}
                per person
              </div>
              <div className="total-amount">
                Total of {currencySymbol}
                {selectedSettlement.amountSpent || "0.00"}
              </div>
            </div>
          }
          variant="primary"
          actions={
            selectedSettlement.status !== "paid"
              ? [
                {
                  label: "Mark as paid",
                  onClick: () => {
                    markSettlementAsPaid(selectedSettlement.id);
                    setShowPopup(false);
                    setSelectedSettlement(null);
                  },
                  variant: "primary",
                },
              ]
              : []
          }
          customActions={
            // Show edit button only for settlement author and not for paid settlements
            user &&
              selectedSettlement.author?.name ===
              `${user.firstName} ${user.lastName}` &&
              selectedSettlement.status !== "paid" ? (
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
                        amountSpent: selectedSettlement.amountSpent,
                      },
                    },
                  });
                  setShowPopup(false);
                  setSelectedSettlement(null);
                }}
              >
                <img src={pencilIcon} alt="Edit settlement" />
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
              <div className="participants-list__header">BUYER</div>
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
            {/* Unsettled Section - Show assignees who haven't paid yet (excluding buyer) */}
            {(() => {
              const paidBy = selectedSettlement.paidBy || [];
              const buyerName = selectedSettlement.author?.name;
              const unsettledAssignees = selectedSettlement.assignees.filter(
                (assignee) =>
                  !paidBy.includes(assignee.name) && assignee.name !== buyerName
              );

              return unsettledAssignees.length > 0 ? (
                <div className="participants-list unsettled-list">
                  <div className="participants-list__header">UNSETTLED</div>
                  <div className="participants-list__content">
                    {unsettledAssignees.map((a, idx) => (
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
              ) : null;
            })()}
            {/* Settled Section - Show assignees who have paid (excluding buyer) */}
            {(() => {
              const paidBy = selectedSettlement.paidBy || [];
              const buyerName = selectedSettlement.author?.name;
              const settledAssignees = selectedSettlement.assignees.filter(
                (assignee) =>
                  paidBy.includes(assignee.name) && assignee.name !== buyerName
              );

              return settledAssignees.length > 0 ? (
                <div className="participants-list assignees-list">
                  <div className="participants-list__header">SETTLED</div>
                  <div className="participants-list__content">
                    {settledAssignees.map((a, idx) => (
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
              ) : null;
            })()}
          </div>
        </Popup>
      )}

      <BottomNav />
    </div>
  );
};

export default Settlements;
