# PWA Push Notifications Implementation Progress

**Last Updated:** 2025-10-29 (23:17 UTC)

## Overview
Implementing push notifications for the Choruko PWA to notify users about app events even when the app is closed.

## Current State Assessment

### ✅ What We Already Have
- Service worker configured (`frontend/ChorukoWeb/src/sw.ts`)
- PWA manifest with all required icons
- Workbox for precaching
- VitePWA plugin setup in `vite.config.ts`
- ASP.NET Core backend with Identity authentication
- Cookie-based auth with CORS configured

### ❌ What We Need to Implement

## Implementation Plan - Broken Down Into Tasks

### Phase 1: Backend Infrastructure ✅ COMPLETED
**Task 1.1: Database Setup** ✅
- [x] Create `PushSubscription` entity/model
  - Properties: Id, UserId, Endpoint, P256dhKey, AuthKey, UserAgent, CreatedAt, LastUsedAt
  - One-to-many relationship with User
- [x] Create EF Core migration
- [x] Apply migration to database
  - ⚠️ **NOTE FOR DEVS:** After merging to dev, run `python3 backend/update-db.py` to apply migration

**Task 1.2: VAPID Keys Generation** ✅
- [x] Install `WebPush` NuGet package
- [x] Generate VAPID keys (public/private keypair)
- [x] Store in `appsettings.json` or environment variables
- [x] Add configuration model for VAPID settings

**Task 1.3: Backend API Endpoints** ✅
- [x] Create `PushSubscriptionController`
- [x] POST `/api/push/subscribe` - Save new push subscription
- [x] DELETE `/api/push/unsubscribe` - Remove subscription
- [x] GET `/api/push/public-key` - Return VAPID public key to frontend

**Task 1.4: Push Service Implementation** ✅
- [x] Create `IPushNotificationService` interface
- [x] Create `PushNotificationService` implementation
- [x] Method to send push to single subscription
- [x] Method to send push to all user's subscriptions
- [x] Handle failed subscriptions (410 status) - remove from DB
- [x] Register service in DI container

### Phase 2: Frontend - Service Worker ✅ COMPLETED
**Task 2.1: Service Worker Push Handling** ✅
- [x] Add `push` event listener to `sw.ts`
- [x] Parse incoming push data
- [x] Display notification using Notifications API
- [x] Handle notification click event (open app to specific route)
- [x] Handle notification close event (optional analytics)

**Task 2.2: Service Worker Helpers** ✅
- [x] Create utility functions for notification display (inline in push handler)
- [x] Add notification icons/badges configuration (defaults with override pattern)
- [x] Add default notification options (fallback values)

### Phase 3: Frontend - Application Logic
**Task 3.1: Permission & Subscription Management**
- [ ] Create `usePushNotifications` hook or service
- [ ] Request notification permission from user
- [ ] Subscribe to push notifications (browser API)
- [ ] Send subscription to backend API
- [ ] Handle subscription updates/refresh
- [ ] Handle errors and edge cases

**Task 3.2: UI Components**
- [ ] Create notification permission prompt UI
- [ ] Add toggle in user settings to enable/disable notifications
- [ ] Show current notification permission status
- [ ] Handle permission denied state gracefully

**Task 3.3: Integration with API Service**
- [ ] Add API methods in existing service layer
- [ ] POST subscription to backend
- [ ] DELETE subscription on logout/disable
- [ ] GET VAPID public key

### Phase 4: Business Logic Integration
**Task 4.1: Identify Notification Triggers**
- [ ] Define which events should trigger notifications
  - Examples: New chore assigned, chore completed, settlement created, etc.
- [ ] Document notification types and content

**Task 4.2: Implement Notification Triggers**
- [ ] Add push notification calls in relevant backend services
- [ ] When task assigned → notify assigned user
- [ ] When task completed → notify household admin
- [ ] When settlement created → notify all participants
- [ ] Add any other business-specific triggers

