[← Back to Setup Development Environment](./set-up-dev-env.md)

> **Note:** this guide covers the current PWA build. PWA is planned to be replaced
> by native app shells (Android APK and an iOS home-screen web clip); this document
> will be updated or removed as part of that change.

# Testing PWA on mobile devices
This guide will help you test the Progressive Web App (PWA) features of Choruko on mobile devices using Android Studio's emulator.

## Prerequisites
- [Android Studio](https://developer.android.com/studio/install)

## Create an Android Virtual Device

1. Open Android Studio and click on "More Actions" > "Virtual Device Manager".

    <img src="../images/test-pwa-mobile/1.png" style="max-width: 600px;">

2. Click on "Create virtual device...".

    <img src="../images/test-pwa-mobile/2.png" style="max-width: 600px;">

3. Select a device definition (e.g., Pixel 9) and click "Next".

    <img src="../images/test-pwa-mobile/3.png" style="max-width: 600px;">

4. Choose a device name and system image (latest stable version recommended), then click "Finish".

    <img src="../images/test-pwa-mobile/4.png" style="max-width: 600px;">

5. Start the virtual device by clicking the "Play" button next to it.

    <img src="../images/test-pwa-mobile/5.png" style="max-width: 600px;">

## Access the PWA on the emulator

1. Go to ADB path:
   - macOS: `/Users/<your-username>/Library/Android/sdk/platform-tools/adb`
   - Linux: `/home/<your-username>/Android/Sdk/platform-tools/adb`
   - Windows: `C:\Users\<your-username>\AppData\Local\Android\Sdk\platform-tools\adb`

2. Forward the port to access the PWA running on your local machine:

   ```bash
    ./adb reverse tcp:3000 tcp:3000
    ```

3. Open the Chrome browser on the emulator and navigate to `http://localhost:3000`.

    Click on the menu (three dots) and select `Add to Home screen`.

    <img src="../images/test-pwa-mobile/6.png" style="max-width: 300px;">

    <img src="../images/test-pwa-mobile/7.png" style="max-width: 300px;">

 4. Click `Install` to install and add the PWA to the home screen.

    <img src="../images/test-pwa-mobile/8.png" style="max-width: 300px;">

 5. Installed PWA should open in a standalone window without browser UI elements.

    <img src="../images/test-pwa-mobile/9.png" style="max-width: 300px;">