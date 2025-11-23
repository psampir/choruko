import React from "react";
import "./ViewModeToggle.scss";

interface ViewModeToggleProps {
  viewMode: "all" | "my";
  onViewModeChange: (mode: "all" | "my") => void;
  myLabel?: string;
  allLabel?: string;
}

const ViewModeToggle: React.FC<ViewModeToggleProps> = ({
  viewMode,
  onViewModeChange,
  myLabel = "My Items",
  allLabel = "All Items",
}) => {
  return (
    <div className="view-toggle">
      <button
        className={`view-toggle__button ${
          viewMode === "my" ? "view-toggle__button--active" : ""
        }`}
        onClick={() => onViewModeChange("my")}
      >
        {myLabel}
      </button>
      <button
        className={`view-toggle__button ${
          viewMode === "all" ? "view-toggle__button--active" : ""
        }`}
        onClick={() => onViewModeChange("all")}
      >
        {allLabel}
      </button>
    </div>
  );
};

export default ViewModeToggle;
