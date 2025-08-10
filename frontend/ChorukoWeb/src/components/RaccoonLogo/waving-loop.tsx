import React, { useEffect, useRef, useState } from "react";
import raccoonWavingSvg from "../../assets/svg/raccoon-waving.svg"; 
interface RaccoonAnimationControllerProps {
  svgPath?: string;
  restartInterval?: number;
  className?: string;
}

class RaccoonAnimationController {
  private container: HTMLElement;
  private svg: SVGElement | null = null;
  private loopInterval: ReturnType<typeof setInterval> | null = null;
  private restartInterval: number;

  constructor(container: HTMLElement, restartInterval: number = 5000) {
    this.container = container;
    this.restartInterval = restartInterval;
  }

  async loadSVG(svgPath: string): Promise<void> {
    try {
      const response = await fetch(svgPath);
      if (!response.ok) {
        throw new Error(`Failed to load SVG: ${response.statusText}`);
      }

      // Extract the SVG text and replace the container's content
      const svgText = await response.text();
      this.container.innerHTML = svgText;

      this.svg = this.container.querySelector("svg");
      if (!this.svg) {
        throw new Error("SVG element not found in loaded content");
      }

      // Start the infinite animation loop
      this.startInfiniteLoop(this.restartInterval, svgPath);
    } catch (error) {
      console.error("Failed to load raccoon SVG:", error);
      this.container.innerHTML =
        "<p>Failed to load animation. Please check the console for details.</p>";
    }
  }

  async reloadSVG(svgPath: string): Promise<void> {
    // Reload the entire SVG to restart all animations cleanly
    try {
      const response = await fetch(svgPath);
      if (!response.ok) {
        throw new Error(`Failed to reload SVG: ${response.statusText}`);
      }

      const svgText = await response.text();
      this.container.innerHTML = svgText;
    } catch (error) {
      console.error("Failed to reload SVG:", error);
    }
  }

  startInfiniteLoop(
    interval: number = this.restartInterval,
    svgPath: string
  ): void {
    // Clear existing interval if any
    if (this.loopInterval) {
      clearInterval(this.loopInterval);
    }

    // Set up interval to reload the SVG with the specified interval
    this.loopInterval = setInterval(async () => {
      await this.reloadSVG(svgPath);
    }, interval);
  }

  stopAnimation(): void {
    if (this.loopInterval) {
      clearInterval(this.loopInterval);
      this.loopInterval = null;
    }
  }

  destroy(): void {
    this.stopAnimation();
    if (this.container) {
      this.container.innerHTML = "";
    }
  }
}

const RaccoonAnimation: React.FC<RaccoonAnimationControllerProps> = ({
  svgPath = raccoonWavingSvg,
  restartInterval = 5000,
  className = "",
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const controllerRef = useRef<RaccoonAnimationController | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const initializeController = async () => {
      if (!containerRef.current) return;

      try {
        setIsLoading(true);
        setError(null);

        const controller = new RaccoonAnimationController(
          containerRef.current,
          restartInterval
        );

        controllerRef.current = controller;

        // Make controller available globally for debugging
        (
          window as Window & { raccoonController?: RaccoonAnimationController }
        ).raccoonController = controller;

        await controller.loadSVG(svgPath);
        setIsLoading(false);
      } catch (err) {
        console.error(
          "Failed to initialize raccoon animation controller:",
          err
        );
        setError(err instanceof Error ? err.message : "Unknown error occurred");
        setIsLoading(false);
      }
    };

    initializeController();

    // Cleanup function
    return () => {
      if (controllerRef.current) {
        controllerRef.current.destroy();
        controllerRef.current = null;
      }
    };
  }, [svgPath, restartInterval]);

  return (
    <div className={`raccoon-animation-container ${className}`}>
      {isLoading && <div className="loading">Loading animation...</div>}
      {error && <div className="error">Error: {error}</div>}
      <div ref={containerRef} id="svgContainer" className="svg-container" />
    </div>
  );
};

export default RaccoonAnimation;
