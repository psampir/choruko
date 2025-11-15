import React, { useState, useEffect, useRef, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { useIsDarkTheme } from "../../../utils/iconUtils";
import starLight from "../../../assets/svg/light/star.svg";
import householdDefaultIconLight from "../../../assets/svg/light/householdDefault.svg";
import profileDefaultIconLight from "../../../assets/svg/light/profile-icon.svg";
import starDark from "../../../assets/svg/dark/star.svg";
import householdDefaultIconDark from "../../../assets/svg/dark/householdDefault.svg";
import profileDefaultIconDark from "../../../assets/svg/dark/profile-icon.svg";
import { useHousehold } from "../../../contexts/HouseholdContext";
import { getProfilePictureUrl } from "../../../utils/imageUtils";
import type { HouseholdParticipant } from "../../../services/householdService";
import BottomNav from "../../../components/BottomNav/BottomNav";
import "./Leaderboard.scss";

type TimeFilter = "CURRENT MONTH" | "CURRENT YEAR" | "ALL TIME";

// Ranked household participants display with time-based filtering
const Leaderboard: React.FC = () => {
  const navigate = useNavigate();
  const [activeFilter, setActiveFilter] = useState<TimeFilter>("ALL TIME");
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const { householdInfo, participants, loading, error, refreshHousehold } =
    useHousehold();
  const isDark = useIsDarkTheme();

  // Select theme-appropriate icons
  const star = isDark ? starDark : starLight;
  const householdDefaultIcon = isDark
    ? householdDefaultIconDark
    : householdDefaultIconLight;
  const profileDefaultIcon = isDark
    ? profileDefaultIconDark
    : profileDefaultIconLight;

  // Refs for calculating dynamic participant list height based on individual item size
  const participantsRef = useRef<HTMLDivElement>(null);
  const scrollableContentRef = useRef<HTMLDivElement>(null);
  const participantRef = useRef<HTMLDivElement>(null);

  // Refresh household info when component mounts
  useEffect(() => {
    refreshHousehold();
  }, [refreshHousehold]);

  const handleFilterChange = (filter: TimeFilter) => {
    setActiveFilter(filter);
    setIsDropdownOpen(false);
  };

  // Sort participants by points for selected time period and assign ranks
  const sortedParticipants = useMemo(() => {
    if (!participants || participants.length === 0) return [];

    // Select appropriate points field based on time filter
    const getPoints = (participant: HouseholdParticipant): number => {
      switch (activeFilter) {
        case "CURRENT MONTH":
          return participant.pointsCurrentMonth;
        case "CURRENT YEAR":
          return participant.pointsCurrentYear;
        case "ALL TIME":
          return participant.pointsAllTime;
        default:
          return participant.pointsAllTime;
      }
    };

    // Sort by points descending and assign ranks
    const sorted = [...participants]
      .map((participant) => ({
        ...participant,
        displayPoints: getPoints(participant),
        rank: 0,
      }))
      .sort((a, b) => b.displayPoints - a.displayPoints)
      .map((participant, index) => ({
        ...participant,
        rank: index + 1,
      }));

    return sorted;
  }, [participants, activeFilter]);

  // Redirect if user was removed from household
  useEffect(() => {
    if (!loading && error && error.includes("not part of any household")) {
      navigate("/joinOrCreateHousehold", { replace: true });
    }
  }, [error, loading, navigate]);

  // Calculate dynamic height to show 3 participants, recalculate on resize
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

        // Set CSS custom properties for dynamic styling
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

    // Delay ensures DOM is fully rendered before measuring
    const timer = setTimeout(calculateDynamicHeight, 100);

    // Recalculate on window resize for responsive behavior
    window.addEventListener("resize", calculateDynamicHeight);

    return () => {
      clearTimeout(timer);
      window.removeEventListener("resize", calculateDynamicHeight);
    };
  }, [sortedParticipants]);

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
              <span className="dropdown__arrow">▼</span>
              <span className="dropdown__text">{activeFilter}</span>
            </div>
            {isDropdownOpen && (
              <div className="dropdown__options">
                {(["CURRENT MONTH", "CURRENT YEAR", "ALL TIME"] as TimeFilter[])
                  .filter((filter) => filter !== activeFilter) // Only show unselected options
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
          {loading ? (
            <div className="loading-message">Loading participants...</div>
          ) : sortedParticipants.length === 0 ? (
            <div className="empty-message">No participants to display</div>
          ) : (
            sortedParticipants.map((participant, index) => (
              <div
                key={participant.id}
                className="participant"
                ref={index === 0 ? participantRef : null}
              >
                <div className="participant__rank">#{participant.rank}</div>
                <img
                  src={
                    getProfilePictureUrl(participant.profilePicturePath) ||
                    profileDefaultIcon
                  }
                  alt={`${participant.firstName} ${participant.lastName}`}
                  className="participant__avatar"
                />
                <div className="participant-profile__container">
                  <span className="participant__name">
                    {participant.firstName} {participant.lastName}
                  </span>
                  <div className="participant__points">
                    <img className="star" src={star} alt="star" />
                    <span className="points">{participant.displayPoints}</span>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default Leaderboard;
