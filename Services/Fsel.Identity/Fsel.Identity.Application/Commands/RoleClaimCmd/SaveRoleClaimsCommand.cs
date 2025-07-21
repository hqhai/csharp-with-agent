using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Permissions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.RoleClaimCmd
{
    public class SaveRoleClaimsCommand : SaveRoleClaimCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class SaveRoleClaimsCommandHandler : IRequestHandler<SaveRoleClaimsCommand, MethodResult<bool>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IRoleClaimRepository _roleClaimRepository;
        private readonly RoleManager<Role> _roleManager;

        public SaveRoleClaimsCommandHandler(IPermissionGroupRepository permissionGroupRepository, IRoleClaimRepository roleClaimRepository, RoleManager<Role> roleManager)
        {
            _permissionGroupRepository = permissionGroupRepository;
            _roleClaimRepository = roleClaimRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<bool>> Handle(SaveRoleClaimsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.RoleClaims == null || !request.RoleClaims.Any())
            {
                var removeRoleClaims = await _roleClaimRepository.Queryable.Where(p => p.RoleId == request.RoleId).ToListAsync(cancellationToken);
                await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
                {
                    await _roleClaimRepository.DeleteListAsync(removeRoleClaims);
                    await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
                return methodResult;
            }

            var isDuplicate = request.RoleClaims
                            .GroupBy(x => new { x.PermissionGroupId, x.PermissionId })
                            .Any(g => g.Count() > 1);
            if (isDuplicate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            var role = await _roleManager.Roles.FirstOrDefaultAsync(p => p.Id == request.RoleId, cancellationToken);
            if (role == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var permissionGroupIds = request.RoleClaims.Select(p => p.PermissionGroupId).Distinct().ToList();

            var permissionGroups = await _permissionGroupRepository.Queryable.Include(p => p.Permissions).WhereBulkContains(permissionGroupIds, p => p.Id).ToListAsync(cancellationToken);
            if (permissionGroups.Count != permissionGroupIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            request.RoleClaims.ForEach(p =>
            {
                var permissionGroup = permissionGroups.First(x => x.Id == p.PermissionGroupId);
                var permissionIds = permissionGroup.Permissions.Select(x => x.Id).ToList();
                if (!permissionIds.Contains(p.PermissionId))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), p.PermissionGroupId.ToString(), p.PermissionId.ToString());
                }
            });

            if (methodResult.StatusCode != null)
            {
                return methodResult;
            }

            var roleClaims = await _roleClaimRepository.Queryable.Where(p => p.RoleId == request.RoleId && p.Status).ToListAsync(cancellationToken);

            var deleteRoleClaims = new List<RoleClaim>();
            var createRoleClaims = new List<RoleClaim>();

            roleClaims.ForEach(dbClaim =>
            {
                bool existsInRequest = request.RoleClaims.Any(rc =>
                    rc.PermissionGroupId == dbClaim.PermissionGroupId &&
                    rc.PermissionId == dbClaim.PermissionId);

                if (!existsInRequest)
                {
                    deleteRoleClaims.Add(dbClaim);
                }
            });

            request.RoleClaims.ForEach(reqClaim =>
            {
                bool existsInDb = roleClaims.Any(db =>
                   db.PermissionGroupId == reqClaim.PermissionGroupId &&
                   db.PermissionId == reqClaim.PermissionId);

                if (!existsInDb)
                {
                    var permissionGroup = permissionGroups.First(p => p.Id == reqClaim.PermissionGroupId);
                    createRoleClaims.Add(new RoleClaim
                    {
                        RoleId = role.Id,
                        PermissionGroupId = reqClaim.PermissionGroupId,
                        PermissionId = reqClaim.PermissionId,
                        Status = true,
                        ClaimType = permissionGroup.ClaimType,
                        ClaimValue = permissionGroup.Permissions.First(x => x.Id == reqClaim.PermissionId).ClaimValue
                    });
                }
            });

            await _permissionGroupRepository.ExecuteTransactionAsync(async () =>
            {
                await _roleClaimRepository.AddRangeAsync(createRoleClaims);
                await _roleClaimRepository.DeleteListAsync(deleteRoleClaims);
                await _permissionGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
