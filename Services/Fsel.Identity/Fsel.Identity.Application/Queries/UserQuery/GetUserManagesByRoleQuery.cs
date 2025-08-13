// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

    public class GetUserManagesByRoleQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<UserManageModel>>>
    {
        public EnumRole Role { get; set; }

        public Guid? GroupId { get; set; }

        public Guid? ManageUserId { get; set; }
    }

    public class GetUserManagesByRoleQueryHandler : IRequestHandler<GetUserManagesByRoleQuery, MethodResult<PagingItemsModel<UserManageModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly RoleManager<Role> _roleManager;

        public GetUserManagesByRoleQueryHandler(UserManager<User> userManager,
                                                IUserRoleRepository userRoleRepository,
                                                RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<PagingItemsModel<UserManageModel>>> Handle(GetUserManagesByRoleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<UserManageModel>> methodResult = new MethodResult<PagingItemsModel<UserManageModel>>();

            var userQuerys = from a in _userManager.Users
                             join ur in _userRoleRepository.GetQuery() on a.Id equals ur.UserId
                             join r in _roleManager.Roles on ur.RoleId equals r.Id
                             where r.Name == request.Role.ToString()
                             select new UserManageModel
                             {
                                 Id = a.Id,
                                 FullName = a.FullName,
                                 Email = a.Email,
                                 PhoneNumber = a.PhoneNumber,
                                 Birthday = a.Birthday,
                                 Gender = a.Gender,
                                 Position = a.Position,
                                 ManageUserId = a.ManageUserId,
                                 RoleId = ur.RoleId,
                                 GroupName = r.Name,
                                 UserName = a.UserName,
                                 Status = a.Status,
                                 CreatedDate = a.CreatedDate,
                                 CreatedFullName = a.CreatedFullName,
                                 CreatedUserId = a.CreatedUserId,
                                 UpdatedDate = a.UpdatedDate,
                                 UpdatedFullName = a.UpdatedFullName,
                                 UpdatedUserId = a.UpdatedUserId
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

            foreach (var item in lists)
            {
                var manageUser = manageUsers.FirstOrDefault(x => x.Id == item.ManageUserId);

                item.ManageUser = manageUser?.FullName;
            }

            methodResult.Result = new PagingItemsModel<UserManageModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
