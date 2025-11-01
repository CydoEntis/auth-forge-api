using AuthForge.Application.Common.Models;
using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Admin.Commands.Login;

public sealed record LoginAdminResponse(
    TokenPair Tokens,
    AdminDetails Admin);