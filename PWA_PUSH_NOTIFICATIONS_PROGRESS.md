# PWA Push Notifications Implementation Progress

**Last Updated:** 2025-10-25

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

### Phase 1: Backend Infrastructure
**Task 1.1: Database Setup**
- [ ] Create `PushSubscription` entity/model
  - Properties: Id, UserId, Endpoint, P256dhKey, AuthKey, UserAgent, CreatedAt, LastUsedAt
  - One-to-many relationship with User
- [ ] Create EF Core migration
- [ ] Apply migration to database

**Task 1.2: VAPID Keys Generation**
- [ ] Install `WebPush` NuGet package
- [ ] Generate VAPID keys (public/private keypair)
- [ ] Store in `appsettings.json` or environment variables
- [ ] Add configuration model for VAPID settings

**Task 1.3: Backend API Endpoints**
- [ ] Create `PushSubscriptionController`
- [ ] POST `/api/push/subscribe` - Save new push subscription
- [ ] DELETE `/api/push/unsubscribe` - Remove subscription
- [ ] GET `/api/push/public-key` - Return VAPID public key to frontend

**Task 1.4: Push Service Implementation**
- [ ] Create `IPushNotificationService` interface
- [ ] Create `PushNotificationService` implementation
- [ ] Method to send push to single subscription
- [ ] Method to send push to all user's subscriptions
- [ ] Handle failed subscriptions (410 status) - remove from DB
- [ ] Register service in DI container

### Phase 2: Frontend - Service Worker
**Task 2.1: Service Worker Push Handling**
- [ ] Add `push` event listener to `sw.ts`
- [ ] Parse incoming push data
- [ ] Display notification using Notifications API
- [ ] Handle notification click event (open app to specific route)
- [ ] Handle notification close event (optional analytics)

**Task 2.2: Service Worker Helpers**
- [ ] Create utility functions for notification display
- [ ] Add notification icons/badges configuration
- [ ] Add default notification options

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
