namespace AuthForge.Application.EndUsers.Commands.LockEndUser;

public record LockEndUserResponse(
    string Message,
    DateTime LockedOutUntil);