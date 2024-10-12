// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateBannerCommand : CreateBannerCommandModel, IRequest<MethodResult<BannerModel>>
    {
    }

    public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public CreateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<BannerModel>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            #region Validation

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

            var banner = _mapper.Map<Banner>(request);
            if (!banner.IsValid())
            {
                methodResult.AddErrorBadRequest(banner.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                _bannerRepository.Add(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<BannerModel>(banner);
                return methodResult;
            });

            return methodResult;
        }
    }
}
