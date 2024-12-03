// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class PreviewBannerByDateQuery : IRequest<MethodResult<IList<BannerModel>>>
    {
        public Guid? Id { get; set; }

        public DateTime Date { get; set; }
    }

    public class PreviewBannerByDateQueryHandler : IRequestHandler<PreviewBannerByDateQuery, MethodResult<IList<BannerModel>>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public PreviewBannerByDateQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<BannerModel>>> Handle(PreviewBannerByDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<BannerModel>> methodResult = new MethodResult<IList<BannerModel>>();

            var banners = await _bannerRepository.Queryable
                                                 .Where(x => x.Status && x.StartDate.Date <= request.Date.Date && x.EndDate.Date >= request.Date.Date)
                                                 .OrderBy(x => x.DisplayStartDate).ToListAsync(cancellationToken);

            if (request.Id.HasValue)
            {
                banners = banners.Where(x => x.Id == request.Id).ToList();
            }

            methodResult.Result = _mapper.Map<IList<BannerModel>>(banners);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
