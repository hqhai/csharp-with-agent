// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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

    public class SearchUsersQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<UserManageModel>>>
    {
        public Guid? GroupId { get; set; }

        public Guid? ManageUserId { get; set; }
    }

    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, MethodResult<PagingItemsModel<UserManageModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly RoleManager<Role> _roleManager;

        public SearchUsersQueryHandler(UserManager<User> userManager,
                                       IHumanRepository humanRepository,
                                       IPlatformRepository platformRepository,
                                       IUserPlatformRepository userPlatformRepository,
                                       IUserRoleRepository userRoleRepository,
                                       RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<PagingItemsModel<UserManageModel>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<UserManageModel>>();

            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMSAdmin, cancellationToken);

            if (platform == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumPlatformCode.LMSAdmin));
                return methodResult;
            }

            var userQuerys = from a in _userManager.Users
                             join h in _humanRepository.Queryable on a.Id equals h.UserId into human
                             from h in human.DefaultIfEmpty()
                             join ur in _userRoleRepository.Queryable on a.Id equals ur.UserId into userRole
                             from ur in userRole.DefaultIfEmpty()
                             join up in _userPlatformRepository.Queryable on a.Id equals up.UserId
                             where up.PlatformId == platform.Id
                             group new { a, h, ur, up } by a.Id into groupedUsers
                             select new UserManageModel
                             {
                                 Id = groupedUsers.Key,
                                 FullName = groupedUsers.First().a.FullName,
                                 Email = groupedUsers.First().a.Email,
                                 PhoneNumber = groupedUsers.First().a.PhoneNumber,
                                 Birthday = groupedUsers.First().h.Birthday,
                                 Gender = groupedUsers.First().h.Gender,
                                 Position = groupedUsers.First().h.Position,
                                 ManageUserId = groupedUsers.First().h.ManageUserId,
                                 RoleId = groupedUsers.First().ur.RoleId,
                                 UserName = groupedUsers.First().a.UserName,
                                 Status = groupedUsers.First().a.Status,
                                 CreatedDate = groupedUsers.First().a.CreatedDate,
                                 CreatedFullName = groupedUsers.First().a.CreatedFullName,
                                 CreatedUserId = groupedUsers.First().a.CreatedUserId,
                                 UpdatedDate = groupedUsers.First().a.UpdatedDate,
                                 UpdatedFullName = groupedUsers.First().a.UpdatedFullName,
                                 UpdatedUserId = groupedUsers.First().a.UpdatedUserId
                             };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.InvariantCulture);

                if (request.Keyword.IsValidEmail())
                {
                    userQuerys = userQuerys.Where(m => m.Email != null && m.Email.Contains(request.Keyword));
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    userQuerys = userQuerys.Where(m => m.PhoneNumber != null && m.PhoneNumber.Contains(request.Keyword));
                }
                else
                {
                    var queryFullName = userQuerys.Where(m => m.FullName != null && m.FullName.Contains(request.Keyword));
                    var queryUserName = userQuerys.Where(m => m.UserName != null && m.UserName.Contains(request.Keyword));
                    userQuerys = queryFullName.Union(queryUserName);
                }
            }

            if (request.GroupId.HasValue)
            {
                userQuerys = userQuerys.Where(x => x.RoleId == request.GroupId);
            }

            if (request.ManageUserId.HasValue)
            {
                userQuerys = userQuerys.Where(x => x.ManageUserId == request.ManageUserId);
            }

            int totalItem = await userQuerys.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await userQuerys.ApplySortAndPaging(request)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

            var manageUserId = lists.Select(x => x.ManageUserId).Distinct().ToList();
            var manageUsers = await _userManager.Users.WhereBulkContains(manageUserId, x => x.Id).ToListAsync(cancellationToken);

            var roleIds = lists.Select(x => x.RoleId).Distinct().ToList();
            var roles = await _roleManager.Roles.WhereBulkContains(roleIds, x => x.Id).ToListAsync(cancellationToken);

            foreach (var item in lists)
            {
                var manageUser = manageUsers.FirstOrDefault(x => x.Id == item.ManageUserId);
                var role = roles.FirstOrDefault(x => x.Id == item.RoleId);

                item.ManageUser = manageUser?.FullName;
                item.GroupName = role?.Name;
            }

            methodResult.Result = new PagingItemsModel<UserManageModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
