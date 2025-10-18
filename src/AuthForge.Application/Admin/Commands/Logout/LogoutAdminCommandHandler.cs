using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.Logout;

public sealed class LogoutAdminCommandHandler : ICommandHandler<LogoutAdminCommand, Result>
{
    private readonly IAdminRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutAdminCommandHandler(
        IAdminRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Result> Handle(
        LogoutAdminCommand command,
        CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllAsync(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}