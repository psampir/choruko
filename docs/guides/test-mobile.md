[← Back to Setup Development Environment](./set-up-dev-env.md)

# Testing on mobile

Choruko has no service worker: it is a plain React SPA. On iOS you can still add it
to the home screen as a web clip, and on Android the native app is published on
[GitHub Releases](https://github.com/psampir/choruko/releases/latest). This guide
covers checking the layout on a phone or emulator against your local dev server.

## From a phone on the same network

1. Start the backend (`dotnet run`, port `5180`) and the frontend
   (`npm run dev`, port `3000`).
2. Vite listens on all interfaces, so open `http://<your-computer-ip>:3000` on the
   phone. The dev server proxies `/api` and `/uploads` to the backend for you.
3. No TLS is needed. The site is not a secure context, so the clipboard API is
   unavailable; copying the ntfy topic falls back to a hidden text area.

## Android emulator

### Prerequisites
- [Android Studio](https://developer.android.com/studio/install)

### Create an Android Virtual Device

1. Open Android Studio and click on "More Actions" > "Virtual Device Manager".

    <img src="../images/test-mobile/1.png" style="max-width: 600px;">

2. Click on "Create virtual device...".

    <img src="../images/test-mobile/2.png" style="max-width: 600px;">

3. Select a device definition (e.g., Pixel 9) and click "Next".

    <img src="../images/test-mobile/3.png" style="max-width: 600px;">

4. Choose a device name and system image (latest stable version recommended), then click "Finish".

    <img src="../images/test-mobile/4.png" style="max-width: 600px;">

5. Start the virtual device by clicking the "Play" button next to it.

    <img src="../images/test-mobile/5.png" style="max-width: 600px;">

### Reach the dev server

1. Go to the ADB directory:
   - macOS: `/Users/<your-username>/Library/Android/sdk/platform-tools/adb`
   - Linux: `/home/<your-username>/Android/Sdk/platform-tools/adb`
   - Windows: `C:\Users\<your-username>\AppData\Local\Android\Sdk\platform-tools\adb`

2. Forward the frontend and API ports to the emulator:

   ```bash
   ./adb reverse tcp:3000 tcp:3000
   ./adb reverse tcp:5180 tcp:5180
   ```

3. Open Chrome on the emulator and navigate to `http://localhost:3000`.

## iOS web clip

Safari's **Share → Add to Home Screen** turns the site into a full-screen web clip;
`apple-mobile-web-app-capable` and the safe-area padding are already in place. It
works over plain HTTP, but a trusted TLS certificate is still recommended so the
session cookie and password are not sent in clear text.

## Notifications

The app does not show notifications by itself. Log in, open **Manage Account**, copy
your topic from the **Notifications** section and subscribe to it in the ntfy app on
the same server the backend uses (`NTFY_BASE_URL`). The topic is per user and
unguessable; treat it like a password.
