// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using Fsel.Common.ActionResults;
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
            var query = _bannerRepository.Queryable.Select(x => new BannerModel
            {
                Name = x.Name,
                Code = x.Code,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                Description = x.Description,
                EndDate = x.EndDate,
                FilePath = x.FilePath,
                Id = x.Id,
                StartDate = x.StartDate,
                Status = x.Status,
                Type = x.Type,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                Url = x.Url
            });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(x => x.Name == request.Keyword);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<BannerModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
