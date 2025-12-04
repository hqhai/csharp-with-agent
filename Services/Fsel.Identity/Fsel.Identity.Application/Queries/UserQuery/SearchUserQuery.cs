// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
        private readonly UserManager<User> _userManager;
        private readonly ITrainingService _trainingService;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;

        public SearchUserQueryHandler(UserManager<User> userManager
            , ITrainingService trainingService
            , ITeacherRepository teacherRepository
            , ICSORepository cSORepository)
        {
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
            var query = _userManager.Users;
            IQueryable<UserSearchModel>? userQuery = default;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.InvariantCulture);
                if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.PhoneNumber == request.Keyword);
                }
                else
                {
                    query = query.Where(m => m.FullName!.Contains(request.Keyword));
                }
            }

            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                userQuery = from u in query
                            join t in _teacherRepository.Queryable on u.Id equals t.UserId
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
                                CreatedDate = u.CreatedDate,
                                Status = !u.Status.HasValue || u.Status == EnumUserStatus.Active,
                            };

                if (request.RoleTeachers != null && request.RoleTeachers.Any(x => x == EnumRoleTeacher.Teacher))
                {
                    userQuery = userQuery.Where(y => y.CourseTypesStr != null);
                }
                if (request.RoleTeachers != null && request.RoleTeachers.Any(x => x == EnumRoleTeacher.TeacherLive))
                {
                    userQuery = userQuery.Where(y => y.LiveCourseTypesStr != null);
                }
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                userQuery = from u in query
                            join cso in _cSORepository.Queryable on u.Id equals cso.UserId
                            where usersByRole.Select(x => x.Id).Contains(u.Id)
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Role = EnumRoleRegisterWithAdmin.CSO,
                                Email = u.Email,
                                CSOId = cso.Id,
                                CreatedDate = u.CreatedDate,
                                Status = !u.Status.HasValue || u.Status == EnumUserStatus.Active,
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                userQuery = from u in query
                            where usersByRole.Select(x => x.Id).Contains(u.Id)
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Role = EnumRoleRegisterWithAdmin.Moderator,
                                Email = u.Email,
                                NumberClass = 0,
                                CreatedDate = u.CreatedDate,
                                Status = !u.Status.HasValue || u.Status == EnumUserStatus.Active,
                            };
            }

            int totalItem = userQuery != null ? await userQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false) : default;
            var lists = userQuery != null ? await userQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false) : default;
            await GetRoles(lists, request.Role);

            methodResult.Result = new PagingItemsModel<UserSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task GetRoles(IList<UserSearchModel>? lists, EnumRoleRegisterWithAdmin? role)
        {
            if (lists != null && lists.Count > 0)
            {
                switch (role)
                {
                    case EnumRoleRegisterWithAdmin.Teacher:
                        var classeResults = await _trainingService.GetUserClassByTeacherIds(lists.Where(x => x.TeacherId != null).Select(x => x.TeacherId ?? default).Distinct().ToList());
                        var classes = classeResults.Content?.Result;
                        foreach (var item in lists)
                        {
                            var user = lists.FirstOrDefault(x => x.Id == item.Id);
                            var teacher = classes?.FirstOrDefault(x => x.Id == user?.TeacherId);
                            var roleTeachers = new List<EnumRoleTeacher>();
                            if (item.LiveCourseTypesStr != null)
                            {
                                roleTeachers.Add(EnumRoleTeacher.TeacherLive);
                            }
                            if (item.CourseTypesStr != null)
                            {
                                roleTeachers.Add(EnumRoleTeacher.Teacher);
                            }
                            item.RoleTeachers = roleTeachers;
                            item.NumberClass = teacher?.TotalClass ?? default;
                        }
                        break;

                    case EnumRoleRegisterWithAdmin.CSO:
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
