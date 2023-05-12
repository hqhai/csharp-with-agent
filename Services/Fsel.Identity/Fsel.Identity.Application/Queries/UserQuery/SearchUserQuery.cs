// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Linq;
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
            var usersByRole = await _userManager.GetUsersInRoleAsync(request.Role.ToString() ?? string.Empty);
            IQueryable<UserSearchModel>? userQuery = null;

            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join t in _teacherRepository.Queryable on i.Id equals t.HumanId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Email = u.Email,
                                Role = request.Role.ToString(),
                                NumberClass = 0,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.TeacherLive)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            join t in _teacherRepository.Queryable on i.Id equals t.HumanId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Email = u.Email,
                                Role = request.Role.ToString(),
                                NumberClass = 0,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
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
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Email = u.Email,
                                Role = request.Role.ToString(),
                                NumberClass = 0,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                userQuery = from u in _userManager.Users
                            join i in _humanRepository.Queryable on u.Id equals i.UserId
                            select new UserSearchModel
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                PhoneNumber = u.PhoneNumber,
                                Email = u.Email,
                                Role = request.Role.ToString(),
                                NumberClass = 0,
                                CreatedDate = i.CreatedDate,
                                Status = u.LockoutEnabled,
                            };
            }

            userQuery = userQuery!.Where(m => usersByRole.Select(x => x.Id).Contains(m.Id));

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
