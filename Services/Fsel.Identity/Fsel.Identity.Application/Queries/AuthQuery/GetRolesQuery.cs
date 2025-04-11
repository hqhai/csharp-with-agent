using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.AuthQuery
{
    public class GetRolesQuery : IRequest<MethodResult<IList<RoleModel>>>
    {
    }

    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, MethodResult<IList<RoleModel>>>
    {
        private readonly RoleManager<Role> _roleManager;

        public GetRolesQueryHandler(RoleManager<Role> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<MethodResult<IList<RoleModel>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<RoleModel>>();

            var roles = await _roleManager.Roles.Select(p => new RoleModel
            {
                Id = p.Id,
                Name = p.Name
            }).ToListAsync(cancellationToken);

            methodResult.Result = roles;
            return methodResult;
        }
    }
}
