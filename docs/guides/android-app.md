[← Back to README](../../README.md)

# Android app

The Android app is a thin shell around the Choruko web interface. It ships as an
APK on [GitHub Releases](https://github.com/psampir/choruko/releases) and is not on
Google Play.

Nothing about your server is baked into the APK: on first launch the app asks for the
address of your instance and remembers it. The same APK works with any self-hosted
Choruko, over HTTPS or plain HTTP on a LAN.

## Install

1. Download `choruko-*.apk` from the latest release on a phone or tablet.
2. Allow installing apps from your browser or file manager when Android asks.
3. Open **Choruko** and enter your instance address, for example
   `https://choruko.example.com` or `http://192.168.1.10:2467`.
4. Tap **Connect**. The app checks that the address really serves Choruko before it
   saves it.

To point the app at a different instance later, use any of:

- long-press the Choruko icon and choose **Change server**;
- **Manage Account → Server → Change Server** inside the app;
- **Change server** on the screen shown when the instance cannot be reached.

The current address stays saved until a new one passes the check, so backing out of
the address screen keeps it.

## Opening the app from a browser

On Android, the site tries to hand the page over to the app once per browser
session. Chromium browsers (Chrome, Edge, Brave, Samsung Internet, Opera) do this
silently: if the app is installed it opens, and if it is not the browser stays on
the same page. Firefox has no equivalent, so there the **Already installed? Open in
the app** link in the install hint does the same on tap.

The link carries the current server and page, so the app opens the same instance at
the same place. A server that differs from the saved one opens the address screen
with the link's server pre-filled, so a second instance is never opened silently.
`?stay=1` in the address, or simply staying on the page, keeps the browser for the
rest of the session.

The web page cannot see which apps are installed — this attempt is the only
mechanism browsers allow. Custom-scheme links (`chorukoapp://`) are not App Links:
those would require a fixed HTTPS domain baked into the APK, which does not fit
self-hosted instances.

## What the app does

- Shows the same interface as the browser, in a full-screen WebView.
- Keeps the login cookie across restarts.
- Hands file inputs (the profile picture) to the system picker, and opens links to
  other sites in your browser.
- Adds `ChorukoApp/<version>` to the user agent, so the web interface knows not to
  offer the "download the Android app" hint.
- Back walks the page history and then sends the app to the background, like other
  Android apps.

The app does **not** receive push notifications. Choruko sends notifications through
ntfy, so install the ntfy app and subscribe to the topic shown in
**Manage Account → Notifications**. **Send Test** there sends a notification to that
topic and reports why it failed if it did.

## Requirements

Android 8.0 (API 26) or newer.

## Building

The project lives in [`android/`](../../android) and uses the Gradle wrapper.

```bash
cd android
./gradlew assembleRelease
```

The APK lands in `android/app/build/outputs/apk/release/`. Without signing variables
(see below) the build produces an unsigned APK, which is enough to check that the
project compiles.

`local.properties` with `sdk.dir=/path/to/Android/Sdk` is only needed when the
`ANDROID_HOME` environment variable is not set.

### Signing

Release signing is read from the environment so the keystore never enters the
repository:

| Variable | Meaning |
|---|---|
| `CHORUKO_KEYSTORE_PATH` | path to the `.jks` file |
| `CHORUKO_KEYSTORE_PASSWORD` | keystore password |
| `CHORUKO_KEY_ALIAS` | key alias |
| `CHORUKO_KEY_PASSWORD` | key password (defaults to the keystore password) |
| `CHORUKO_VERSION_NAME` | version name, for example `1.1.0` |
| `CHORUKO_VERSION_CODE` | integer version code |

For local builds the same values can live in `android/keystore.properties`, which is
git-ignored — see `android/keystore.properties.example`. That keeps the passwords out
of your shell history:

```properties
storeFile=/home/you/choruko-release.jks
storePassword=...
keyAlias=choruko
keyPassword=...
```

Generating a keystore (do this once and keep it safe):

```bash
keytool -genkeypair -v \
  -keystore choruko-release.jks \
  -alias choruko \
  -keyalg RSA -keysize 4096 -validity 10000
```

Losing the keystore means users must uninstall the app before installing a newer
build, because Android rejects an update signed with a different key.

## Releasing

1. Publish the GitHub release for the tag `v<version>`.
2. Run the **Android** workflow (Actions → Android → Run workflow) with that version.
3. The workflow signs the APK, uploads it as an artifact and attaches it to the
   release.

The workflow needs four repository secrets: `ANDROID_KEYSTORE_BASE64` (the keystore
encoded with `base64 -w0 choruko-release.jks`), `ANDROID_KEYSTORE_PASSWORD`,
`ANDROID_KEY_ALIAS` and `ANDROID_KEY_PASSWORD`.

Pull requests that touch `android/` build the project unsigned, so a broken Gradle
setup fails before a release.
