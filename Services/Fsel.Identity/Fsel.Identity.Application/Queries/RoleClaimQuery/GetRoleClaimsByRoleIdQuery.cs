namespace Fsel.Identity.Application.Queries.RoleClaimQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels.Permissions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetRoleClaimsByRoleIdQuery : IRequest<MethodResult<IList<RoleClaimModel>>>
    {
        public Guid RoleId { get; set; }
    }

    public class GetRoleClaimsByRoleIdQueryHandler : IRequestHandler<GetRoleClaimsByRoleIdQuery, MethodResult<IList<RoleClaimModel>>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleClaimRepository _roleClaimRepository;

        public GetRoleClaimsByRoleIdQueryHandler(IPermissionGroupRepository permissionGroupRepository, IPermissionRepository permissionRepository, IRoleClaimRepository roleClaimRepository)
        {
            _permissionGroupRepository = permissionGroupRepository;
            _permissionRepository = permissionRepository;
            _roleClaimRepository = roleClaimRepository;
        }

        public async Task<MethodResult<IList<RoleClaimModel>>> Handle(GetRoleClaimsByRoleIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<RoleClaimModel>>();

            var query = await (from p in _permissionRepository.Queryable
                               join pg in _permissionGroupRepository.Queryable on p.PermissionGroupId equals pg.Id
                               where p.Status && pg.Status
                               select new RoleClaimModel()
                               {
                                   PermissionGroupName = pg.Name,
                                   PermissionName = p.Name,
                                   ClaimType = pg.ClaimType,
                                   ClaimValue = p.ClaimValue,
                                   PermissionGroupId = pg.Id,
                                   PermissionId = p.Id
                               }).ToListAsync(cancellationToken);

            var roleClaims = await _roleClaimRepository.Queryable.Where(p => p.RoleId == request.RoleId && p.Status).ToListAsync(cancellationToken);
            var permissionGroupIds = roleClaims.Select(p => p.PermissionGroupId).ToList();
            var permissionIds = roleClaims.Select(p => p.PermissionId).ToList();

            query.ForEach(p =>
            {
                if (permissionGroupIds.Contains(p.PermissionGroupId) && permissionIds.Contains(p.PermissionId))
                {
                    p.PermissionGroupStatus = true;
                    p.PermissionStatus = true;
                }
            });

            methodResult.Result = query;
            return methodResult;
        }
    }
}
