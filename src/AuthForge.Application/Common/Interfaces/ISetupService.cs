using AuthForge.Application.Common.Models;

namespace AuthForge.Application.Common.Interfaces;

public interface ISetupService
{
    Task<bool> IsSetupCompleteAsync();
    Task<bool> TestDatabaseConnectionAsync(DatabaseConfiguration config, CancellationToken cancellationToken = default);
    Task<bool> TestEmailConnectionAsync(EmailConfiguration config, string testRecipient,
        CancellationToken cancellationToken = default);
    Task CompleteSetupAsync(SetupConfiguration config, CancellationToken cancellationToken = default);
}

