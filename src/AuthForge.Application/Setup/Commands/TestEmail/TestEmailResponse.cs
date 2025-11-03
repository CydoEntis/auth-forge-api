namespace AuthForge.Application.Setup.Commands.TestEmail;

public record TestEmailResponse(
    bool IsSuccessful,
    string Message);