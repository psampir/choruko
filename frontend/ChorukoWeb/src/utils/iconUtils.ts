import { useTheme } from "../contexts/ThemeContext";

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