**Task 4.3: Notification Content**
- [ ] Create notification message templates
- [ ] Include relevant data (task name, user, household, etc.)
- [ ] Add click actions (deep links to specific pages)

### Phase 5: Testing & Polish
**Task 5.1: Testing**
- [ ] Test on multiple devices
- [ ] Test on multiple browsers (Chrome, Firefox, Safari, Edge)
- [ ] Test with app closed, minimized, and open
- [ ] Test with multiple devices per user
- [ ] Test subscription expiry handling
- [ ] Test offline scenarios

**Task 5.2: Error Handling & Edge Cases**
- [ ] Handle browser doesn't support push
- [ ] Handle user blocks notifications
- [ ] Handle network failures
- [ ] Handle subscription endpoint changes
- [ ] Add logging for debugging

**Task 5.3: Documentation**
- [ ] Update README with push notification info
- [ ] Document VAPID key setup for deployment
- [ ] Document environment variables needed
- [ ] Add troubleshooting guide

## Technical Architecture Notes

### Push Flow Diagram
```
1. Setup:
   User grants permission → Browser subscribes → Frontend sends to Backend → Backend stores in DB

2. Trigger:
   Event occurs → Backend retrieves subscriptions → Backend sends to Push Service (Google/Mozilla/Apple)

3. Delivery:
   Push Service → Device → Wakes Service Worker → Displays notification
```

### Key Technologies
- **Frontend:** React, TypeScript, Workbox, Service Workers API, Push API, Notifications API
- **Backend:** ASP.NET Core, Entity Framework, WebPush library
- **Push Services:** FCM (Chrome/Edge), Mozilla Push Service (Firefox), APNs (Safari)

### Important Notes
- Push notifications require HTTPS (localhost is exempt for dev)
- VAPID keys are generated once and stored securely
- Each device/browser combination = separate subscription
- Subscriptions can expire and need cleanup
- Browser vendors provide push service for FREE
- Service workers are event-driven, not always running

## Next Steps
Start with **Phase 1: Backend Infrastructure** - Set up the database and VAPID keys first.

---

## Progress Log

### Session 1 - 2025-10-25
- Analyzed existing codebase
- Documented current state
- Created implementation plan
- Ready to begin implementation

### Session 2 - 2025-10-27 to 2025-10-29
**Backend Implementation - Phase 1** ✅ COMPLETED

#### Database Setup (2025-10-27)
- Created `PushSubscription` model with all required properties (Id, UserId, Endpoint, P256dhKey, AuthKey, UserAgent, CreatedAt, LastUsedAt)
- Created EF Core migration `AddPushSubscriptionTable` (commit: 7e6efde, fa07579)
- Added `PushSubscription` DbSet to DataContext

#### VAPID Configuration (2025-10-27 to 2025-10-29)
- Installed WebPush NuGet package (commit: 6f28637)
- Created `VapidSettings` configuration model (commit: 400f518)
- Added VAPID configuration to `appsettings.json`
- Implemented automatic VAPID key generation (commit: d06050f)
- Created `VapidKeysChecker` and `VapidKeysGenerator` extensions
- Added VAPID key validation on application startup in `Program.cs`

#### API Endpoints (2025-10-29)
- Created `PushSubscriptionController` with three endpoints (commit: 77b8425):
  - POST `/api/push/subscribe` - Save new push subscription
  - DELETE `/api/push/unsubscribe` - Remove subscription
  - GET `/api/push/public-key` - Return VAPID public key to frontend
- Created request DTOs:
  - `PushSubscriptionRequest` (commit: e0e86cb)
  - `PushSubscriptionUnsubscribeRequest` (commit: 52d1a19)

#### Push Notification Service (2025-10-28)
- Created `IPushNotificationService` interface (commit: 7cb6ef7)
- Implemented `PushNotificationService` with full functionality (commit: 7cb6ef7):
  - Send push to single subscription
  - Send push to all user subscriptions
  - Automatic cleanup of failed/expired subscriptions (410 status)
  - WebPush integration with VAPID authentication

