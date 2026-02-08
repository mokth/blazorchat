# Authentication & User Management Setup Guide

## Overview

This guide explains how to set up and use the new authentication and user management system in BlazorChat.

## Prerequisites

- SQL Server or LocalDB running and accessible
- Connection string configured in `appsettings.json`
- .NET 8.0 SDK installed

## Database Migration

Before using the authentication features, you need to apply the database migrations:

```bash
dotnet ef database update
```

This will:
- Add new columns to the `Users` table (Email, PasswordHash, EmailVerified, EmailVerificationToken, ResetPasswordToken, ResetPasswordTokenExpiry)
- Create `Groups` and `GroupMembers` tables for future group chat functionality
- Add proper indexes for email addresses and group relationships

## Features Implemented

### 1. User Registration (`/register`)

- Users can register with:
  - Username (3-100 characters)
  - Email address
  - Password (minimum 6 characters)
- Email verification token is generated
- Password is securely hashed using BCrypt
- Verification email link is logged to console (for development)

### 2. Email Verification (`/verify-email?token=<token>`)

- Users click the verification link from their email
- Token is validated and user's email is marked as verified
- Users can then log in

### 3. Login (`/login`)

- Users log in with email and password
- Checks if email is verified before allowing login
- Session is stored using ProtectedSessionStorage
- Redirects to chat after successful login

### 4. Forgot Password (`/forgot-password`)

- Users can request a password reset
- Reset token is generated with 1-hour expiry
- Reset email link is logged to console (for development)
- Returns success message even if email doesn't exist (prevents email enumeration)

### 5. Reset Password (`/reset-password?token=<token>`)

- Users click reset link from email
- Can set new password (minimum 6 characters)
- Token is validated and must not be expired
- Password is securely hashed and saved

### 6. User Profile (`/profile`)

- Authenticated users can:
  - View their profile information
  - Update username
  - Update avatar URL
  - Access change password page
- Email address cannot be changed (for security)

### 7. Change Password (`/change-password`)

- Authenticated users can change their password
- Must provide current password for verification
- New password must meet minimum requirements

### 8. Logout (`/logout`)

- Clears session storage
- Redirects to login page

## Authentication Flow

### New Users
1. Visit `/` (redirects to `/login`)
2. Click "Register here"
3. Fill in registration form
4. Check console/logs for verification email link
5. Click verification link or navigate to `/verify-email?token=<token>`
6. Return to login and sign in
7. Access chat application

### Returning Users
1. Visit `/` (redirects to `/login`)
2. Enter email and password
3. Automatically redirected to `/chat`

### Guest Access
- Users can still join as guests without registration
- On the chat page, click "Join as Guest" and enter a name
- Guest users do not have access to profile or other authenticated features

## Configuration

### Email Settings (appsettings.json)

```json
"Email": {
  "BaseUrl": "http://localhost:5000",
  "FromAddress": "noreply@blazorchat.com",
  "FromName": "BlazorChat"
}
```

### Production Email Setup

The current `EmailService` implementation logs email links to the console for development. For production, you need to:

1. Choose an email service provider (SendGrid, Mailgun, Amazon SES, etc.)
2. Update `Services/EmailService.cs` to use the provider's API or SMTP
3. Add required configuration to `appsettings.json`

Example with SMTP:

```csharp
public async Task SendVerificationEmailAsync(string email, string name, string token)
{
    var baseUrl = _configuration["Email:BaseUrl"] ?? "http://localhost:5000";
    var verificationLink = $"{baseUrl}/verify-email?token={token}";
    
    var smtpClient = new SmtpClient("smtp.example.com")
    {
        Port = 587,
        Credentials = new NetworkCredential("username", "password"),
        EnableSsl = true,
    };

    var mailMessage = new MailMessage
    {
        From = new MailAddress(_configuration["Email:FromAddress"]),
        Subject = "Verify your BlazorChat account",
        Body = $"Hi {name},\n\nClick here to verify your email: {verificationLink}",
        IsBodyHtml = false,
    };
    mailMessage.To.Add(email);

    await smtpClient.SendMailAsync(mailMessage);
}
```

## Security Features

### Password Security
- Passwords are hashed using BCrypt with automatic salt generation
- Minimum 6 characters required
- Never stored in plain text

### Email Verification
- Users must verify email before logging in
- Verification tokens are unique GUIDs
- Tokens are deleted after successful verification

### Password Reset
- Reset tokens expire after 1 hour
- Tokens are unique GUIDs
- Single-use tokens (deleted after password reset)
- No email enumeration (always returns success)

### Session Management
- Uses ASP.NET Core's ProtectedSessionStorage
- Session data is encrypted
- Automatic session cleanup on logout

## API Services

### IAuthService

