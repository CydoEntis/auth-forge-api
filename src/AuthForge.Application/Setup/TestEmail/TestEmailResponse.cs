namespace AuthForge.Application.Setup.TestEmail;

public record TestEmailResponse(
    bool IsSuccessful,
    string Message);