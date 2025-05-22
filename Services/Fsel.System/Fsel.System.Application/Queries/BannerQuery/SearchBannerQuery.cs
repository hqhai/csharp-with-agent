// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Domain.Models.QueryModels.Banners;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchBannerQuery : SearchBannerQueryModel, IRequest<MethodResult<PagingItemsModel<BannerModel>>>
    {
    }

    public class SearchBannerQueryHandler : IRequestHandler<SearchBannerQuery, MethodResult<PagingItemsModel<BannerModel>>>
    {
        private readonly IBannerRepository _bannerRepository;

        public SearchBannerQueryHandler(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        public async Task<MethodResult<PagingItemsModel<BannerModel>>> Handle(SearchBannerQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<BannerModel>> methodResult = new MethodResult<PagingItemsModel<BannerModel>>();
            ArgumentNullException.ThrowIfNull(request);

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _bannerRepository.Queryable
                                         .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Trim().Contains(request.Keyword.ToLower().Trim())) || (x.Code != null && x.Code.ToLower().Trim().Contains(request.Keyword.ToLower().Trim())));
            }

            if (request.ListCourseLevels != null && request.ListCourseLevels.Any())
            {
                query = query.Where(x => x.BannerScopes != null && x.BannerScopes.Any(c => c.CourseLevel.HasValue && request.ListCourseLevels.Contains(c.CourseLevel.Value)));
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(x => request.StartDate.Value.Date <= x.EndDate.Date && request.EndDate.Value.Date >= x.StartDate.Date);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(x => x.Type == request.Type);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status);
            }

            return await _bannerRepository.GetListByPageResultAsync<BannerModel>(query, request, cancellationToken);
        }
    }
}
