import React, { useEffect, useState } from "react";
import Popup from "../Popup/Popup";
import Button from "../Button";
import "./InstallHint.scss";

interface InstallHintProps {
  open: boolean;
  onClose: () => void;
}

const RELEASES_URL = "https://github.com/psampir/choruko/releases/latest";

const InstallHint: React.FC<InstallHintProps> = ({ open, onClose }) => {
  const [platform, setPlatform] = useState<"ios" | "android" | "other">("other");
  const [isNative, setIsNative] = useState(false);

  useEffect(() => {
    const capacitor = (
      window as Window & { Capacitor?: { isNativePlatform?: () => boolean } }
    ).Capacitor;
    // The Android shell appends "ChorukoApp/<version>" to the user agent.
    const isShellApp =
      /chorukoapp\//i.test(window.navigator.userAgent) ||
      (capacitor?.isNativePlatform?.() ?? false);
    setIsNative(isShellApp);

    const userAgent = window.navigator.userAgent.toLowerCase();
    if (/iphone|ipad|ipod/.test(userAgent)) {
      setPlatform("ios");
    } else if (/android/.test(userAgent)) {
      setPlatform("android");
    }
  }, []);

  if (isNative || platform === "other") {
    return null;
  }

  return (
    <Popup
      title="Install Choruko"
      open={open}
      onClose={onClose}
      variant="primary"
      className="install-hint"
    >
      {platform === "ios" ? (
        <div className="install-hint__section">
          <p>Add Choruko to your home screen for a full-screen experience:</p>
          <ol>
            <li>
              Tap <strong>Share</strong> in Safari.
            </li>
            <li>
              Tap <strong>Add to Home Screen</strong>.
            </li>
          </ol>
        </div>
      ) : (
        <div className="install-hint__section">
          <p>Install the Android app for a full-screen experience:</p>
          <Button
            variant="primary"
            type="button"
            onClick={() =>
              window.open(RELEASES_URL, "_blank", "noopener,noreferrer")
            }
          >
            Download the Android app
          </Button>
        </div>
      )}

      <p className="install-hint__hint">
        Notifications are delivered through the ntfy app.
      </p>
    </Popup>
  );
};

export default InstallHint;
