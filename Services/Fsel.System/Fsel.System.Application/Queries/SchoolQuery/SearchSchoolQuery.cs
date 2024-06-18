// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.SchoolQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels;
    using global::System;
    using global::System.Linq;
    using global::System.Threading.Tasks;
    using MediatR;

    public class SearchSchoolQuery : SearchSchoolQueryModel, IRequest<MethodResult<PagingItemsModel<SchoolModel>>>
    {
    }

    public class SearchSchoolQueryHandler : IRequestHandler<SearchSchoolQuery, MethodResult<PagingItemsModel<SchoolModel>>>
    {
        private readonly ISchoolRepository _schoolRepository;

        public SearchSchoolQueryHandler(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolModel>>> Handle(SearchSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var query = _schoolRepository.Queryable;

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()));
            }

            if (request.LocationId != null)
            {
                query = query.Where(m => m.LocationId == request.LocationId);
            }

            if (request.EducationLevel != null)
            {
                query = query.Where(m => m.EducationLevel == request.EducationLevel);
            }

            var methodResult = await _schoolRepository.GetListByPageResultAsync<SchoolModel>(query, request, cancellationToken);
            return methodResult;
        }
    }
}
