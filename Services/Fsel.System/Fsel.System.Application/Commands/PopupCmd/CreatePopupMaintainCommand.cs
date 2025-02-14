// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.PopupCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.MaintainConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreatePopupMaintainCommand : CreatePopupMaintainCommandModel, IRequest<MethodResult<PopupMaintainModel>>
    {
    }

    public class CreatePopupMaintainCommandHandler : IRequestHandler<CreatePopupMaintainCommand, MethodResult<PopupMaintainModel>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public CreatePopupMaintainCommandHandler(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PopupMaintainModel>> Handle(CreatePopupMaintainCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PopupMaintainModel>();

            #region Validation
            (bool isValid, string errorCode, string field, object? value) = await ValidateBanner(request);
            if (!isValid)
            {
                methodResult.AddErrorBadRequest(errorCode, field, value);
                return methodResult;
            }
            #endregion Validation

            var banner = _mapper.Map<Banner>(request);
            await _bannerRepository.ExecuteTransactionAsync(async () =>
            {
                banner.Code = request.Code;
                banner.Name = request.Name;
                banner.StartDate = request.StartDate;
                banner.EndDate = request.EndDate;
                banner.Type = EnumBannerType.Warning;
                banner.DisplayStartDate = request.StartDate.AddHours(-request.NotificationTime);
                _bannerRepository.Add(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PopupMaintainModel>(banner);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<(bool, string, string, object?)> ValidateBanner(CreatePopupMaintainCommand request)
        {
            bool condition = true;
            string errorMessage = string.Empty;
            string field = string.Empty;
            object? value = default;

            if (request.StartDate >= request.EndDate)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate);
                field = nameof(request.StartDate);
                value = request.StartDate;
            }

            if (request.StartDate <= DateTime.UtcNow)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.StartDateLowerThanDateTimeNow);
                field = nameof(request.StartDate);
                value = request.StartDate;
            }

            return (condition, errorMessage, field, value);
        }

    }
}
