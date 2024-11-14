// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateBannerCommand : UpdateBannerCommandModel, IRequest<MethodResult<BannerModel>>
    {
    }

    public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public UpdateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BannerModel>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            if (request.StartDate >= request.EndDate)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate));
                return methodResult;
            }
            if (request.Type == EnumBannerType.Popup && string.IsNullOrEmpty(request.FilePath))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumBannerType.Popup), request.FilePath);
                return methodResult;
            }
            else if (request.Type == EnumBannerType.Warning && string.IsNullOrEmpty(request.Description))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumBannerType.Warning), request.Description);
                return methodResult;
            }

            var banner = await _bannerRepository.GetByIdAsync(request.Id);
            if (banner == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(banner));
                return methodResult;
            }
            _mapper.Map(request, banner);
            if (!banner.IsValid())
            {
                methodResult.AddErrorBadRequest(banner.ErrorMessages);
                return methodResult;
            }

            banner.StartDate = banner.StartDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
            banner.EndDate = banner.EndDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                _bannerRepository.Update(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<BannerModel>(banner);
                return methodResult;
            });

            return methodResult;
        }
    }
}
