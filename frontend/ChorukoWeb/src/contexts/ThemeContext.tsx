import React, { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";

export type ThemeType = "light" | "dark" | "system";

interface ThemeContextType {
  theme: ThemeType;
  setTheme: (theme: ThemeType) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

const THEME_STORAGE_KEY = "choruko-theme-preference";

const applyTheme = (theme: ThemeType) => {
  const root = document.documentElement;

  const getSystemTheme = (): "light" | "dark" => {
    return window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light";
  };

  const effectiveTheme = theme === "system" ? getSystemTheme() : theme;

  // Set data-theme attribute for CSS selectors
  root.setAttribute("data-theme", effectiveTheme);

  if (effectiveTheme === "dark") {
    root.style.setProperty("--color-primary", "var(--color-dark-primary)");
    root.style.setProperty("--color-secondary", "var(--color-dark-secondary)");
    root.style.setProperty(
      "--color-secondary-half-tr",
      "var(--color-dark-secondary-half-tr)"
    );
    root.style.setProperty(
      "--color-background",
      "var(--color-dark-background)"
    );
    root.style.setProperty(
      "--color-background-secondary",
      "var(--color-dark-background-secondary)"
    );
    root.style.setProperty(
      "--color-text-primary",
      "var(--color-dark-text-primary)"
    );
    root.style.setProperty(
      "--color-text-secondary",
      "var(--color-dark-text-secondary)"
    );
    root.style.setProperty("--color-white", "var(--color-dark-white)");
    root.style.setProperty("--color-black", "var(--color-dark-black)");
    root.style.setProperty("--color-error-bg", "var(--color-dark-error-bg)");
    root.style.setProperty(
      "--color-error-bg-secondary",
      "var(--color-dark-error-bg-secondary)"
    );
  } else {
    // Light mode: Reference light theme variables
    root.style.setProperty("--color-primary", "var(--color-light-primary)");
    root.style.setProperty("--color-secondary", "var(--color-light-secondary)");
    root.style.setProperty(
      "--color-secondary-half-tr",
      "var(--color-light-secondary-half-tr)"
    );
    root.style.setProperty(
      "--color-background",
      "var(--color-light-background)"
    );
    root.style.setProperty(
      "--color-background-secondary",
      "var(--color-light-background-secondary)"
    );
    root.style.setProperty(
      "--color-text-primary",
      "var(--color-light-text-primary)"
    );
    root.style.setProperty(
      "--color-text-secondary",
      "var(--color-light-text-secondary)"
    );
    root.style.setProperty("--color-white", "var(--color-light-white)");
    root.style.setProperty("--color-black", "var(--color-light-black)");
    root.style.setProperty("--color-error-bg", "var(--color-light-error-bg)");
    root.style.setProperty(
      "--color-error-bg-secondary",
      "var(--color-light-error-bg-secondary)"
    );
  }
};

export const ThemeProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [theme, setThemeState] = useState<ThemeType>(() => {
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);
    const initialTheme = (savedTheme as ThemeType) || "light";
    // Apply theme immediately during initialization
    applyTheme(initialTheme);
    return initialTheme;
  });

  const setTheme = (newTheme: ThemeType) => {
    setThemeState(newTheme);
    localStorage.setItem(THEME_STORAGE_KEY, newTheme);
    applyTheme(newTheme);
  };

  useEffect(() => {
    // Only listen for system theme changes, don't re-apply the initial theme
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const handleSystemThemeChange = () => {
      if (theme === "system") {
        applyTheme("system");
      }
    };

    mediaQuery.addEventListener("change", handleSystemThemeChange);
    return () =>
      mediaQuery.removeEventListener("change", handleSystemThemeChange);
  }, [theme]);

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return context;
};
