# Architecture Changes: User Persistence

## Summary
This document describes the architectural changes made to implement database-backed user persistence, removing reliance on browser localStorage.

## Problem Statement
Previously:
- User identity (userId, userName) was stored in browser localStorage
- Users lost their identity if localStorage was cleared
- No persistent user accounts existed
- Page refresh required retrieving user from localStorage

## Solution
Implemented database-backed user accounts where:
- User identity persists in SQL Server database
- Users are identified by username across sessions
- localStorage is completely removed
- Page refresh requires re-entering username, but same user ID is assigned

## Architecture Overview

### Previous Architecture
```
Browser (localStorage) ──────┐
                             │
                             ▼
         Blazor Server Components
                             │
                             ▼
         SignalR Hub (ChatHub)
                             │
                             ▼
    In-Memory Dictionary (Users) ◄── ConnectionId + Session Data
                             │
                             ▼
         Database (Messages only)
```

### New Architecture
```
         Blazor Server Components
                             │
                             ▼
         SignalR Hub (ChatHub)
              │              │
              ├──────────────┴─── Get/Create User by Name
              │                         │
              ▼                         ▼
    In-Memory Sessions           Database (Users Table)
    (ConnectionId only)          - User Identity
                                 - User History
              │
              ▼
         Database (Messages)
```

## Key Changes

### 1. User Model Enhancement
**File**: `Models/User.cs`

**Changes**:
- Added EF Core attributes (`[Key]`, `[Required]`, `[MaxLength]`)
- Added `[NotMapped]` for session-only properties (ConnectionId, IsOnline)
- Added `CreatedAt` timestamp for audit trail
- Properties now properly support database persistence

**Why**: Enable User entity to be properly stored and queried in database

### 2. Database Context Update
**File**: `Data/ChatDbContext.cs`

**Changes**:
- Added `Users` DbSet
- Created unique index on `User.Name` (prevents duplicate usernames)
- Created index on `User.LastSeen` (for efficient queries)

**Why**: Enable database storage and efficient querying of user accounts

### 3. Service Layer Separation
**Files**: `Services/IChatService.cs`, `Services/ChatService.cs`

**Changes**:
- Separated database operations from session management
- Added database methods:
  - `GetUserByNameAsync()` - Look up user by username
  - `GetUserByIdAsync()` - Look up user by ID
  - `CreateUserAsync()` - Create new user account
  - `UpdateUserLastSeenAsync()` - Track user activity
- Renamed session methods (added "Session" suffix for clarity):
  - `AddUserSession()` - Add to in-memory active sessions
  - `RemoveUserSession()` - Remove from active sessions
  - `GetUserSessionById()` - Get active session user

**Why**: Clear separation between persistent identity (database) and active sessions (memory)

### 4. Hub Authentication Flow
**File**: `Hubs/ChatHub.cs`

**Changes**:
- Modified `UserConnected` signature: `(userId, userName)` → `(userName)`
- New flow:
  1. Receive username from client
  2. Query database for existing user
  3. Create new user if not found
  4. Update LastSeen for existing users
  5. Send userId back to client via `UserIdAssigned` event
  6. Create in-memory session with ConnectionId
- Updated all message routing to use `GetUserSessionById` instead of `GetUserById`

**Why**: Server now controls user identity, ensuring consistency and database persistence

### 5. Client-Side Simplification
**File**: `Components/Pages/Chat.razor.cs`

**Changes**:
- Removed all localStorage read/write operations
- Removed `StoredChatUser` and `StoredChatSelection` classes
- Changed `userId` from generated GUID to empty string (server-assigned)
- Added handler for `UserIdAssigned` event from server
- Simplified reconnection logic (only sends userName)
- Removed selection persistence methods

**Why**: Client is now stateless, all persistence handled by server/database

### 6. JavaScript Cleanup
**Files**: `wwwroot/js/chat.js`, `Components/App.razor`

