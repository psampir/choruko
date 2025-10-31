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

    // Helper to trigger welcome notification
    const triggerWelcomeNotification = async () => {
      console.log('[PushNotificationHandler] ✅ Subscription successful! Requesting welcome notification...');
      try {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/PushSubscription/welcome`, {
          method: 'POST',
          credentials: 'include',
        });
        
        if (response.ok) {
          const data = await response.json();
          console.log('[PushNotificationHandler] Welcome notification response:', data);
        } else {
          console.error('[PushNotificationHandler] Welcome notification request failed:', response.status);
        }
      } catch (error) {
        console.error('[PushNotificationHandler] Failed to request welcome notification:', error);
      }
    };

    // Auto-subscribe after login
    const attemptSubscription = async () => {
      hasTriedSubscription.current = true;

      // If permission already granted, just subscribe
      if (permission === 'granted') {
        console.log('[PushNotificationHandler] Permission already granted, subscribing...');
        const subscribed = await subscribe();
        console.log('[PushNotificationHandler] Subscribe result (already granted):', subscribed);
        
        if (subscribed) {
          await triggerWelcomeNotification();
        } else {
          console.error('[PushNotificationHandler] ❌ Subscription failed');
        }
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
        const subscribed = await subscribe();
        console.log('[PushNotificationHandler] Subscribe result:', subscribed);
        
        if (subscribed) {
          await triggerWelcomeNotification();
        } else {
          console.error('[PushNotificationHandler] ❌ Subscription failed');
        }
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
  const wasAuthenticatedRef = useRef(isAuthenticated);
  
  useEffect(() => {
    // Only unsubscribe if user WAS authenticated and is NOW not authenticated (logout)
    if (wasAuthenticatedRef.current && !isAuthenticated && isSubscribed) {
      console.log('[PushNotificationHandler] User logged out, unsubscribing...');
      unsubscribe();
      hasTriedSubscription.current = false; // Reset for next login
    }
    
    // Update the ref
    wasAuthenticatedRef.current = isAuthenticated;
  }, [isAuthenticated, isSubscribed, unsubscribe]);

  // This component doesn't render anything
  return null;
};

export default PushNotificationHandler;
