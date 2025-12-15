import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { ReactNode } from 'react';
import pushNotificationService from '../services/pushNotificationService';

// Helper function to convert VAPID key
function urlBase64ToUint8Array(base64String: string): BufferSource {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding)
    .replace(/-/g, '+')
    .replace(/_/g, '/');

  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);

  for (let i = 0; i < rawData.length; ++i) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

export type NotificationPermission = 'default' | 'granted' | 'denied';

interface PushNotificationContextType {
  permission: NotificationPermission;
  isSupported: boolean;
  isSubscribed: boolean;
  isLoading: boolean;
  error: string | null;
  requestPermission: () => Promise<boolean>;
  subscribe: () => Promise<boolean>;
  unsubscribe: () => Promise<boolean>;
  clearError: () => void;
}

const PushNotificationContext = createContext<PushNotificationContextType | undefined>(undefined);

export const usePushNotifications = () => {
  const context = useContext(PushNotificationContext);
  if (!context) {
    throw new Error('usePushNotifications must be used within PushNotificationProvider');
  }
  return context;
};

interface PushNotificationProviderProps {
  children: ReactNode;
}

export const PushNotificationProvider: React.FC<PushNotificationProviderProps> = ({ children }) => {
  const [permission, setPermission] = useState<NotificationPermission>('default');
  const [isSubscribed, setIsSubscribed] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Check if push notifications are supported
  const isSupported =
    'serviceWorker' in navigator &&
    'PushManager' in window &&
    'Notification' in window;

  // Check current subscription status
  const checkSubscription = useCallback(async () => {
    if (!isSupported) return;

    try {
      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();
      setIsSubscribed(!!subscription);
    } catch (err) {
      console.error('Error checking subscription:', err);
    }
  }, [isSupported]);

  // Initialize: check permission and subscription status
  useEffect(() => {
    if (isSupported) {
      setPermission(Notification.permission as NotificationPermission);
      checkSubscription();
    }
  }, [isSupported, checkSubscription]);

  // Request notification permission
  const requestPermission = useCallback(async (): Promise<boolean> => {
    if (!isSupported) {
      setError('Push notifications are not supported in this browser');
      return false;
    }

    try {
      setIsLoading(true);
      setError(null);

      const result = await Notification.requestPermission();
      setPermission(result as NotificationPermission);

      if (result === 'granted') {
        return true;
      } else if (result === 'denied') {
        setError('Notification permission denied');
      }
      return false;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(`Error requesting permission: ${errorMessage}`);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [isSupported]);

  // Subscribe to push notifications
  const subscribe = useCallback(async (): Promise<boolean> => {
    if (!isSupported) {
      setError('Push notifications are not supported');
      return false;
    }

    if (permission !== 'granted') {
      setError('Notification permission not granted');
      return false;
    }

    try {
      setIsLoading(true);
      setError(null);

      // Get VAPID public key from backend
      const publicKey = await pushNotificationService.getPublicKey();
      if (!publicKey) {
        setError('Failed to get VAPID public key');
        return false;
      }

      // Subscribe to push manager
      const registration = await navigator.serviceWorker.ready;
      
      // Check for existing subscription first (iOS specific workaround)
      const existingSubscription = await registration.pushManager.getSubscription();
      if (existingSubscription) {
        console.log('Found existing subscription, unsubscribing first...');
        await existingSubscription.unsubscribe();
      }

      // Add timeout for iOS (30 seconds)
      const subscribePromise = registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: urlBase64ToUint8Array(publicKey),
      });

      const timeoutPromise = new Promise<never>((_, reject) => 
        setTimeout(() => reject(new Error('Subscription timeout - please try again')), 30000)
      );

      const subscription = await Promise.race([subscribePromise, timeoutPromise]);

      // Extract subscription data
      const subscriptionJson = subscription.toJSON();
      if (!subscriptionJson.endpoint || !subscriptionJson.keys) {
        setError('Invalid subscription data');
        return false;
      }

      // Send subscription to backend
      const success = await pushNotificationService.subscribe({
        endpoint: subscriptionJson.endpoint,
        p256dhKey: subscriptionJson.keys.p256dh || '',
        authKey: subscriptionJson.keys.auth || '',
      });

      if (success) {
        setIsSubscribed(true);
        return true;
      } else {
        setError('Failed to save subscription to server');
        return false;
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(`Error subscribing: ${errorMessage}`);
      console.error('Subscription error:', err);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [isSupported, permission]);

  // Unsubscribe from push notifications
  const unsubscribe = useCallback(async (): Promise<boolean> => {
    if (!isSupported) {
      setError('Push notifications are not supported');
      return false;
    }

    try {
      setIsLoading(true);
      setError(null);

      const registration = await navigator.serviceWorker.ready;
      const subscription = await registration.pushManager.getSubscription();

      if (!subscription) {
        setIsSubscribed(false);
        return true;
      }

      // Unsubscribe from push manager
      const unsubscribed = await subscription.unsubscribe();

      if (unsubscribed) {
        // Remove subscription from backend
        await pushNotificationService.unsubscribe(subscription.endpoint);
        setIsSubscribed(false);
        return true;
      }

      return false;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(`Error unsubscribing: ${errorMessage}`);
      return false;
    } finally {
      setIsLoading(false);
    }
  }, [isSupported]);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const value: PushNotificationContextType = {
    permission,
    isSupported,
    isSubscribed,
    isLoading,
    error,
    requestPermission,
    subscribe,
    unsubscribe,
    clearError,
  };

  return (
    <PushNotificationContext.Provider value={value}>
      {children}
    </PushNotificationContext.Provider>
  );
};
