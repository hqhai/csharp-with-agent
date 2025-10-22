// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery.Classes
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchClassBySchoolQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SchoolClassModel>>>
    {
        public Guid? SchoolId { get; set; }
        public string? SchoolIdStr { get; set; }
    }

    public class SearchClassBySchoolQueryHandler : IRequestHandler<SearchClassBySchoolQuery, MethodResult<PagingItemsModel<SchoolClassModel>>>
    {
        private readonly ISchoolClassRepository _schoolClassRepository;

        public SearchClassBySchoolQueryHandler(ISchoolClassRepository schoolClassRepository)
        {
            _schoolClassRepository = schoolClassRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolClassModel>>> Handle(SearchClassBySchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolClassModel>>();

            var schoolIds = request.SchoolIdStr.ToList<Guid>();

            var queryClass = _schoolClassRepository.Queryable;
            if (request.SchoolId.HasValue)
            {
                queryClass = queryClass.Where(x => x.SchoolId == request.SchoolId);
            }
            if (schoolIds != null && schoolIds.Any())
            {
                queryClass = queryClass.WhereBulkContains(schoolIds, x => x.SchoolId);
            }
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                queryClass = queryClass.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword));
            }

            var query = queryClass.Select(p => new SchoolClassModel()
            {
                Id = p.Id,
                CreatedUserId = p.CreatedUserId,
                Name = p.Name,
                TeacherId = p.TeacherId,
                CreatedDate = p.CreatedDate
            });

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<SchoolClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
