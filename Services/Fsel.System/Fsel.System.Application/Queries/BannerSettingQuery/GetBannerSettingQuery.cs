// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerSettingQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBannerSettingQuery : IRequest<MethodResult<BannerSettingModel>>
    {
    }

    public class GetBannerSettingQueryHandler : IRequestHandler<GetBannerSettingQuery, MethodResult<BannerSettingModel>>
    {
        private readonly IBannerSettingRepository _bannerSettingRepository;
        private readonly IMapper _mapper;

        public GetBannerSettingQueryHandler(IBannerSettingRepository bannerSettingRepository, IMapper mapper)
        {
            _bannerSettingRepository = bannerSettingRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BannerSettingModel>> Handle(GetBannerSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<BannerSettingModel> methodResult = new MethodResult<BannerSettingModel>();

            var bannerSetting = await _bannerSettingRepository.Queryable.FirstOrDefaultAsync(cancellationToken);
            if (bannerSetting == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(bannerSetting));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<BannerSettingModel>(bannerSetting);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
