namespace AuthForge.Application.Admin.Commands.TestEmail;

public record TestEmailResponse(
    bool IsSuccessful,
    string Message);