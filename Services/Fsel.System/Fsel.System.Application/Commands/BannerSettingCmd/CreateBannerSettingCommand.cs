// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerSettingCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.BannerSettings;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateBannerSettingCommand : CreateBannerSettingCommandModel, IRequest<MethodResult<BannerSettingModel>>
    {
    }

    public class CreateBannerSettingCommandHandler : IRequestHandler<CreateBannerSettingCommand, MethodResult<BannerSettingModel>>
    {
        private readonly IBannerSettingRepository _bannerSettingRepository;
        private readonly IMapper _mapper;

        public CreateBannerSettingCommandHandler(IBannerSettingRepository bannerSettingRepository, IMapper mapper)
        {
            _bannerSettingRepository = bannerSettingRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BannerSettingModel>> Handle(CreateBannerSettingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<BannerSettingModel> methodResult = new MethodResult<BannerSettingModel>();

            if (request.MaximumPerDay < 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.MaximumPerDayMustGreaterThanZero), nameof(request.MaximumPerDay), request.MaximumPerDay);
                return methodResult;
            }

            if (request.DisplayIntervalTime < 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.DisplayIntervalTimeMustGreaterThanZero), nameof(request.DisplayIntervalTime), request.DisplayIntervalTime);
                return methodResult;
            }

            await _bannerSettingRepository.ExecuteTransactionAsync(async () =>
            {
                var bannerSetting = await _bannerSettingRepository.Queryable.FirstOrDefaultAsync();
                if (bannerSetting != null)
                {
                    bannerSetting.MaximumPerDay = request.MaximumPerDay;
                    bannerSetting.DisplayIntervalTime = request.DisplayIntervalTime;
                    _bannerSettingRepository.Update(bannerSetting);
                    methodResult.Result = _mapper.Map<BannerSettingModel>(bannerSetting);
                }
                else
                {
                    var command = _mapper.Map<BannerSetting>(request);
                    _bannerSettingRepository.Add(command);
                    methodResult.Result = _mapper.Map<BannerSettingModel>(command);
                }

                await _bannerSettingRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
