import React, { useState, useEffect, useRef } from "react";
import BottomNav from "../../../components/BottomNav/BottomNav";
import star from "../../../assets/svg/star.svg";
import householdDefaultIcon from "../../../assets/svg/householdDefault.svg";
import householdService from "../../../services/householdService";
import type { HouseholdInfo } from "../../../services/householdService";
import "./Leaderboard.scss";

// Mock data for the leaderboard
const mockParticipants = [
  {
    id: 1,
    name: "Andreas Toretto",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 64,
    rank: 1,
  },
  {
    id: 2,
    name: "Ann Huang",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 32,
    rank: 2,
  },
  {
    id: 3,
    name: "Gabbie Mayers",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 16,
    rank: 3,
  },
  {
    id: 4,
    name: "John Smith",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 12,
    rank: 4,
  },
  {
    id: 5,
    name: "Sarah Wilson",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 10,
    rank: 5,
  },
  {
    id: 6,
    name: "Mike Johnson",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 8,
    rank: 6,
  },
  {
    id: 7,
    name: "Lisa Brown",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 6,
    rank: 7,
  },
  {
    id: 8,
    name: "David Lee",
    avatar: "/src/assets/svg/profile-icon.svg",
    points: 4,
    rank: 8,
  },
];

type TimeFilter = "CURRENT MONTH" | "CURRENT YEAR" | "ALL TIME";

const Leaderboard: React.FC = () => {
  const [activeFilter, setActiveFilter] = useState<TimeFilter>("CURRENT MONTH");
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [householdInfo, setHouseholdInfo] = useState<HouseholdInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const participantsRef = useRef<HTMLDivElement>(null);
  const scrollableContentRef = useRef<HTMLDivElement>(null);
  const participantRef = useRef<HTMLDivElement>(null);

  // Fetch household information
  useEffect(() => {
    const fetchHouseholdInfo = async () => {
      setLoading(true);
      const result = await householdService.getHouseholdInfo();
      if (result.success && result.data) {
        setHouseholdInfo(result.data);
      }
      setLoading(false);
    };

    fetchHouseholdInfo();
  }, []);

  const handleFilterChange = (filter: TimeFilter) => {
    setActiveFilter(filter);
    setIsDropdownOpen(false);
  };

  // Calculate dynamic height after component mounts
  useEffect(() => {
    const calculateDynamicHeight = () => {
      if (
        participantRef.current &&
        participantsRef.current &&
        scrollableContentRef.current
      ) {
        const participantHeight = participantRef.current.offsetHeight;
        const desiredVisibleCount = 3;
        const maxHeight = participantHeight * desiredVisibleCount;

        // Set CSS custom property for dynamic height
        participantsRef.current.style.setProperty(
          "--dynamic-participant-height",
          `${participantHeight}px`
        );
        participantsRef.current.style.setProperty(
          "--max-visible-participants",
          `${desiredVisibleCount}`
        );

        // Set max height on both containers
        participantsRef.current.style.maxHeight = `${maxHeight}px`;
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
  }, [mockParticipants]);

  return (
    <div className="leaderboard">
      <div className="household-section">
        <img
          className="household-image"
          src={householdInfo?.profilePictureUrl || householdDefaultIcon}
          alt="Household"
        />
        <h1 className="household-name">
          {loading ? "Loading..." : householdInfo?.name || "No Household"}
        </h1>
        <p className="leaderboard-text">LEADERBOARD</p>
      </div>

      {/* Time Filter Dropdown */}
      <div className="time-filter">
        <div
          className={`dropdown__container ${
            isDropdownOpen ? "dropdown__container--open" : ""
          }`}
        >
          <div
            className={`dropdown ${isDropdownOpen ? "dropdown--open" : ""}`}
            onClick={() => setIsDropdownOpen(!isDropdownOpen)}
          >
            <div className="dropdown__selected">
              <span className="dropdown__arrow">&#9660;</span>
              <span className="dropdown__text">{activeFilter}</span>
            </div>
            {isDropdownOpen && (
              <div className="dropdown__options">
                {(["CURRENT MONTH", "CURRENT YEAR", "ALL TIME"] as TimeFilter[])
                  .filter((filter) => filter !== activeFilter) // Hide currently selected option
                  .map((filter) => (
                    <div
                      key={filter}
                      className="dropdown__option"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleFilterChange(filter);
                      }}
                    >
                      {filter}
                    </div>
                  ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Participants List */}
      <div className="participants" ref={participantsRef}>
        <div className="scrollable-content" ref={scrollableContentRef}>
          {mockParticipants.map((participant, index) => (
            <div
              key={participant.id}
              className="participant"
              ref={index === 0 ? participantRef : null} // Use first participant for measurement
            >
              <div className="participant__rank">#{participant.rank}</div>
              <img
                src={participant.avatar}
                alt={participant.name}
                className="participant__avatar"
              />
              <div className="participant-profile__container">
                <span className="participant__name">{participant.name}</span>
                <div className="participant__points">
                  <img className="star" src={star} alt="star" />
                  <span className="points">{participant.points}</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <BottomNav />
    </div>
  );
};

export default Leaderboard;
