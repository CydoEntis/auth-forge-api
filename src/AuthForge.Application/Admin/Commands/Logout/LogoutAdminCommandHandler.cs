using AuthForge.Application.Common.Interfaces;
using AuthForge.Domain.Common;
using Mediator;

namespace AuthForge.Application.Admin.Commands.Logout;

public sealed class LogoutAdminCommandHandler 
    : ICommandHandler<LogoutAdminCommand, Result<LogoutAdminResponse>>
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

    public async ValueTask<Result<LogoutAdminResponse>> Handle(
        LogoutAdminCommand command,
        CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAllAsync(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LogoutAdminResponse("Successfully logged out from all devices");
        return Result<LogoutAdminResponse>.Success(response);
    }
}