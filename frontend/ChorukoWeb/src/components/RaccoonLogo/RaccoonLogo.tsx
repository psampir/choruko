import React from "react";
import "./RaccoonLogo.scss";
import RaccoonAnimation from './waving-loop';
import raccoonSvg from "../../assets/svg/raccoon-waving.svg"

interface RaccoonLogoProps {
  className?: string;
}

const RaccoonLogo: React.FC<RaccoonLogoProps> = ({ className }) => {
  return (
    <div className={`logo ${className}`}>
      <RaccoonAnimation
        svgPath={raccoonSvg}
        restartInterval={8000}
        className="logo__raccoon"
      />{" "}
    </div>
  );
};

export default RaccoonLogo;