**Changes**:
- Completely removed `chatStorage` object
- Removed all localStorage API calls
- Kept only UI utility functions (scroll, file reading, warnings)

**Why**: localStorage completely removed as per requirements

## Data Flow

### User Registration/Login Flow
```
1. User enters username → Client
2. Client sends username → Server (SignalR)
3. Server queries database for username
   ├─ User exists: Get existing user ID
   └─ User doesn't exist: Create new user record
4. Server creates in-memory session (ConnectionId + User data)
5. Server sends userId back → Client
6. Server loads user's messages → Client
7. Server broadcasts user online status → All clients
```

### Message Send Flow
```
1. Client sends message + userId + recipientId → Server
2. Server validates userId in active sessions
3. Server saves message to database (with userId as SenderId)
4. Server looks up recipient's session (if direct message)
5. Server sends message via SignalR → Target recipients
```

### Page Refresh Flow
```
1. User refreshes page
2. SignalR connection disconnects
   └─ Server removes from active sessions
   └─ Server updates LastSeen in database
3. User sees join screen (must re-enter username)
4. User enters username → Follows registration flow
5. Server recognizes username → Assigns same userId
6. User's message history loads from database
```

## Benefits

### ✅ Persistence
- User accounts survive browser sessions
- Identity preserved across devices (same username = same ID)
- Full audit trail (CreatedAt, LastSeen)

### ✅ Data Integrity
- Single source of truth (database)
- Unique username enforcement via database constraint
- No localStorage synchronization issues

### ✅ Security
- No client-side storage of user identity
- Server controls authentication
- Cannot forge user IDs from client

### ✅ Scalability
- Ready for future authentication systems
- Can add password/email fields easily
- Supports multiple server instances (shared database)

## Migration Path

### For Existing Users
**Before**: userId stored in localStorage (random GUID)
**After**: userId assigned by server based on username lookup

**Impact**: 
- Existing users must re-enter username after update
- New userId will be assigned (old messages remain with old userId)
- Optional: Add migration script to associate old userIds with new accounts

### For Developers
1. Apply database migration: `dotnet ef database update`
2. No code changes needed in consuming applications
3. Remove any localStorage access code

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastSeen DATETIME2 NOT NULL,
    AvatarUrl NVARCHAR(MAX) NULL,
    
    CONSTRAINT UQ_Users_Name UNIQUE (Name)
);

CREATE INDEX IX_Users_LastSeen ON Users(LastSeen);
```

### ChatMessages Table (Unchanged)
```sql
-- Messages reference Users.Id via SenderId and RecipientId
-- Foreign key constraints not added to allow message retention
-- even if user account is deleted (optional)
```

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "ChatDatabase": "Server=...;Database=BlazorChat;..."
  },
  "ChatRetention": {
    "HistoryDays": 7,      // Messages shown to users
    "CleanupAfterDays": 20 // Auto-delete threshold
  }
}
```

## Future Enhancements

### Potential Additions
1. **Password Authentication**: Add Password field + hashing
2. **Email Verification**: Add Email field + verification flow
3. **Profile Management**: Allow users to update Name, AvatarUrl
4. **User Roles**: Add admin/moderator capabilities
5. **Account Linking**: Associate multiple devices with one account
6. **Session Management**: Track multiple active sessions per user
7. **User Search**: Find users by name/email
8. **User Analytics**: Login frequency, message counts, etc.

### Breaking Changes to Consider
- None! Current changes are backward compatible with message structure
- WebForm integration still works (provides userName via query string)

## Testing Recommendations

See `TESTING.md` for comprehensive test scenarios covering:
- First-time user registration
- Returning user recognition
- localStorage absence verification
- Multi-user scenarios
- 7-day message retention
- Connection persistence

## Conclusion

The architecture now properly separates concerns:
- **Client**: UI and real-time communication
- **Server**: Business logic and session management  
- **Database**: Persistent storage and identity management

This provides a solid foundation for adding authentication and scaling the application.