#### Branch Status
- Work done on `add-notification-handling` branch
- Merged into `148-add-notification-handler` branch (commit: e7d1115)

### Session 3 - 2025-10-29 (Evening)
**VAPID Key Management Improvements & Service Registration** ✅

#### VAPID Key Generation Refactored
- **Completed:** Proper VAPID key generation on application startup
- **Implementation details:**
  - `VapidKeysChecker`: Checks if `vapid-keys.json` exists and contains valid keys
  - `VapidKeysGenerator`: Generates new VAPID key pair using WebPush library and saves to JSON file
  - `VapidKeysLoader`: Loads keys from JSON file into application configuration at startup
  - Keys are stored in `vapid-keys.json` (excluded from git via .gitignore)
  - Automatic generation on first run if file doesn't exist
  - Configuration properly loads from file instead of appsettings.json

#### Service Registration
- **Completed:** Registered `IPushNotificationService` in DI container
- **Completed:** Registered `VapidSettings` configuration binding
- Service is now available for injection throughout the application

#### Security & Best Practices
- Added `vapid-keys.json` to `.gitignore` to prevent committing sensitive keys
- Keys are generated once and persisted across application restarts
- Empty placeholders remain in `appsettings.json` for documentation purposes
- Console logging shows key generation status on startup

#### Files Modified
- `Extensions/VapidKeysChecker.cs` - Implemented full checking logic
- `Extensions/VapidKeysGenerator.cs` - Implemented key generation and file writing
- `Extensions/VapidKeysLoader.cs` - **NEW:** Loads keys from file into configuration
- `Program.cs` - Fixed logic (was inverted), added loader, registered services
- `backend/.gitignore` - Added vapid-keys.json to ignore list

#### Migration Applied
- **DONE:** Applied `AddPushSubscriptionTable` migration locally
- `PushSubscriptions` table created with unique index on `Endpoint`
- Used `backend/update-db.py` script for applying migration

#### Important Notes for Team
- ⚠️ **MIGRATION REQUIRED:** After merging this branch to `dev`, all developers must run:
  ```bash
  python3 backend/update-db.py
  ```
  This applies the new `PushSubscriptions` table migration.

### Session 4 - 2025-10-29 (Late Evening)
**Phase 2: Service Worker Implementation** ✅ COMPLETED

#### Service Worker Push Handling
- **Completed:** Added comprehensive push notification handling to `sw.ts`
- **Implementation details:**
  - `push` event listener: Receives push from browser, parses JSON payload, displays notification
  - `notificationclick` event: Handles user clicks, focuses existing window or opens new one, navigates to URL
  - `notificationclose` event: Optional analytics hook for dismissed notifications
  - Default values with override pattern: Ensures notifications always work even if payload is missing
  - Event-driven architecture: Service worker wakes on events, processes, then sleeps

#### Technical Details
- **Payload format:** `{title, body, icon, badge, url, data}`
- **Navigation logic:** Reuses existing app window if open, otherwise opens new window
- **Notification options:** Tag for grouping, requireInteraction: false for auto-dismiss
- **Error handling:** Try-catch on JSON parsing with fallback to defaults
- **Pattern:** Observer pattern + Promise chain pattern with `event.waitUntil()`

#### Files Modified
- `frontend/ChorukoWeb/src/sw.ts` - Added 87 lines of push notification handling code

#### Build Status
- ✅ TypeScript compilation successful
- ✅ Vite build successful
- ✅ Service worker bundle generated: `dist/sw.js` (17.08 kB gzipped: 5.81 kB)

#### Next Steps
- **TODO:** Begin Phase 3: Frontend - Application Logic (hooks, UI components, API integration)
- **TODO:** Test push notifications end-to-end with backend
