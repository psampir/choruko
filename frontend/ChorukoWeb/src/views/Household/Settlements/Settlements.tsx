import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import "./Settlements.scss";
import Button from "../../../components/Button";
import BottomNav from "../../../components/BottomNav/BottomNav";
import Popup from "../../../components/Popup/Popup";
import ErrorMessage from "../../../components/ErrorMessage";
import ViewModeToggle from "../../../components/ViewModeToggle/ViewModeToggle";
import { useUser } from "../../../contexts/UserContext";
import { useSettlement } from "../../../contexts/SettlementContext";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import settlementService from "../../../services/settlementService";
import type { SettlementListItem, Settlement as BackendSettlement } from "../../../services/settlementService";
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
  id: string;
}
interface Author {
  name: string;
  avatar: string | null;
  id: string;
}
interface UISettlement {
  id: number;
  backendId: string;
  description: string;
  assignees: Assignee[];
  status: "unpaid" | "paid";
  author: Author;
  amount: number;
  settledUsers: Assignee[];
  unsettledUsers: Assignee[];
}

const Settlements: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useUser();
  const { settlements: backendSettlements, loading, error, refreshSettlements, markAsPaid, markAsUnpaid, markAsDone, unmarkAsDone } = useSettlement();
  const { refreshHousehold } = useHousehold();
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

  const [settlements, setSettlements] = useState<UISettlement[]>([]);
  const [errorMessage, setErrorMessage] = useState("");
  const [viewMode, setViewMode] = useState<"all" | "my">("my");

  // Refresh household info and settlements when component mounts
  useEffect(() => {
    const refreshData = async () => {
      await refreshHousehold();
      await refreshSettlements();
    };
    refreshData();
  }, [refreshHousehold, refreshSettlements]);

  // Convert backend settlements to UI format
  useEffect(() => {
    const convertedSettlements: UISettlement[] = backendSettlements.map((backendSettlement: SettlementListItem, index: number) => {
      return {
        id: index,
        backendId: backendSettlement.id,
        description: backendSettlement.description,
        assignees: [],
        status: backendSettlement.status === "Completed" ? "paid" : "unpaid",
        author: {
          id: backendSettlement.creator.id,
          name: `${backendSettlement.creator.firstName} ${backendSettlement.creator.lastName}`,
          avatar: getProfilePictureUrl(backendSettlement.creator.profilePicturePath),
        },
        amount: backendSettlement.amount,
        settledUsers: (backendSettlement.settledUserIds || []).map(id => ({
          id,
          name: '',
          avatar: profileIcon,
        })),
        unsettledUsers: (backendSettlement.unsettledUserIds || []).map(id => ({
          id,
          name: '',
          avatar: profileIcon,
        })),
      };
    });
    setSettlements(convertedSettlements);
  }, [backendSettlements, profileIcon]);

  const toggleSection = (section: keyof typeof collapsedSections) => {
    setCollapsedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };

  const addSettlement = () => {
    navigate("/settlements/add");
  };

  // Categorize settlements from user's perspective
  const categorizeForUser = (settlement: UISettlement): "unpaid" | "paid" => {
    if (!user) return settlement.status;
    
    // For creators, use the settlement status
    if (settlement.author.id === user.id) {
      return settlement.status;
    }
    
    // For payers, check if they personally have paid
    const hasUserPaid = settlement.settledUsers.some(u => u.id === user.id);
    return hasUserPaid ? "paid" : "unpaid";
  };

  // In "my" view, categorize based on user's personal payment status
  // In "all" view, use the overall settlement status
  const unpaidSettlements = settlements.filter((settlement) => {
    if (viewMode === "my") {
      return categorizeForUser(settlement) === "unpaid";
    }
    return settlement.status === "unpaid";
  });

  const paidSettlements = settlements.filter((settlement) => {
    if (viewMode === "my") {
      return categorizeForUser(settlement) === "paid";
    }
    return settlement.status === "paid";
  });

  // Filter settlements: "my" shows user involvement, "all" shows everything
  const getFilteredSettlements = (settlementList: UISettlement[]) => {
    if (viewMode === "all") {
      return settlementList;
    }
    // "my" view - show only settlements where user is creator or payer
    if (!user) return [];
    return settlementList.filter(
      (settlement) => 
        settlement.author.id === user.id ||
        settlement.settledUsers.some(u => u.id === user.id) ||
        settlement.unsettledUsers.some(u => u.id === user.id)
    );
  };

  const filteredUnpaidSettlements = getFilteredSettlements(unpaidSettlements);
  const filteredPaidSettlements = getFilteredSettlements(paidSettlements);

  const [selectedSettlement, setSelectedSettlement] =
    useState<UISettlement | null>(null);
  const [selectedBackendSettlement, setSelectedBackendSettlement] =
    useState<BackendSettlement | null>(null);
  const [showPopup, setShowPopup] = useState(false);

  const renderSettlement = (
    settlement: UISettlement,
    isLast: boolean,
    isFirst: boolean
  ) => {
    // Check if user is involved in this settlement
    const isInvolved = user && (
      settlement.author.id === user.id ||
      settlement.settledUsers.some(u => u.id === user.id) ||
      settlement.unsettledUsers.some(u => u.id === user.id)
    );

    return (
      <div
        className={`settlement ${isFirst ? "settlement--first" : ""} ${isLast ? "settlement--last" : ""
          }`}
        key={settlement.id}
        onClick={async () => {
          // Fetch full settlement details with users
          const result = await settlementService.getSettlement(settlement.backendId);
          if (result.success && result.data) {
            setSelectedSettlement(settlement);
            setSelectedBackendSettlement(result.data);
            setShowPopup(true);
          } else {
            setErrorMessage(result.message || "Failed to load settlement details");
          }
        }}
        style={{ cursor: "pointer" }}
      >
        <div className="settlement --container">
          <div className="settlement__author-avatar">
            <img
              src={settlement.author.avatar || profileIcon}
              alt={settlement.author.name}
              className="settlement__avatar-img"
            />
          </div>
          <div className="settlement__info">
            <p className="settlement__name">
              {currencySymbol}{settlement.amount.toFixed(2)}
            </p>
            <p className="settlement__description">{settlement.description}</p>
          </div>
          {isInvolved && (
            <span className="settlement__icon-container">
              <img
                src={settlement.status === "unpaid" ? settlementIcon : checkIcon}
                alt="Settlement status icon"
              />
            </span>
          )}
        </div>
      </div>
    );
  };

  return (
    <div className="settlements">
      <ErrorMessage
        message={errorMessage}
        onClose={() => setErrorMessage("")}
        variant="overlay"
      />

      <div className="settlements__header">
        <h1 className="settlements__title">Settlements</h1>
        <Button variant="primary" onClick={addSettlement}>
          Add
        </Button>
      </div>

      {loading && (
        <div className="settlements__loading">
          <p>Loading settlements...</p>
        </div>
      )}

      {error && (
        <div className="settlements__error">
          <p>Error: {error}</p>
          <Button variant="primary" onClick={refreshSettlements}>
            Retry
          </Button>
        </div>
      )}

      {!loading && !error && (
        <>
          <ViewModeToggle
            viewMode={viewMode}
            onViewModeChange={setViewMode}
            myLabel="My Payments"
            allLabel="All Payments"
          />

          <div className="settlements__content">
        {/* Unpaid Section - only show if there are unpaid settlements */}
        {filteredUnpaidSettlements.length > 0 && (
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
                {filteredUnpaidSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === filteredUnpaidSettlements.length - 1,
                    index === 0
                  )
                )}
              </div>
            )}
          </div>
        )}

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
              {filteredPaidSettlements.length > 0 ? (
                filteredPaidSettlements.map((settlement, index) =>
                  renderSettlement(
                    settlement,
                    index === filteredPaidSettlements.length - 1,
                    index === 0
                  )
                )
              ) : (
                <p className="settlements__empty-message">
                  {viewMode === "my"
                    ? "No paid settlements assigned to you"
                    : "No paid settlements"}
                </p>
              )}
            </div>
          )}
        </div>
        </div>
      </>
      )}

      {/* Settlement Details Popup */}
      {showPopup && selectedSettlement && (
        <Popup
            open={showPopup}
            onClose={() => {
              setShowPopup(false);
              setSelectedSettlement(null);
              setSelectedBackendSettlement(null);
            }}
            title={
              <div>
                <div className="amount-per-person">
                  {currencySymbol}
                  {selectedBackendSettlement && (selectedBackendSettlement.unsettledUsers?.length || 0) + (selectedBackendSettlement.settledUsers?.length || 0) > 0
                    ? (selectedSettlement.amount / ((selectedBackendSettlement.unsettledUsers?.length || 0) + (selectedBackendSettlement.settledUsers?.length || 0))).toFixed(2)
                    : selectedSettlement.amount.toFixed(2)}{" "}
                  per person
                </div>
                <div className="total-amount">
                  Total of {currencySymbol}{selectedSettlement.amount.toFixed(2)}
                </div>
              </div>
            }
            variant="primary"
            actions={(() => {
              if (!selectedBackendSettlement || !user) return [];
              
              const isCreator = selectedSettlement.author.id === user.id;
              const isPayer = selectedBackendSettlement.unsettledUsers?.some(u => u.id === user.id) || 
                             selectedBackendSettlement.settledUsers?.some(u => u.id === user.id);
              const hasUserPaid = selectedBackendSettlement.settledUsers?.some(u => u.id === user.id);
              
              // Creator sees "Mark as done/undone" buttons
              if (isCreator) {
                if (selectedSettlement.status === "unpaid") {
                  return [{
                    label: "Mark as done",
                    onClick: async () => {
                      await markAsDone(selectedBackendSettlement.id);
                      setShowPopup(false);
                      setSelectedSettlement(null);
                      setSelectedBackendSettlement(null);
                    },
                    variant: "primary" as const,
                  }];
                } else {
                  return [{
                    label: "Mark as undone",
                    onClick: async () => {
                      await unmarkAsDone(selectedBackendSettlement.id);
                      setShowPopup(false);
                      setSelectedSettlement(null);
                      setSelectedBackendSettlement(null);
                    },
                    variant: "primary" as const,
                  }];
                }
              }
              
              // Payers see "Mark as paid/unpaid" buttons
              if (isPayer) {
                if (hasUserPaid) {
                  return [{
                    label: "Mark as unpaid",
                    onClick: async () => {
                      await markAsUnpaid(selectedBackendSettlement.id);
                      setShowPopup(false);
                      setSelectedSettlement(null);
                      setSelectedBackendSettlement(null);
                    },
                    variant: "primary" as const,
                  }];
                } else {
                  return [{
                    label: "Mark as paid",
                    onClick: async () => {
                      await markAsPaid(selectedBackendSettlement.id);
                      setShowPopup(false);
                      setSelectedSettlement(null);
                      setSelectedBackendSettlement(null);
                    },
                    variant: "primary" as const,
                  }];
                }
              }
              
              return [];
            })()}
            customActions={
              // Show edit button only for settlement author and not for paid settlements
              user &&
                selectedSettlement.author.id === user.id &&
                selectedSettlement.status !== "paid" ? (
                <div
                  className="settlement-edit-button"
                  onClick={() => {
                    navigate("/settlements/add", {
                      state: {
                        editSettlement: selectedBackendSettlement,
                      },
                    });
                    setShowPopup(false);
                    setSelectedSettlement(null);
                    setSelectedBackendSettlement(null);
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
                        src={selectedSettlement.author.avatar || profileIcon}
                        alt={selectedSettlement.author.name}
                        className="assignee-avatar"
                      />
                      <p>{selectedSettlement.author.name}</p>
                    </div>
                  </div>
                </div>
              </div>
              {/* Unsettled Section */}
              {selectedBackendSettlement?.unsettledUsers && selectedBackendSettlement.unsettledUsers.length > 0 && (
                <div className="participants-list unsettled-list">
                  <div className="participants-list__header">UNSETTLED</div>
                  <div className="participants-list__content">
                    {selectedBackendSettlement.unsettledUsers.map((u, idx) => (
                      <div className="activity activity--selected" key={idx}>
                        <div className="activity-data">
                          <img
                            src={getProfilePictureUrl(u.profilePicturePath) || profileIcon}
                            alt={`${u.firstName} ${u.lastName}`}
                            className="assignee-avatar"
                          />
                          <p>{u.firstName} {u.lastName}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
              {/* Settled Section */}
              {selectedBackendSettlement?.settledUsers && selectedBackendSettlement.settledUsers.length > 0 && (
                <div className="participants-list assignees-list">
                  <div className="participants-list__header">SETTLED</div>
                  <div className="participants-list__content">
                    {selectedBackendSettlement.settledUsers.map((u, idx) => (
                      <div className="activity activity--selected" key={idx}>
                        <div className="activity-data">
                          <img
                            src={getProfilePictureUrl(u.profilePicturePath) || profileIcon}
                            alt={`${u.firstName} ${u.lastName}`}
                            className="assignee-avatar"
                          />
                          <p>{u.firstName} {u.lastName}</p>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </Popup>
      )}

      <BottomNav />
    </div>
  );
};

export default Settlements;
