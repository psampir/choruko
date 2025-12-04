import React, { useState, useEffect } from 'react';
import Popup from './Popup/Popup';
import './PWAInstallPopup.scss';

interface PWAInstallPopupProps {
  onClose: () => void;
  open: boolean;
}

const PWAInstallPopup: React.FC<PWAInstallPopupProps> = ({ onClose, open }) => {
  const [isIOS, setIsIOS] = useState(false);
  const [isAndroid, setIsAndroid] = useState(false);

  useEffect(() => {
    const userAgent = window.navigator.userAgent.toLowerCase();
    const ios = /iphone|ipad|ipod/.test(userAgent);
    const android = /android/.test(userAgent);

    setIsIOS(ios);
    setIsAndroid(android);
  }, []);

  return (
    <Popup
      title="📱 Install Choruko"
      open={open}
      onClose={onClose}
      variant="primary"
      className="pwa-install-popup"
    >
      <div className="pwa-install-content">
        <p className="pwa-install-subtitle" style={{ marginBottom: '1.5rem' }}>Get the full app experience!</p>

        {isIOS && (
          <div className="pwa-install-section">
            <h3>iPhone & iPad</h3>
            <ol>
              <li>Tap the <strong>Share</strong> button <span className="share-icon">⬆️</span></li>
              <li>Scroll and tap <strong>"Add to Home Screen"</strong></li>
              <li>Tap <strong>"Add"</strong></li>
            </ol>
          </div>
        )}

        {isAndroid && (
          <div className="pwa-install-section">
            <h3>Android</h3>
            <ol>
              <li>Tap the <strong>menu</strong> (⋮) in the browser</li>
              <li>Tap <strong>"Add to Home Screen"</strong></li>
              <li>Tap <strong>"Install"</strong></li>
            </ol>
          </div>
        )}

        <div className="pwa-install-benefits">
          <p>
            <span className="checkmark">✓</span>
            Quick access from home screen
          </p>
          <p>
            <span className="checkmark">✓</span>
            Full screen experience
          </p>
          <p>
            <span className="checkmark">✓</span>
            Faster app loading
          </p>
        </div>
      </div>
    </Popup>
  );
};

export default PWAInstallPopup;