- `RegisterAsync(name, email, password)` - Register new user
- `LoginAsync(email, password)` - Authenticate user
- `VerifyEmailAsync(token)` - Verify email address
- `RequestPasswordResetAsync(email)` - Request password reset
- `ResetPasswordAsync(token, newPassword)` - Reset password
- `GetUserByIdAsync(userId)` - Get user by ID
- `GetUserByEmailAsync(email)` - Get user by email
- `UpdateProfileAsync(userId, name, avatarUrl)` - Update user profile
- `ChangePasswordAsync(userId, currentPassword, newPassword)` - Change password
- `VerifyPassword(password, passwordHash)` - Verify password hash
- `HashPassword(password)` - Hash password

### IGroupService (For Future Use)

- `CreateGroupAsync(name, description, createdBy)` - Create new group
- `AddMemberAsync(groupId, userId, isAdmin)` - Add member to group
- `RemoveMemberAsync(groupId, userId)` - Remove member from group
- `UpdateGroupAsync(groupId, name, description, avatarUrl)` - Update group
- `DeleteGroupAsync(groupId, userId)` - Delete group
- `GetUserGroupsAsync(userId)` - Get user's groups
- `GetGroupByIdAsync(groupId)` - Get group by ID
- `GetGroupMembersAsync(groupId)` - Get group members
- `IsUserMemberAsync(groupId, userId)` - Check if user is member
- `IsUserAdminAsync(groupId, userId)` - Check if user is admin

## Testing

### Test Scenario 1: User Registration
1. Navigate to `/register`
2. Enter username, email, and password
3. Submit form
4. Check server logs for verification link
5. Copy token from log
6. Navigate to `/verify-email?token=<token>`
7. Confirm verification success

### Test Scenario 2: User Login
1. Navigate to `/login`
2. Enter registered email and password
3. Verify redirect to `/chat`
4. Confirm navigation menu shows with Profile and Logout options

### Test Scenario 3: Password Reset
1. Navigate to `/forgot-password`
2. Enter registered email
3. Check server logs for reset link
4. Copy token from log
5. Navigate to `/reset-password?token=<token>`
6. Enter new password
7. Login with new password

### Test Scenario 4: Profile Management
1. Login as authenticated user
2. Navigate to `/profile`
3. Update username
4. Upload avatar URL
5. Save changes
6. Navigate to `/change-password`
7. Enter current and new passwords
8. Verify password change

## Troubleshooting

### Issue: "Email already registered"
**Solution**: Use a different email address or reset password for existing account

### Issue: "Please verify your email before logging in"
**Solution**: Check server logs for verification link and complete email verification

### Issue: "Invalid or expired reset token"
**Solution**: Request a new password reset link (tokens expire after 1 hour)

### Issue: Database migration fails
**Solution**: Ensure SQL Server is running and connection string is correct

### Issue: Session storage errors
**Solution**: Clear browser cache and cookies, ensure ProtectedSessionStorage is working

## Database Schema Changes

### Users Table (Updated)
- `Id` (string, primary key, GUID)
- `Name` (string, unique index)
- `Email` (string, unique index) - NEW
- `PasswordHash` (string) - NEW
- `EmailVerified` (bool) - NEW
- `EmailVerificationToken` (string, nullable) - NEW
- `ResetPasswordToken` (string, nullable) - NEW
- `ResetPasswordTokenExpiry` (datetime, nullable) - NEW
- `CreatedAt` (datetime)
- `LastSeen` (datetime)
- `AvatarUrl` (string, nullable)

### Groups Table (New)
- `Id` (string, primary key, GUID)
- `Name` (string, index)
- `Description` (string, nullable)
- `CreatedBy` (string)
- `CreatedAt` (datetime)
- `AvatarUrl` (string, nullable)

### GroupMembers Table (New)
- `Id` (string, primary key, GUID)
- `GroupId` (string, foreign key)
- `UserId` (string)
- `JoinedAt` (datetime)
- `IsAdmin` (bool)
- Unique index on (GroupId, UserId)

## Future Enhancements

1. **Group Chat UI**: Add interface for creating and managing groups
2. **Email Service**: Implement production email sending
3. **Social Login**: Add OAuth providers (Google, Facebook, etc.)
4. **Two-Factor Authentication**: Add 2FA support
5. **Account Settings**: Expand profile with more customization options
6. **Admin Panel**: Add administrative interface for user management
7. **User Blocking**: Add ability to block/report users
8. **Message Encryption**: Add end-to-end encryption for messages

## Security Summary

✅ **No security vulnerabilities detected** by CodeQL analysis
✅ Passwords securely hashed with BCrypt
✅ Email verification required before login
✅ Session data encrypted with ProtectedSessionStorage
✅ Password reset tokens with expiration
✅ No email enumeration in password reset
✅ Input validation on all forms
✅ SQL injection prevention via Entity Framework
✅ CSRF protection via ASP.NET Core Antiforgery

## Support

For issues or questions, please refer to:
- `TESTING.md` for additional testing scenarios
- `ARCHITECTURE_CHANGES.md` for system architecture details
- GitHub Issues for bug reports and feature requests
