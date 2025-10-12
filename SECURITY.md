# Security

## Overview

AuthForge is a multi-tenant authentication-as-a-service platform built with security as a top priority. This document
outlines security best practices for both **API consumers** (developers integrating AuthForge) and **contributors**.

---

## 🔐 Token Storage Best Practices

### How AuthForge Returns Tokens

AuthForge returns **both access and refresh tokens in the response body** (not in cookies). This design choice follows
industry standards set by Auth0, Firebase, AWS Cognito, and Supabase, providing maximum flexibility across different
client types.

**Example Login Response:**

```json
{
  "success": true,
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "fullName": "John Doe",
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6...",
    "accessTokenExpiresAt": "2025-10-12T15:45:00Z",
    "refreshTokenExpiresAt": "2025-10-19T15:30:00Z",
    "expiresIn": 900,
    "tokenType": "Bearer"
  }
}
```

## 📱 Recommended Storage Strategies by Platform

#### Web Applications (Single Page Apps)

✅ Option 1: In-Memory Storage (Most Secure)
Store tokens in JavaScript variables (lost on page refresh)

```javascript
// Store tokens in memory
let accessToken = null;
let refreshToken = null;

async function login(email, password) {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({email, password, tenantId: 'your-tenant-id'})
    });

    const data = await response.json();
    accessToken = data.data.accessToken;
    refreshToken = data.data.refreshToken;

    // Set up automatic refresh before expiration
    scheduleTokenRefresh(data.data.expiresIn);
}

function scheduleTokenRefresh(expiresIn) {
    // Refresh 1 minute before expiration
    const refreshTime = (expiresIn - 60) * 1000;
    setTimeout(async () => {
        await refreshAccessToken();
    }, refreshTime);
}

async function refreshAccessToken() {
    const response = await fetch('/api/auth/refresh', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({refreshToken})
    });

    const data = await response.json();
    accessToken = data.data.accessToken;
    refreshToken = data.data.refreshToken;

    scheduleTokenRefresh(data.data.expiresIn);
}

// Use access token in requests
async function makeAuthenticatedRequest() {
    const response = await fetch('/api/protected-resource', {
        headers: {
            'Authorization': `Bearer ${accessToken}`
        }
    });
    return response.json();
}
```

✅ Option 2: sessionStorage (Good Balance)
Tokens cleared when browser tab closes.

```js
// Store tokens in sessionStorage
sessionStorage.setItem('access_token', accessToken);
sessionStorage.setItem('refresh_token', refreshToken);

// Retrieve tokens
const accessToken = sessionStorage.getItem('access_token');
const refreshToken = sessionStorage.getItem('refresh_token');

// Clear on logout
sessionStorage.removeItem('access_token');
sessionStorage.removeItem('refresh_token');
```

#### Mobile Applications

#### iOS

Use Keychain for secure token storage:

```swift
import Security

// Store token
let token = "your-token".data(using: .utf8)!
let query: [String: Any] = [
    kSecClass as String: kSecClassGenericPassword,
    kSecAttrAccount as String: "access_token",
    kSecValueData as String: token
]
SecItemAdd(query as CFDictionary, nil)

// Retrieve token
let query: [String: Any] = [
    kSecClass as String: kSecClassGenericPassword,
    kSecAttrAccount as String: "access_token",
    kSecReturnData as String: true
]
var result: AnyObject?
SecItemCopyMatching(query as CFDictionary, &result)
```

#### Android

Use Keychain for secure token storage:

```kotlin
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey

val masterKey = MasterKey.Builder(context)
    .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
    .build()

val sharedPreferences = EncryptedSharedPreferences.create(
    context,
    "auth_tokens",
    masterKey,
    EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
    EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
)

// Store token
sharedPreferences.edit().putString("access_token", token).apply()

// Retrieve token
val token = sharedPreferences.getString("access_token", null)
```

#### React Native

Use react-native-keychain:

```js
import * as Keychain from 'react-native-keychain';

// Store tokens
await Keychain.setGenericPassword('auth_tokens', JSON.stringify({
    accessToken,
    refreshToken
}));

// Retrieve tokens
const credentials = await Keychain.getGenericPassword();
const {accessToken, refreshToken} = JSON.parse(credentials.password);

// Clear tokens
await Keychain.resetGenericPassword();
```

