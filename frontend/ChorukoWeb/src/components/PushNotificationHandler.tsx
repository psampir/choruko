import { useEffect, useRef } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { usePushNotifications } from '../contexts/PushNotificationContext';

/**
 * Component that handles automatic push notification subscription after login
 * and unsubscription on logout.
 */
const PushNotificationHandler: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const { 
    isSupported, 
    permission, 
    isSubscribed, 
    requestPermission, 
    subscribe, 
    unsubscribe 
  } = usePushNotifications();
  
  const hasTriedSubscription = useRef(false);

  useEffect(() => {
    // Only run when user is authenticated
    if (!isAuthenticated || !isSupported) {
      return;
    }

    // If already subscribed, nothing to do
    if (isSubscribed) {
      return;
    }

    // Prevent multiple attempts in same session
    if (hasTriedSubscription.current) {
      return;
    }

    // Auto-subscribe after login
    const attemptSubscription = async () => {
      hasTriedSubscription.current = true;

      // If permission already granted, just subscribe
      if (permission === 'granted') {
        console.log('[PushNotificationHandler] Permission already granted, subscribing...');
        await subscribe();
        return;
      }

      // If permission denied, don't ask again
      if (permission === 'denied') {
        console.log('[PushNotificationHandler] Permission denied, skipping subscription');
        return;
      }

      // Permission is 'default' - ask user
      console.log('[PushNotificationHandler] Requesting notification permission...');
      const granted = await requestPermission();
      
      if (granted) {
        console.log('[PushNotificationHandler] Permission granted, subscribing...');
        await subscribe();
      } else {
        console.log('[PushNotificationHandler] Permission not granted');
      }
    };

    // Delay the request slightly to avoid showing permission prompt immediately on login
    const timer = setTimeout(() => {
      attemptSubscription();
    }, 2000); // Wait 2 seconds after login

    return () => clearTimeout(timer);
  }, [isAuthenticated, isSupported, permission, isSubscribed, requestPermission, subscribe]);

  // Handle logout - unsubscribe
  useEffect(() => {
    if (!isAuthenticated && isSubscribed) {
      console.log('[PushNotificationHandler] User logged out, unsubscribing...');
      unsubscribe();
      hasTriedSubscription.current = false; // Reset for next login
    }
  }, [isAuthenticated, isSubscribed, unsubscribe]);

  // This component doesn't render anything
  return null;
};

export default PushNotificationHandler;
