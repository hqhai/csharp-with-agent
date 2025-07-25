// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUsersByRolesQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<UserModel>>>
    {
        public IList<EnumRole>? Roles { get; set; }
    }

    public class GetUsersByRolesQueryHandler : IRequestHandler<GetUsersByRolesQuery, MethodResult<PagingItemsModel<UserModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly Microsoft.AspNetCore.Identity.RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        public GetUsersByRolesQueryHandler(UserManager<User> userManager,
                                                 RoleManager<Role> roleManager,
                                                 IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<UserModel>>> Handle(GetUsersByRolesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Roles);
            MethodResult<PagingItemsModel<UserModel>> methodResult = new MethodResult<PagingItemsModel<UserModel>>();

            List<string> roleNames = request.Roles.Select(role => role.ToString().Trim()).ToList();

            var userQuerys = from a in _userManager.Users
                             join ur in _userRoleRepository.GetQuery() on a.Id equals ur.UserId
                             join r in _roleManager.Roles on ur.RoleId equals r.Id
                             where r.Name != null && roleNames.Contains(r.Name)
                             select new UserModel
                             {
                                 Id = a.Id,
                                 FullName = a.FullName,
                                 PhoneNumber = a.PhoneNumber
                             };

            int totalItem = await userQuerys.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await userQuerys.ApplySortAndPaging(request)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<UserModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
