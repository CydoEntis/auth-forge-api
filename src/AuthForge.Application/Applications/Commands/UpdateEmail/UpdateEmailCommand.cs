using AuthForge.Domain.ValueObjects;

namespace AuthForge.Application.Applications.Commands.UpdateEmailConfig;

public void UpdateEmailConfiguration(ApplicationEmailSettings settings)
{
    EmailSettings = settings ?? throw new ArgumentNullException(nameof(settings));
    UpdatedAtUtc = DateTime.UtcNow;
}