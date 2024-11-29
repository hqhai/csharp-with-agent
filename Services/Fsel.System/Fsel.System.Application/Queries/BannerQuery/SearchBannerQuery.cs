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

            var query = await _bannerRepository.Queryable
                                               .Include(x => x.BannerScopes)
                                               .Select(x => new BannerModel
                                               {
                                                   Id = x.Id,
                                                   Name = x.Name,
                                                   Code = x.Code,
                                                   CreatedDate = x.CreatedDate,
                                                   CreatedFullName = x.CreatedFullName,
                                                   CreatedUserId = x.CreatedUserId,
                                                   Content = x.Content,
                                                   FilePath = x.FilePath,
                                                   StartDate = x.StartDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam),
                                                   EndDate = x.EndDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam),
                                                   Type = x.Type,
                                                   UpdatedDate = x.UpdatedDate,
                                                   UpdatedFullName = x.UpdatedFullName,
                                                   UpdatedUserId = x.UpdatedUserId,
                                                   Url = x.Url,
                                                   BannerFrequency = x.BannerFrequency,
                                                   DisplayStartDate = x.DisplayStartDate,
                                                   DisplayEndDate = x.DisplayEndDate,
                                                   DisplayStartTime = x.DisplayStartTime,
                                                   DisplayEndTime = x.DisplayEndTime,
                                                   Status = x.Status,
                                                   BannerScopes = _mapper.Map<IList<BannerScopeModel>>(x.BannerScopes)
                                               }).ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => (x.Name != null && x.Name.ToLower().Trim() == request.Keyword.ToLower().Trim()) || (x.Code != null && x.Code.ToLower().Trim() == request.Keyword.ToLower().Trim())).ToList();
            }

            if (request.CourseLevel.HasValue)
            {
                query = query.Where(x => x.BannerScopes != null && x.BannerScopes.Any(c => c.CourseLevel == request.CourseLevel)).ToList();
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(x => request.StartDate.Value.Date <= x.EndDate.Date && request.EndDate.Value.Date >= x.StartDate.Date).ToList();
            }

            int totalItem = query.Count;
            var lists = query.ApplySortAndPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<BannerModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
