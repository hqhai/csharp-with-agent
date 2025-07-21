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

    public class ChangeStatusPermissionCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeStatusPermissionCommandHandler : IRequestHandler<ChangeStatusPermissionCommand, MethodResult<bool>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleClaimRepository _roleClaimRepository;

        public ChangeStatusPermissionCommandHandler(IPermissionRepository permissionRepository, IRoleClaimRepository roleClaimRepository)
        {
            _permissionRepository = permissionRepository;
            _roleClaimRepository = roleClaimRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusPermissionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var permission = await _permissionRepository.GetByIdAsync(request.Id);
            if (permission == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var roleClaims = await _roleClaimRepository.Queryable.Where(p => p.PermissionId == permission.Id).ToListAsync(cancellationToken);

            await _permissionRepository.ExecuteTransactionAsync(async () =>
            {
                permission.Status = !permission.Status;
                if (roleClaims.Any())
                {
                    roleClaims.ForEach(p => p.Status = permission.Status);
                    await _roleClaimRepository.UpdateListAsync(roleClaims);
                }
                permission = _permissionRepository.Update(permission);
                await _permissionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
