using AuthForge.Api.Common.Responses;
using AuthForge.Application.Admin.Commands.ChangePassword;
using Mediator;

namespace AuthForge.Api.Endpoints.Admin;

public static class ChangeAdminPasswordEndpoint
{
    public static void MapChangeAdminPasswordEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/me/change-password", async (
                ChangeAdminPasswordRequest request,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var command = new ChangeAdminPasswordCommand(
                    request.CurrentPassword,
                    request.NewPassword);

                var result = await mediator.Send(command, cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(ApiResponse<ChangeAdminPasswordResponse>.SuccessResponse(result.Value!))
                    : Results.BadRequest(ApiResponse<ChangeAdminPasswordResponse>.FailureResponse(result.Error!));
            })
            .RequireAuthorization("Admin")
            .WithName("ChangeAdminPassword")
            .WithTags("Admin")
            .Produces<ApiResponse<ChangeAdminPasswordResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ChangeAdminPasswordResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ChangeAdminPasswordResponse>>(StatusCodes.Status401Unauthorized);
    }
}

public sealed record ChangeAdminPasswordRequest(
    string CurrentPassword,
    string NewPassword);