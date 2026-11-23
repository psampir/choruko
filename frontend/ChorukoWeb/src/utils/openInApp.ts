import { isShellApp } from "./nativeApp";

const ANDROID_APP_PACKAGE = "psampir.choruko";

/**
 * Android browsers cannot see which apps are installed, so "detection" is an
 * attempt to open the app: if it is there, the system opens it, otherwise the
 * browser stays on the page. Chromium browsers honour intent:// with a silent
 * fallback; Firefox has no intent:// and would show an "unhandled link" page, so
 * there the user taps a button instead of us navigating on load.
 */
export const isAndroidBrowser = (): boolean =>
  /android/i.test(window.navigator.userAgent) && !isShellApp();

export const isFirefoxAndroid = (): boolean =>
  /firefox|fxios/i.test(window.navigator.userAgent);

const currentTarget = (): { origin: string; path: string } => ({
  origin: window.location.origin,
  path: `${window.location.pathname}${window.location.search}`,
});

/** chorukoapp://open?server=…&path=… — handled by MainActivity in the shell. */
export const buildAppLink = (): string => {
  const { origin, path } = currentTarget();
  const params = new URLSearchParams({ server: origin, path });

  return `chorukoapp://open?${params.toString()}`;
};

/**
 * Tries to hand the current page over to the app. Returns what was attempted so
 * callers can show a hint when nothing happens.
 */
export const openInApp = (): "intent" | "scheme" | "none" => {
  if (!isAndroidBrowser()) {
    return "none";
  }

  const { origin, path } = currentTarget();
  const params = new URLSearchParams({ server: origin, path });

  if (isFirefoxAndroid()) {
    window.location.href = buildAppLink();
    return "scheme";
  }

  const fallback = encodeURIComponent(window.location.href);
  window.location.href =
    `intent://open?${params.toString()}` +
    `#Intent;scheme=chorukoapp;package=${ANDROID_APP_PACKAGE};` +
    `S.browser_fallback_url=${fallback};end`;

  return "intent";
};

/**
 * Runs once per browser session on Android Chromium: opens the app when it is
 * installed and does nothing when it is not. Skipped inside the shell (the SPA is
 * already running there) and when the user asked to stay in the browser.
 */
export const tryOpenInAppOnStart = (): void => {
  if (!isAndroidBrowser() || isFirefoxAndroid()) {
    return;
  }

  const query = new URLSearchParams(window.location.search);
  if (query.has("stay")) {
    sessionStorage.setItem("choruko-stay-browser", "1");
    return;
  }

  if (
    sessionStorage.getItem("choruko-stay-browser") ||
    sessionStorage.getItem("choruko-open-app-attempted")
  ) {
    return;
  }

  sessionStorage.setItem("choruko-open-app-attempted", "1");
  openInApp();
};
