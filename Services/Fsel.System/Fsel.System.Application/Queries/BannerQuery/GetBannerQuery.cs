// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetBannerQuery : IRequest<MethodResult<BannerModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetBannerQueryHandler : IRequestHandler<GetBannerQuery, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public GetBannerQueryHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BannerModel>> Handle(GetBannerQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<BannerModel> methodResult = new MethodResult<BannerModel>();
            var banner = await _bannerRepository.GetByIdAsync(request.Id);
            methodResult.Result = _mapper.Map<BannerModel>(banner);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
