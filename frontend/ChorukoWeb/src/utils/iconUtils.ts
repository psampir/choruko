import { useTheme } from "../contexts/ThemeContext";
import { useMemo } from "react";

// Helper to determine if dark theme is active
export const useIsDarkTheme = (): boolean => {
  const { theme } = useTheme();

  const getSystemTheme = (): "light" | "dark" => {
    return window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  };

  const effectiveTheme = theme === "system" ? getSystemTheme() : theme;
  return effectiveTheme === "dark";
};

// Helper to get the current theme directory
export const useThemeDir = (): "light" | "dark" => {
  const isDark = useIsDarkTheme();
  return isDark ? "dark" : "light";
};

// Hook to dynamically import themed icons
export const useThemedIcon = (iconName: string) => {
  const themeDir = useThemeDir();
  
  return useMemo(() => {
    // For raccoon icons, use the root directory
    if (iconName.includes("raccoon")) {
      return `/src/assets/svg/${iconName}`;
    }
    // For all other icons, use theme directory
    return `/src/assets/svg/${themeDir}/${iconName}`;
  }, [iconName, themeDir]);
};

