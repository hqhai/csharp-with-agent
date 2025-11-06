// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery.Classes
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
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

    public class SearchTeacherCampusQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TeacherModel>>>
    {
    }

    public class SearchTeacherCampusQueryHandler : IRequestHandler<SearchTeacherCampusQuery, MethodResult<PagingItemsModel<TeacherModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;
        private readonly AuthContext _authContext;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IHumanRepository _humanRepository;

        public SearchTeacherCampusQueryHandler(UserManager<User> userManager, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository, IUserSchoolRepository userSchoolRepository, AuthContext authContext, ITeacherRepository teacherRepository, IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
            _userSchoolRepository = userSchoolRepository;
            _authContext = authContext;
            _teacherRepository = teacherRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<PagingItemsModel<TeacherModel>>> Handle(SearchTeacherCampusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<TeacherModel>>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var query = await (from u in _userManager.Users
                               join h in _humanRepository.Queryable on u.Id equals h.UserId
                               join t in _teacherRepository.Queryable on h.Id equals t.HumanId
                               join ur in _userRoleRepository.Queryable on u.Id equals ur.UserId
                               join r in _roleManager.Roles on ur.RoleId equals r.Id
                               join us in _userSchoolRepository.Queryable on u.Id equals us.UserId
                               where us.SchoolId == schoolId && r.Name == EnumRole.TeacherCampus.ToString()
                               select new TeacherModel()
                               {
                                   Id = u.Id,
                                   CreatedDate = t.CreatedDate,
                                   Human = new HumanModel()
                                   {
                                       FullName = u.FullName,
                                       AvatarPath = h.AvatarPath,
                                   }
                               }).ToListAsync(cancellationToken);

            int totalItem = query.Count;
            var lists = query.ApplySortAndPaging(request)
                                        .ToList();

            methodResult.Result = new PagingItemsModel<TeacherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
