// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Fsel.Shared.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Core.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class SearchUsersByRolesQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<UserModel>>>
    {
        public string? RolesStr { get; set; }

        [JsonIgnore]
        public IList<EnumRole>? Roles => RolesStr.ToList<EnumRole>();
    }

    public class SearchUsersByRolesQueryHandler : IRequestHandler<SearchUsersByRolesQuery, MethodResult<PagingItemsModel<UserModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        public SearchUsersByRolesQueryHandler(UserManager<User> userManager, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<UserModel>>> Handle(SearchUsersByRolesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Roles);
            var methodResult = new MethodResult<PagingItemsModel<UserModel>>();

            var roleNames = request.Roles.Select(role => role.ToString().Trim()).ToList();

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

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                userQuerys = userQuerys.Where(m => m.FullName != null && EF.Functions.Contains(m.FullName, $"\"{request.Keyword.Trim()}\"") && m.FullName.Contains(request.Keyword.Trim()));
            }

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
