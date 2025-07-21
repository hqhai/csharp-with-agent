namespace Fsel.Identity.Application.Commands.PermissionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeletePermissionCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, MethodResult<bool>>
    {
        private readonly IPermissionRepository _permissionRepository;

        public DeletePermissionCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var permission = await _permissionRepository.Queryable.Include(p => p.RoleClaims).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (permission == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            if (permission.RoleClaims.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(permission.RoleClaims));
                return methodResult;
            }

            await _permissionRepository.ExecuteTransactionAsync(async () =>
            {
                await _permissionRepository.DeleteAsync(permission);
                await _permissionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
