// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
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
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ICSORepository _cSORepository;

        public SearchUserQueryHandler(IMapper mapper, IHumanRepository humanRepository
            , UserManager<User> userManager
            , ITeacherRepository teacherRepository
            , ICSORepository cSORepository)
        {
            _mapper = mapper;
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
            IQueryable<UserSearchModel>? humanQuery = default;
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                humanQuery = from i in _humanRepository.Queryable
                             join u in _userManager.Users on i.UserId equals u.Id
                             join t in _teacherRepository.Queryable on i.Id equals t.HumanId
                             select new UserSearchModel
                             {
                                 Id = i.Id,
                                 CreatedDate = i.CreatedDate,
                                 Status = u.LockoutEnabled,
                                 Human = new HumanSearchModel
                                 {
                                     FullName = i.FullName,
                                     Birthday = i.Birthday,
                                     Email = i.Email,
                                     Teacher = _mapper.Map<TeacherModel>(t)
                                 }
                             };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                humanQuery = from i in _humanRepository.Queryable
                             join u in _userManager.Users on i.UserId equals u.Id
                             join cso in _cSORepository.Queryable on i.Id equals cso.HumanId
                             select new UserSearchModel
                             {
                                 Id = i.Id,
                                 CreatedDate = i.CreatedDate,
                                 Status = u.LockoutEnabled,
                                 Human = new HumanSearchModel
                                 {
                                     FullName = i.FullName,
                                     Birthday = i.Birthday,
                                     Email = i.Email,
                                     CSO = _mapper.Map<CSOModel>(cso)
                                 }
                             };
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.Moderator)
            {
                humanQuery = from i in _humanRepository.Queryable
                             join u in _userManager.Users on i.UserId equals u.Id
                             select new UserSearchModel
                             {
                                 Id = i.Id,
                                 CreatedDate = i.CreatedDate,
                                 Status = u.LockoutEnabled,
                                 Human = new HumanSearchModel
                                 {
                                     FullName = i.FullName,
                                     Birthday = i.Birthday,
                                     Email = i.Email
                                 }
                             };
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                humanQuery = humanQuery!.Where(m => m!.Id!.ToString() == request.Keyword || m.Human != null && m.Human.FullName == request.Keyword);
            }

            int totalItem = await humanQuery!.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await humanQuery!.OrderByDescending(x => x.CreatedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<UserSearchModel>
            {
                Items = _mapper.Map<IEnumerable<UserSearchModel>>(lists),
                PagingInfo = new PagingInfoModel { Page = request.Page, PageSize = request.PageSize, TotalItems = totalItem }
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
