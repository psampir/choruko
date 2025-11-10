import { useEffect, useRef } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { usePushNotifications } from '../contexts/PushNotificationContext';

/**
 * Component that handles automatic push notification subscription after login
 * and unsubscription on logout.
 */
const PushNotificationHandler: React.FC = () => {
  const { isAuthenticated, onBeforeLogout } = useAuth();
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
        const subscribed = await subscribe();
        console.log('[PushNotificationHandler] Subscribe result (already granted):', subscribed);
        
        if (!subscribed) {
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
        
        if (!subscribed) {
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

  // Handle logout - unsubscribe BEFORE logout completes
  useEffect(() => {
    if (!isAuthenticated) return;
    
    const cleanup = onBeforeLogout(async () => {
      if (isSubscribed) {
        console.log('[PushNotificationHandler] Pre-logout: unsubscribing...');
        await unsubscribe();
        hasTriedSubscription.current = false; // Reset for next login
        console.log('[PushNotificationHandler] Pre-logout: unsubscribe complete');
      }
    });
    
    return cleanup;
  }, [isAuthenticated, isSubscribed, unsubscribe, onBeforeLogout]);

  // This component doesn't render anything
  return null;
};

export default PushNotificationHandler;
