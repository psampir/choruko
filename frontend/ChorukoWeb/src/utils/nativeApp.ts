/**
 * True when the page runs inside the Android shell (android/) rather than a browser.
 * The shell appends "ChorukoApp/<version>" to the user agent; Capacitor is still
 * checked so a future wrapper keeps working.
 */
export const isShellApp = (): boolean => {
  if (/chorukoapp\//i.test(window.navigator.userAgent)) {
    return true;
  }

  const capacitor = (
    window as Window & { Capacitor?: { isNativePlatform?: () => boolean } }
  ).Capacitor;

  return capacitor?.isNativePlatform?.() ?? false;
};
