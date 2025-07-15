// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Models;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Domain.Models.QueryModels.UserGroup;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.UserGroupQuery
{
    public class GetUserGroupsQuery : GetUserGroupQueryModel, IRequest<MethodResult<PagingItemsModel<UserGroupModel>>>
    {
    }

    public class GetUserGroupsQueryHandler : IRequestHandler<GetUserGroupsQuery, MethodResult<PagingItemsModel<UserGroupModel>>>
    {
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IMapper _mapper;

        public GetUserGroupsQueryHandler(IUserGroupRepository userGroupRepository, IMapper mapper)
        {
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<UserGroupModel>>> Handle(GetUserGroupsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<UserGroupModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            // Build query based on search parameters
            var query = _userGroupRepository.Queryable;

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                string keyword = request.Keyword.ToLower();
                query = query.Where(x =>
                    (x.GroupName != null && x.GroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword))
                );
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            // Default ordering
            request.SortBy ??= new List<GenericSortModel>();

            if (!request.SortBy.Any())
            {
                request.SortBy.Add(new GenericSortModel
                {
                    Property = nameof(UserGroup.DisplayOrder),
                    IsDesc = false
                });
            }

            // Get total count
            int totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting and paging
            var list = await query
                .ApplySortAndPaging(request)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to model
            var userGroupModels = _mapper.Map<List<UserGroupModel>>(list);

            // Build result
            methodResult.Result = new PagingItemsModel<UserGroupModel>(userGroupModels, request, totalCount);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
