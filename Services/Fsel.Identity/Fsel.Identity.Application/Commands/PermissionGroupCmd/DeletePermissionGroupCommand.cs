using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.PermissionGroupCmd
{
    public class DeletePermissionGroupCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeletePermissionGroupCommandHandler : IRequestHandler<DeletePermissionGroupCommand, MethodResult<bool>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public DeletePermissionGroupCommandHandler(IPermissionGroupRepository permissionGroupRepository)
        {
            _permissionGroupRepository = permissionGroupRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeletePermissionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var permissionGroup = await _permissionGroupRepository.Queryable.Include(p => p.RoleClaims).Include(p => p.Permissions).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (permissionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (permissionGroup.RoleClaims.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(permissionGroup.RoleClaims));
                return methodResult;
            }

            await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
            {
                await _permissionGroupRepository.DeleteAsync(permissionGroup);
                await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
