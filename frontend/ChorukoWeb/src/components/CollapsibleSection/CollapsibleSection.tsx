import React from "react";
import "./CollapsibleSection.scss";

interface CollapsibleSectionProps {
  title: string;
  isCollapsed: boolean;
  onToggle: () => void;
  variant?: "default" | "error";
  children: React.ReactNode;
  className?: string;
}

const CollapsibleSection: React.FC<CollapsibleSectionProps> = ({
  title,
  isCollapsed,
  onToggle,
  variant = "default",
  children,
  className = "",
}) => {
  return (
    <div
      className={`collapsible-section collapsible-section--${variant} ${
        isCollapsed ? "collapsible-section--collapsed" : ""
      } ${className}`}
    >
      <h2 className="collapsible-section__title" onClick={onToggle}>
        {title}
      </h2>
      <span className="collapsible-section__arrow" onClick={onToggle}>
        &#9660;
      </span>
      {!isCollapsed && (
        <div className="collapsible-section__content">{children}</div>
      )}
    </div>
  );
};

export default CollapsibleSection;
