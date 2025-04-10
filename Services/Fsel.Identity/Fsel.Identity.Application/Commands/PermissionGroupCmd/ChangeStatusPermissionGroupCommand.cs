using System.Linq.Dynamic.Core;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.PermissionGroupCmd
{
    public class ChangeStatusPermissionGroupCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class ChangeStatusPermissionGroupCommandHandler : IRequestHandler<ChangeStatusPermissionGroupCommand, MethodResult<bool>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IRoleClaimRepository _roleClaimRepository;

        public ChangeStatusPermissionGroupCommandHandler(IPermissionGroupRepository permissionGroupRepository, IRoleClaimRepository roleClaimRepository)
        {
            _permissionGroupRepository = permissionGroupRepository;
            _roleClaimRepository = roleClaimRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusPermissionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var permissionGroup = await _permissionGroupRepository.GetByIdAsync(request.Id);
            if (permissionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var roleClaims = await _roleClaimRepository.Queryable.Where(p => p.PermissionGroupId == permissionGroup.Id).ToListAsync(cancellationToken);

            await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
            {
                permissionGroup.Status = !permissionGroup.Status;
                if (roleClaims.Any())
                {
                    roleClaims.ForEach(p => p.Status = permissionGroup.Status);
                    await _roleClaimRepository.UpdateListAsync(roleClaims);
                }
                permissionGroup = _permissionGroupRepository.Update(permissionGroup);
                await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
