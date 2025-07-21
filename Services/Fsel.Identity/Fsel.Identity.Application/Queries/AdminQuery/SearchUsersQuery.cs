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
        private readonly IUserGroupMemberShipRepository _userGroupMemberShipRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;

        public SearchUsersQueryHandler(UserManager<User> userManager, IUserGroupMemberShipRepository userGroupMemberShipRepository, IUserGroupRepository userGroupRepository, IPlatformRepository platformRepository, IUserPlatformRepository userPlatformRepository)
        {
            _userManager = userManager;
            _userGroupMemberShipRepository = userGroupMemberShipRepository;
            _userGroupRepository = userGroupRepository;
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
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
                             join grm in _userGroupMemberShipRepository.Queryable on a.Id equals grm.UserId into grmGroup
                             from grm in grmGroup.DefaultIfEmpty()
                             join up in _userPlatformRepository.Queryable on a.Id equals up.UserId
                             where up.PlatformId == platform.Id
                             group new { a, grm, up } by a.Id into groupedUsers
                             select new UserManageModel
                             {
                                 Id = groupedUsers.Key,
                                 FullName = groupedUsers.First().a.FullName,
                                 Email = groupedUsers.First().a.Email,
                                 PhoneNumber = groupedUsers.First().a.PhoneNumber,
                                 Birthday = groupedUsers.First().a.Birthday,
                                 Gender = groupedUsers.First().a.Gender,
                                 Position = groupedUsers.First().a.Position,
                                 ManageUserId = groupedUsers.First().a.ManageUserId,
                                 GroupId = groupedUsers.First().grm.GroupId,
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
                userQuerys = userQuerys.Where(x => x.GroupId == request.GroupId);
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

            var groupIds = lists.Select(x => x.GroupId).Distinct().ToList();
            var groups = await _userGroupRepository.Queryable.WhereBulkContains(groupIds, x => x.Id).ToListAsync(cancellationToken);

            foreach (var item in lists)
            {
                var manageUser = manageUsers.FirstOrDefault(x => x.Id == item.ManageUserId);
                var group = groups.FirstOrDefault(x => x.Id == item.GroupId);

                item.ManageUser = manageUser?.FullName;
                item.GroupName = group?.GroupName;
            }

            methodResult.Result = new PagingItemsModel<UserManageModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
