// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Data;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Users;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchUserQuery : SearchUserQueryModel, IRequest<MethodResult<PagingItemsModel<UserSearchModel>>>
    {
    }

    public class SearchUserQueryHandler : IRequestHandler<SearchUserQuery, MethodResult<PagingItemsModel<UserSearchModel>>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;

        public SearchUserQueryHandler(IHumanRepository humanRepository
            , UserManager<User> userManager
            , ITrainingService trainingService
            , ITeacherRepository teacherRepository
            , ICSORepository cSORepository)
        {
            _humanRepository = humanRepository;
            _userManager = userManager;
            _trainingService = trainingService;
            _teacherRepository = teacherRepository;
            _cSORepository = cSORepository;
        }

        public async Task<MethodResult<PagingItemsModel<UserSearchModel>>> Handle(SearchUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<UserSearchModel>> methodResult = new MethodResult<PagingItemsModel<UserSearchModel>>();

            if (request.PageSize > 100 || request.Role == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var usersByRole = await _userManager.GetUsersInRoleAsync(request.Role.ToString() ?? string.Empty);
            IQueryable<UserSearchModel>? userQuery = default;
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join t in _teacherRepository.Queryable on i.Id equals t.HumanId
                            where usersByRole.Select(x => x.Id).Contains(u.Id)
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Role = EnumRoleRegisterWithAdmin.Teacher,
                                LiveCourseTypesStr = t.LiveCourseTypesStr,
                                CourseTypesStr = t.CourseTypesStr,
                                Email = u.Email,
                                TeacherId = t.Id,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
                if (request.RoleTeachers != null)
                {
                    userQuery = (request.RoleTeachers.Count == 2) ? userQuery.Where(y => y.CourseTypesStr != null && y.LiveCourseTypesStr != null) :
                                (request.RoleTeachers.Any(x => x == EnumRoleTeacher.Teacher)) ? userQuery.Where(y => y.CourseTypesStr != null) :
                                (request.RoleTeachers.Any(x => x == EnumRoleTeacher.TeacherLive)) ? userQuery.Where(y => y.LiveCourseTypesStr != null) : default;
                }
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join cso in _cSORepository.Queryable on i.Id equals cso.HumanId
                            where usersByRole.Select(x => x.Id).Contains(u.Id)
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Role = EnumRoleRegisterWithAdmin.CSO,
                                Email = u.Email,
                                CSOId = cso.Id,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            where usersByRole.Select(x => x.Id).Contains(u.Id)
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Role = EnumRoleRegisterWithAdmin.Moderator,
                                Email = u.Email,
                                NumberClass = 0,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                userQuery = userQuery?.Where(m => m.Id == request.Keyword || m.FullName!.Contains(request.Keyword));
            }

            int totalItem = userQuery != null ? await userQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false) : default;
            var lists = userQuery != null ? await userQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false) : default;
            await GetRoles(lists, request.Role, request.RoleTeachers);

            methodResult.Result = new PagingItemsModel<UserSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task GetRoles(IList<UserSearchModel>? lists, EnumRoleRegisterWithAdmin? role, IList<EnumRoleTeacher>? teacherRoles)
        {
            if (lists != null && lists.Count > 0)
            {
                switch (true)
                {
                    case var solutionOne when solutionOne == (role == EnumRoleRegisterWithAdmin.Teacher):
                        var classeResults = await _trainingService.GetUserClassByTeacherIds(lists.Where(x => x.TeacherId != null).Select(x => x.TeacherId ?? default).Distinct().ToList());
                        var classes = classeResults.Content?.Result;
                        foreach (var item in lists)
                        {
                            var user = lists.FirstOrDefault(x => x.Id == item.Id);
                            var teacher = classes?.FirstOrDefault(x => x.Id == user?.TeacherId);
                            item.RoleTeachers =
                                 (teacherRoles?.Count == 2) ? new List<EnumRoleTeacher> { EnumRoleTeacher.Teacher, EnumRoleTeacher.TeacherLive } :
                                 (teacherRoles?.Any(x => x == EnumRoleTeacher.Teacher) ?? default) ? new List<EnumRoleTeacher> { EnumRoleTeacher.Teacher } :
                                 (teacherRoles?.Any(x => x == EnumRoleTeacher.TeacherLive) ?? default) ? new List<EnumRoleTeacher> { EnumRoleTeacher.TeacherLive } :
                                 (teacherRoles == null ? (
                                 (item.LiveCourseTypesStr != null && item.CourseTypesStr != null) ? new List<EnumRoleTeacher> { EnumRoleTeacher.Teacher, EnumRoleTeacher.TeacherLive } :
                                 (item.LiveCourseTypesStr != null) ? new List<EnumRoleTeacher> { EnumRoleTeacher.TeacherLive } :
                                 (item.CourseTypesStr != null) ? new List<EnumRoleTeacher> { EnumRoleTeacher.Teacher } : default) : default);
                            item.NumberClass = teacher?.TotalClass ?? default;
                        }
                        break;

                    case var solutionOne when solutionOne == (role == EnumRoleRegisterWithAdmin.CSO):
                        var classeCsoResults = await _trainingService.GetUserClassByTeacherIds(lists.Where(x => x.CSOId != null).Select(x => x.TeacherId ?? default).Distinct().ToList());
                        var classeCsos = classeCsoResults.Content?.Result;
                        foreach (var item in lists)
                        {
                            var user = lists.FirstOrDefault(x => x.Id == item.Id);
                            var teacher = classeCsos?.FirstOrDefault(x => x.Id == user?.CSOId);
                            item.NumberClass = teacher?.TotalClass ?? default;
                        }
                        break;
                }
            }
        }
    }
}