----

🛡️ Security Features Built Into AuthForge

1. Short-Lived Access Tokens (15 minutes)

    - Limits damage window if token is stolen via XSS
    - Forces clients to refresh frequently
    - Enables detection of suspicious activity

2. Single-Use Refresh Tokens (Token Rotation)

    - Each refresh token can only be used once
    - After use, a new refresh token is issued and old one is revoked
    - Automatic theft detection: If an old token is reused, the system knows someone has the stolen token

Token Rotation Flow:

```text
User logs in
  ↓
Receives: AccessToken_1 + RefreshToken_1
  ↓
AccessToken_1 expires after 15 minutes
  ↓
Client uses RefreshToken_1 to get new tokens
  ↓
RefreshToken_1 is REVOKED
  ↓
Receives: AccessToken_2 + RefreshToken_2
  ↓
If RefreshToken_1 is used again:
  → SECURITY ALERT: Token theft detected!
  → ALL tokens for this user are revoked
  → User must re-authenticate
```

3. IP Address and User-Agent Tracking
    - Every refresh token stores the IP address and user agent used to create it
    - Enables audit trails and suspicious activity detection
    - Can detect if token is used from unexpected location

4. Account Lockout Protection
    - Configurable failed login attempt limits (default: 5 attempts)
    - Automatic account lockout after threshold (default: 15 minutes)
    - Protects against brute-force attacks

5. Generic Error Messages (Anti-Enumeration)
    - Login endpoint always returns "Invalid credentials" for:
        - Invalid email format
        - Non-existent users
        - Wrong passwords
        - Inactive accounts

Prevents attackers from discovering which emails are registered

6. Automatic Token Cleanup
    - Old expired and revoked tokens are automatically removed
    - Configurable retention period (default: 90 days)
    - Reduces database bloat and improves query performance

----
🔥 Protecting Against XSS (Cross-Site Scripting)
Since tokens are stored client-side, XSS is the primary threat vector. Follow these practices:
For Web Applications:

Sanitize All User Input

```js
   // ❌ Never do this
element.innerHTML = userInput;

// ✅ Do this
element.textContent = userInput;
```

Use Content Security Policy (CSP) Headers

```http request
Content-Security-Policy: default-src 'self'; script-src 'self' https://trusted-cdn.com
```

Keep Dependencies Updated

```bash
npm audit
npm update
```

Use Modern Frameworks

- React, Vue, Angular automatically escape output
- Avoid dangerouslySetInnerHTML / v-html unless necessary

Avoid Inline JavaScript

```html
<!-- ❌ Bad -->
<button onclick="alert('XSS')">Click me</button>

<!-- ✅ Good -->
<button id="myButton">Click me</button>
<script src="app.js"></script>
```

🔄 Token Refresh Best Practices
Proactive Refresh (Recommended)
Refresh the access token before it expires:

```js
// Refresh 1 minute before expiration
const refreshTime = (expiresIn - 60) * 1000;
setTimeout(refreshAccessToken, refreshTime);
```

Handle Refresh Failures

```js
async function refreshAccessToken() {
    try {
        const response = await fetch('/api/auth/refresh', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({refreshToken})
        });

        if (!response.ok) {
            // Refresh token expired or revoked
            redirectToLogin();
            return;
        }

        const data = await response.json();
        accessToken = data.data.accessToken;
        refreshToken = data.data.refreshToken;

    } catch (error) {
        console.error('Token refresh failed:', error);
        redirectToLogin();
    }
}
```

Silent Refresh Pattern
For in-memory storage, implement silent refresh on app load:

```js
// On app initialization
async function initializeAuth() {
    const storedRefreshToken = sessionStorage.getItem('refresh_token');

    if (storedRefreshToken) {
        try {
            await refreshAccessToken();
// User is logged in, proceed to app
        } catch {
// Refresh failed, redirect to login
            redirectToLogin();
        }
    } else {
        redirectToLogin();
    }
}
```