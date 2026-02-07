# Testing User Persistence Implementation

## Overview
This document describes how to test the user persistence implementation that stores user accounts in the database instead of browser localStorage.

## Prerequisites
- SQL Server or LocalDB running and accessible
- Connection string configured in `appsettings.json`
- .NET 8.0 SDK installed

## Database Setup

1. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```
   This will create the `Users` table with the following schema:
   - `Id` (string, primary key) - GUID identifier
   - `Name` (string, unique index) - Username
   - `CreatedAt` (DateTime) - When user account was created
   - `LastSeen` (DateTime) - Last activity timestamp
   - `AvatarUrl` (string, nullable) - User avatar URL

## Test Scenarios

### Test 1: First Time User Registration
**Objective**: Verify new users are created in database

**Steps**:
1. Start the application: `dotnet run`
2. Navigate to `/chat`
3. Enter a username (e.g., "Alice") and click "Join Chat"
4. Query the database: `SELECT * FROM Users WHERE Name = 'Alice'`

**Expected Result**:
- User record exists in database with auto-generated GUID
- `CreatedAt` and `LastSeen` timestamps are set
- User appears in online users list

### Test 2: Returning User
**Objective**: Verify existing users are recognized and reuse their ID

**Steps**:
1. Complete Test 1 first
2. Close the browser tab
3. Open a new browser tab and navigate to `/chat`
4. Enter the same username ("Alice") and join
5. Check the database for Alice's record

**Expected Result**:
- Same user ID is reused (query shows only one record for "Alice")
- `LastSeen` timestamp is updated
- Previous messages from this user still show correctly
- User can see their conversation history

### Test 3: Page Refresh Without localStorage
**Objective**: Verify localStorage is not used

**Steps**:
1. Join chat as user "Bob"
2. Open browser DevTools > Application > Local Storage
3. Verify no `blazorchat.user` or `blazorchat.selection` entries exist
4. Send a few messages
5. Refresh the page (F5)
6. Check localStorage again

**Expected Result**:
- No localStorage entries at any point
- After refresh, user must enter username again (expected behavior)
- Upon rejoining with same username, previous messages are retrieved
- Same user ID is assigned

### Test 4: Multiple Concurrent Users
**Objective**: Verify database handles multiple users correctly

**Steps**:
1. Open chat in Browser 1 as "Charlie"
2. Open chat in Browser 2 as "Dana"
3. Send messages between the two users
4. Query database: `SELECT * FROM Users ORDER BY CreatedAt`

**Expected Result**:
- Two distinct user records exist
- Each has unique ID and name
- Messages correctly reference the sender/recipient IDs
- Both users see each other in online users list

### Test 5: Message History with 7-Day Retention
**Objective**: Verify messages still load correctly from database

**Steps**:
1. Join as user "Eve"
2. Send messages to group chat
3. Send direct messages to another user
4. Disconnect and reconnect (enter "Eve" again)

**Expected Result**:
- All messages from last 7 days are loaded
- Messages are correctly filtered per conversation
- User ID remains consistent across sessions

### Test 6: Connection Persistence
**Objective**: Verify SignalR connections work with database users

**Steps**:
1. Join as "Frank"
2. Open chat in second browser as "Grace"
3. Send message from Frank to Grace
4. Temporarily disconnect Frank's network
5. Reconnect Frank's network

**Expected Result**:
- Frank automatically reconnects via SignalR
- Frank can still send/receive messages
- `LastSeen` updates on reconnection
- No errors in browser console

## Database Verification Queries

```sql
-- View all users
SELECT * FROM Users ORDER BY CreatedAt DESC;

-- Count users
SELECT COUNT(*) FROM Users;

-- Find users not seen in last 24 hours
SELECT * FROM Users 
WHERE LastSeen < DATEADD(hour, -24, GETUTCDATE());

-- View messages for a specific user
SELECT m.*, u.Name as SenderName
FROM ChatMessages m
INNER JOIN Users u ON m.SenderId = u.Id
WHERE m.SenderId = '<userId>' OR m.RecipientId = '<userId>'
ORDER BY m.Timestamp DESC;

-- Verify no duplicate usernames
SELECT Name, COUNT(*) as Count
FROM Users
GROUP BY Name
HAVING COUNT(*) > 1;
```

## Troubleshooting

### Issue: "A network-related or instance-specific error"
**Solution**: Verify SQL Server is running and connection string is correct in `appsettings.json`

### Issue: "Cannot insert duplicate key"
**Solution**: This indicates the unique index on `User.Name` is working correctly. Users cannot register with existing usernames.

### Issue: Messages not showing after reconnect
**Solution**: Check that the username entered is exactly the same (case-sensitive)

## Success Criteria

All tests pass when:
- ✅ Users persist in database across sessions
- ✅ No localStorage is used for user data
- ✅ Same username gets same user ID
- ✅ Messages load correctly with 7-day retention
- ✅ Multiple users can chat simultaneously
- ✅ SignalR reconnection works properly
- ✅ No errors in browser console or server logs
