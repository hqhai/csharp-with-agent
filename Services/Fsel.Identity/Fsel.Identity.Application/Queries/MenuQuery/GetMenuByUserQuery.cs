using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Shared.Models.ShareModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.MenuQuery
{
    public class GetMenuByUserQuery : IRequest<MethodResult<IList<MenuConfig>>>
    {
    }

    public class GetMenuByUserQueryHandler : IRequestHandler<GetMenuByUserQuery, MethodResult<IList<MenuConfig>>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IRoleClaimRepository _roleClaimRepository;
        private readonly AuthContext _authContext;
        private readonly RoleManager<Role> _roleManager;

        public GetMenuByUserQueryHandler(IMenuRepository menuRepository, IPermissionGroupRepository permissionGroupRepository, IRoleClaimRepository roleClaimRepository, AuthContext authContext, RoleManager<Role> roleManager)
        {
            _menuRepository = menuRepository;
            _permissionGroupRepository = permissionGroupRepository;
            _roleClaimRepository = roleClaimRepository;
            _authContext = authContext;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<IList<MenuConfig>>> Handle(GetMenuByUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<MenuConfig>>();

            var role = _authContext.Roles?.FirstOrDefault();
            if (string.IsNullOrEmpty(role))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), role);
                return methodResult;
            }

            var query = await (from r in _roleManager.Roles
                               join rc in _roleClaimRepository.Queryable on r.Id equals rc.RoleId
                               join pg in _permissionGroupRepository.Queryable on rc.PermissionGroupId equals pg.Id
                               where r.Name == role && pg.MenuId.HasValue
                               select pg.MenuId).ToListAsync(cancellationToken);

            query = query.Distinct().ToList();

            var menus = await _menuRepository.Queryable.WhereBulkContains(query, p => p.Id).OrderBy(p => p.Index).ToListAsync(cancellationToken);

            methodResult.Result = menus.Where(p => p.Config != null).Select(p => p.Config!).ToList();
            return methodResult;
        }
    }
}
