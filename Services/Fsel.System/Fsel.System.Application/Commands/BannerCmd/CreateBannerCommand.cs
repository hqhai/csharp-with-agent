
// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.Common;
    using global::System;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateBannerCommand : CreateBannerCommandModel, IRequest<MethodResult<BannerModel>>
    {
    }

    public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly BannerConverter _bannerConverter;

        public CreateBannerCommandHandler(IBannerRepository bannerRepository,
                                          IMapper mapper,
                                          BannerConverter bannerConverter)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _bannerConverter = bannerConverter;
        }

        public async Task<MethodResult<BannerModel>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            #region Validation
            if (await _bannerRepository.Queryable.AnyAsync(x => x.Code.ToLower().Trim() == request.Code.ToLower().Trim(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.CodeAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            (bool isValid, string errorCode, string field, object? value) = _bannerConverter.ValidateBanner(request);
            if (!isValid)
            {
                methodResult.AddErrorBadRequest(errorCode, field, value);
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
                await _bannerConverter.BannerScopeHandler(request, null, cancellationToken);
                banner.StartDate = banner.StartDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                banner.EndDate = banner.EndDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                banner.Status = true;

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
