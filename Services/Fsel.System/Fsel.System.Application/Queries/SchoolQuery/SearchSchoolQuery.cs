// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Shared.Enums;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSchoolQuery : SearchSchoolQueryModel, IRequest<MethodResult<PagingItemsModel<SchoolModel>>>
    {
    }

    public class SearchSchoolQueryHandler : IRequestHandler<SearchSchoolQuery, MethodResult<PagingItemsModel<SchoolModel>>>
    {
        private readonly ICrmLocationRepository _crmLocationRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public SearchSchoolQueryHandler(
            ICrmLocationRepository crmLocationRepository,
            IUserService userService,
            AuthContext authContext)
        {
            _crmLocationRepository = crmLocationRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolModel>>> Handle(SearchSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolModel>>();

            var query = _crmLocationRepository.Queryable.Where(p => p.TypeName == EnumCrmLocationTypeName.School);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.GlobalId.ToString() == request.Keyword || (m.Name ?? string.Empty).Trim().Contains(request.Keyword.Trim()));
            }

            var targetRoles = new List<string> { nameof(EnumRole.AdminSchool), nameof(EnumRole.TeacherCampus), nameof(EnumRole.AdminCampus) };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var schoolId = (await _userService.GetSchoolIdAsync()).Content?.Result;
                query = query.Where(m => m.GlobalId == schoolId);
            }

            if (request.LocationId != null)
            {
                var parent = _crmLocationRepository.Queryable.FirstOrDefault(p => p.GlobalId == request.LocationId);
                if (parent == null)
                {
                    return methodResult;
                }
                query = query.Where(m => m.ParentId == parent.Id);
            }

            if (request.EducationLevel != null)
            {
                var educationLevel = (EnumCrmLocationTypeLevel)request.EducationLevel.Value;
                query = query.Where(m => m.TypeLevel == educationLevel);
            }

            var model = query.Select(p => new SchoolModel
            {
                Id = p.GlobalId,
                Name = p.Name,
                LongPath = p.LongPath,
                ShortPath = p.ShortPath,
                IdPath = p.IdPath,
            });

            int totalItem = model.Count();
            var lists = await model
                    .ApplySortAndPaging(request)
                    .OrderBy(x => x.Name)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<SchoolModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
