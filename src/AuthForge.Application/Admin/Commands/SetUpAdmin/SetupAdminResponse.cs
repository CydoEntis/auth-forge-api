using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Admin.Commands.SetUpAdmin;

public record SetupAdminResponse(
    string Message,
    TokenPair Tokens,
    AdminDetails Admin);