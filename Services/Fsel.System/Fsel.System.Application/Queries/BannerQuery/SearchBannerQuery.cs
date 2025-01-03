// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
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
        private readonly IMapper _mapper;

        public SearchBannerQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
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
                                         .Include(x => x.BannerScopes)
                                         .AsQueryable();

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Trim().Contains(request.Keyword.ToLower().Trim())) || (x.Code != null && x.Code.ToLower().Trim().Contains(request.Keyword.ToLower().Trim())));
            }

            if (request.ListCourseLevels != null && request.ListCourseLevels.Any())
            {
                query = query.Where(x => x.BannerScopes != null && x.BannerScopes.Any(c => request.ListCourseLevels.Contains(c.CourseLevel)));
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(x => request.StartDate.Value.Date <= x.EndDate.Date && request.EndDate.Value.Date >= x.StartDate.Date);
            }

            int totalItem = await query.CountAsync(cancellationToken);
            var lists = await query.ApplySortAndPaging(request).ToListAsync(cancellationToken);

            methodResult.Result = new PagingItemsModel<BannerModel>(_mapper.Map<IList<BannerModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
