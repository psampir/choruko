const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

interface PushSubscriptionData {
  endpoint: string;
  p256dhKey: string;
  authKey: string;
}

class PushNotificationService {
  async getPublicKey(): Promise<string | null> {
    try {
      const response = await fetch(`${apiBaseUrl}/PushSubscription/public-key`, {
        method: "GET",
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        // Backend now returns ApiResponse with result field
        return data.result || data.publicKey || null;
      }
      return null;
    } catch (error) {
      console.error("Error fetching VAPID public key:", error);
      return null;
    }
  }

  async subscribe(subscriptionData: PushSubscriptionData): Promise<boolean> {
    try {
      const response = await fetch(`${apiBaseUrl}/PushSubscription/subscribe`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(subscriptionData),
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error(`Subscribe failed: ${response.status} - ${errorText}`);
        return false;
      }

      const data = await response.json();
      console.log("Subscribe response:", data);
      return data.isSuccessful !== false;
    } catch (error) {
      console.error("Error subscribing to push notifications:", error);
      return false;
    }
  }

  async unsubscribe(endpoint: string): Promise<boolean> {
    try {
      const response = await fetch(`${apiBaseUrl}/PushSubscription/unsubscribe`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({ endpoint }),
      });

      return response.ok;
    } catch (error) {
      console.error("Error unsubscribing from push notifications:", error);
      return false;
    }
  }
}

export default new PushNotificationService();
