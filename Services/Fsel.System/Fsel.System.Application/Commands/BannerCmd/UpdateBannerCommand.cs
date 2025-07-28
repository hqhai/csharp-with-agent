// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.BannerImages;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.CommandModels.BannerScopes;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateBannerCommand : CreateBannerCommandModel, IRequest<MethodResult<BannerModel>>
    {
        public Guid Id { get; set; }
    }

    public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, MethodResult<BannerModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly BannerConverter _bannerConverter;
        private readonly IBannerScopeRepository _bannerScopeRepository;
        private readonly IBannerImageRepository _bannerImageRepository;

        public UpdateBannerCommandHandler(IBannerRepository bannerRepository,
                                          IMapper mapper,
                                          BannerConverter bannerConverter,
                                          IBannerScopeRepository bannerScopeRepository,
                                          IBannerImageRepository bannerImageRepository)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _bannerConverter = bannerConverter;
            _bannerScopeRepository = bannerScopeRepository;
            _bannerImageRepository = bannerImageRepository;
        }

        public async Task<MethodResult<BannerModel>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            var banner = await _bannerRepository.Queryable
                                                .Include(x => x.BannerScopes)
                                                .Include(x => x.BannerImages)
                                                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (banner == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(banner));
                return methodResult;
            }

            bool checkBanner = (request.BannerScopes == null || !request.BannerScopes.Any()) && request.Status != banner.Status;

            if (await _bannerRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Code.ToLower().Trim() == request.Code.ToLower().Trim(), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.CodeAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }

            (bool isValid, string errorCode, string field, object? value) = _bannerConverter.ValidateBanner(request);
            if (!isValid && !checkBanner)
            {
                methodResult.AddErrorBadRequest(nameof(errorCode), nameof(field), errorCode);
                return methodResult;
            }

            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                if (checkBanner)
                {
                    banner.Status = request.Status;
                    _bannerRepository.Update(banner);
                    await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = _mapper.Map<BannerModel>(banner);
                    return methodResult;
                }

                var bannerScopes = banner.BannerScopes.ToList();
                var bannerImages = banner.BannerImages.ToList();

                await _bannerImageRepository.DeleteListAsync(bannerImages);
                await _bannerScopeRepository.DeleteListAsync(bannerScopes);

                _mapper.Map(request, banner);
                banner.BannerScopes = _mapper.Map<IList<BannerScope>>(request.BannerScopes);
                banner.BannerImages = _mapper.Map<IList<BannerImage>>(request.BannerImages);
                if (!banner.IsValid())
                {
                    methodResult.AddErrorBadRequest(banner.ErrorMessages);
                    return methodResult;
                }

                banner.StartDate = banner.StartDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                banner.EndDate = banner.EndDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                await _bannerConverter.BannerScopeHandler(request, request.Id, cancellationToken);

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
