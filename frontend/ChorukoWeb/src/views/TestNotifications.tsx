import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useIsDarkTheme } from '../utils/iconUtils';
import pushNotificationService from '../services/pushNotificationService';

const TestNotifications: React.FC = () => {
  const navigate = useNavigate();
  const isDark = useIsDarkTheme();
  const [log, setLog] = useState<string[]>([]);
  const [vapidKey, setVapidKey] = useState<string>('');

  const addLog = (message: string) => {
    console.log(message);
    setLog(prev => [...prev, `${new Date().toLocaleTimeString()}: ${message}`]);
  };

  const checkSupport = () => {
    addLog('--- Checking browser support ---');
    addLog(`ServiceWorker: ${'serviceWorker' in navigator}`);
    addLog(`PushManager: ${'PushManager' in window}`);
    addLog(`Notification: ${'Notification' in window}`);
    addLog(`Notification permission: ${Notification?.permission || 'N/A'}`);
    
    // Check for iOS
    const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) || 
                  (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
    const navigatorWithStandalone = window.navigator as Navigator & { standalone?: boolean };
    const isStandalone = navigatorWithStandalone.standalone === true;
    
    if (isIOS) {
      addLog('--- iOS Detected ---');
      addLog(`Running as PWA (standalone): ${isStandalone}`);
      if (!isStandalone) {
        addLog('⚠️  iOS requires app to be installed via "Add to Home Screen"');
        addLog('⚠️  Push notifications ONLY work in standalone mode on iOS');
      }
    }
    
    addLog(`User Agent: ${navigator.userAgent.substring(0, 60)}...`);
  };

  const requestPermission = async () => {
    addLog('--- Requesting notification permission ---');

    if (!('Notification' in window)) {
      addLog('ERROR: Notifications not supported');
      return;
    }

    try {
      const permission = await Notification.requestPermission();
      addLog(`Permission result: ${permission}`);
    } catch (error) {
      addLog(`ERROR: ${error}`);
    }
  };

  const getVapidKey = async () => {
    addLog('--- Fetching VAPID public key ---');
    const key = await pushNotificationService.getPublicKey();
    if (key) {
      setVapidKey(key);
      addLog(`✅ Got VAPID key: ${key.substring(0, 20)}...`);
    } else {
      addLog('❌ Failed to get VAPID key');
    }
  };

  const subscribe = async () => {
    addLog('--- Subscribing to push ---');

    if (Notification.permission !== 'granted') {
      addLog('ERROR: Permission not granted. Request it first!');
      return;
    }

    try {
      const registration = await navigator.serviceWorker.ready;
      addLog('✅ Service worker ready');

      // Check for existing subscription first (iOS specific)
      const existingSub = await registration.pushManager.getSubscription();
      if (existingSub) {
        addLog('⚠️  Found existing subscription, unsubscribing first...');
        await existingSub.unsubscribe();
        addLog('✅ Old subscription removed');
      }

      const publicKey = vapidKey || await pushNotificationService.getPublicKey();
      if (!publicKey) {
        addLog('❌ No VAPID key available');
        return;
      }

      addLog(`VAPID key length: ${publicKey.length}`);

      // Helper function
      const urlBase64ToUint8Array = (base64String: string) => {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);
        for (let i = 0; i < rawData.length; ++i) {
          outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
      };

      const applicationServerKey = urlBase64ToUint8Array(publicKey);
      addLog(`Application server key created (${applicationServerKey.length} bytes)`);

      addLog('Calling pushManager.subscribe() - this may take a moment on iOS...');
      
      // Add timeout for iOS (30 seconds)
      const subscribePromise = registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey,
      });

      const timeoutPromise = new Promise<never>((_, reject) => 
        setTimeout(() => reject(new Error('Subscription timeout after 30s - iOS may require retry')), 30000)
      );

      const subscription = await Promise.race([subscribePromise, timeoutPromise]);

      addLog('✅ Browser subscription created');

      const subJson = subscription.toJSON();
      addLog(`Endpoint: ${subJson.endpoint?.substring(0, 40)}...`);

      // Debug: log what we're sending
      addLog(`Sending to backend: endpoint=${subJson.endpoint?.substring(0, 30)}...`);
      addLog(`Keys: p256dh=${subJson.keys!.p256dh?.substring(0, 20)}..., auth=${subJson.keys!.auth?.substring(0, 20)}...`);

      const success = await pushNotificationService.subscribe({
        endpoint: subJson.endpoint!,
        p256dhKey: subJson.keys!.p256dh!,
        authKey: subJson.keys!.auth!,
      });

      if (success) {
        addLog('✅ Subscription saved to backend!');
      } else {
        addLog('❌ Failed to save subscription to backend');
      }
    } catch (error) {
      addLog(`❌ ERROR: ${error}`);
      if (error instanceof Error) {
        addLog(`Error name: ${error.name}`);
        addLog(`Error message: ${error.message}`);
        if (error.stack) {
          addLog(`Stack: ${error.stack.substring(0, 200)}...`);
        }
      }
    }
  };

  const checkSubscription = async () => {
    addLog('--- Checking subscription status ---');
    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();

      if (subscription) {
        addLog('✅ Subscribed!');
        addLog(`Endpoint: ${subscription.endpoint.substring(0, 50)}...`);
      } else {
        addLog('❌ Not subscribed');
      }
    } catch (error) {
      addLog(`ERROR: ${error}`);
    }
  };

  const sendTestNotification = async () => {
    addLog('--- Sending test notification ---');
    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/PushSubscription/test`, {
        method: 'POST',
        credentials: 'include',
      });

      if (response.ok) {
        addLog('✅ Test notification sent! Check your browser!');
      } else {
        addLog(`❌ Failed: ${response.status}`);
      }
    } catch (error) {
      addLog(`ERROR: ${error}`);
    }
  };

  const unsubscribe = async () => {
    addLog('--- Unsubscribing ---');
    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();

      if (!subscription) {
        addLog('Already unsubscribed');
        return;
      }

      const endpoint = subscription.endpoint;
      await subscription.unsubscribe();
      addLog('✅ Unsubscribed from browser');

      await pushNotificationService.unsubscribe(endpoint);
      addLog('✅ Removed from backend');
    } catch (error) {
      addLog(`ERROR: ${error}`);
    }
  };

  const isIOS = /iPad|iPhone|iPod/.test(navigator.userAgent) || 
                (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
  const navigatorWithStandalone = window.navigator as Navigator & { standalone?: boolean };
  const isStandalone = navigatorWithStandalone.standalone === true;

  const backgroundColor = isDark ? '#1a1a1a' : '#ffffff';
  const textColor = isDark ? '#e0e0e0' : '#333333';
  const logBgColor = isDark ? '#0d0d0d' : '#f5f5f5';
  const logTextColor = isDark ? '#00ff00' : '#00aa00';

  return (
    <div style={{ padding: '20px', fontFamily: 'monospace', backgroundColor, color: textColor, minHeight: '100vh' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '20px' }}>
        <button 
          onClick={() => navigate(-1)} 
          style={{ 
            ...buttonStyle, 
            backgroundColor: '#6c757d',
            fontSize: '18px',
            padding: '8px 12px'
          }}
        >
          ← Back
        </button>
        <h1 style={{ margin: 0 }}>🔔 Push Test</h1>
      </div>

      {isIOS && !isStandalone && (
        <div style={{
          backgroundColor: isDark ? '#4a3f00' : '#fff3cd',
          color: isDark ? '#ffeb99' : '#856404',
          padding: '15px',
          borderRadius: '5px',
          marginBottom: '20px',
          border: `1px solid ${isDark ? '#8a7500' : '#ffc107'}`
        }}>
          <strong>⚠️ iOS Users:</strong> Push notifications only work when the app is installed to your home screen.
          <br />
          <strong>Steps:</strong>
          <ol style={{ margin: '10px 0', paddingLeft: '20px' }}>
            <li>Tap the Share button <span style={{ fontSize: '20px' }}>⎙</span> in Safari</li>
            <li>Select "Add to Home Screen"</li>
            <li>Open the app from your home screen</li>
            <li>Return to this page and test notifications</li>
          </ol>
        </div>
      )}

      <div style={{ marginBottom: '20px', display: 'flex', flexWrap: 'wrap', gap: '10px' }}>
        <button onClick={checkSupport} style={buttonStyle}>1. Check Support</button>
        <button onClick={requestPermission} style={buttonStyle}>2. Request Permission</button>
        <button onClick={getVapidKey} style={buttonStyle}>3. Get VAPID Key</button>
        <button onClick={subscribe} style={buttonStyle}>4. Subscribe</button>
        <button onClick={sendTestNotification} style={{ ...buttonStyle, backgroundColor: '#28a745' }}>🔔 Send Test Push</button>
        <button onClick={checkSubscription} style={buttonStyle}>Check Status</button>
        <button onClick={unsubscribe} style={buttonStyle}>Unsubscribe</button>
        <button onClick={() => setLog([])} style={{ ...buttonStyle, backgroundColor: '#666' }}>Clear Log</button>
      </div>

      <div style={{
        backgroundColor: logBgColor,
        padding: '15px',
        borderRadius: '5px',
        height: '400px',
        overflowY: 'auto',
        fontSize: '12px',
        lineHeight: '1.6',
        border: `1px solid ${isDark ? '#333' : '#ddd'}`
      }}>
        {log.length === 0 ? (
          <div style={{ color: isDark ? '#00ff00' : '#999' }}>Click buttons above to test push notifications...</div>
        ) : (
          log.map((entry, i) => <div key={i} style={{ color: logTextColor }}>{entry}</div>)
        )}
      </div>

      {vapidKey && (
        <div style={{ marginTop: '20px', fontSize: '10px', color: isDark ? '#888' : '#666' }}>
          VAPID Key: {vapidKey}
        </div>
      )}
    </div>
  );
};

const buttonStyle: React.CSSProperties = {
  padding: '10px 15px',
  backgroundColor: '#007bff',
  color: 'white',
  border: 'none',
  borderRadius: '4px',
  cursor: 'pointer',
  fontSize: '14px',
};

export default TestNotifications;
