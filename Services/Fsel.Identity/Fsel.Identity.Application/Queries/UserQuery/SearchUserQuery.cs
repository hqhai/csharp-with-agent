// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
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
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;

        public SearchUserQueryHandler(IHumanRepository humanRepository
            , UserManager<User> userManager
            , ITeacherRepository teacherRepository
            , ICSORepository cSORepository)
        {
            _humanRepository = humanRepository;
            _userManager = userManager;
            _teacherRepository = teacherRepository;
            _cSORepository = cSORepository;
        }

        public async Task<MethodResult<PagingItemsModel<UserSearchModel>>> Handle(SearchUserQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<UserSearchModel>> methodResult = new MethodResult<PagingItemsModel<UserSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            IQueryable<UserSearchModel>? userQuery = default;
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join t in _teacherRepository.Queryable on i.Id equals t.HumanId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                Status = u.LockoutEnabled,
                                FullName = u.FullName,
                                CreatedDate = i.CreatedDate,
                                Human = new HumanSearchModel
                                {
                                    Id = i.Id,
                                    Birthday = i.Birthday,
                                    Email = i.Email,
                                    Teacher = new TeacherModel
                                    {
                                        Id = i.Id,
                                        CertificationPath = t.CertificationPath,
                                        CourseLevels = t.CourseLevels,
                                        CourseTypes = t.CourseTypes,
                                        PassportPath = t.PassportPath,
                                        PoliceClearancePath = t.PoliceClearancePath,
                                        UniversityDegreePath = t.UniversityDegreePath
                                    }
                                }
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join cso in _cSORepository.Queryable on i.Id equals cso.HumanId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                Status = u.LockoutEnabled,
                                FullName = u.FullName,
                                CreatedDate = i.CreatedDate,
                                Human = new HumanSearchModel
                                {
                                    Id = i.Id,
                                    Birthday = i.Birthday,
                                    Email = i.Email,
                                    CSO = new CSOModel
                                    {
                                        Id = i.Id,
                                        CertificationPath = cso.CertificationPath,
                                        CourseLevels = cso.CourseLevels,
                                        CourseTypes = cso.CourseTypes,
                                        PassportPath = cso.PassportPath,
                                        PoliceClearancePath = cso.PoliceClearancePath,
                                        UniversityDegreePath = cso.UniversityDegreePath,
                                    }
                                }
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                Status = u.LockoutEnabled,
                                FullName = u.FullName,
                                CreatedDate = i.CreatedDate,
                                Human = new HumanSearchModel
                                {
                                    Id = i.Id,
                                    Birthday = i.Birthday,
                                    Email = i.Email
                                }
                            };
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                userQuery = userQuery!.Where(m => m.Id == request.Keyword || m.FullName!.Contains(request.Keyword));
            }

            int totalItem = await userQuery!.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await userQuery!
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<UserSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
