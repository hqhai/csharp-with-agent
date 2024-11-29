
// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.BannerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Fsel.System.Domain.Models.EntityModels;
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
        private readonly IBannerScopeRepository _bannerScopeRepository;

        public CreateBannerCommandHandler(IBannerRepository bannerRepository, IMapper mapper, IBannerScopeRepository bannerScopeRepository)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _bannerScopeRepository = bannerScopeRepository;
        }

        public async Task<MethodResult<BannerModel>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BannerModel>();

            #region Validation
            (bool isValid, string errorCode, string field, object? value) = await ValidateBanner(request);
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
                await BannerScopeHandler(request, cancellationToken);
                banner.StartDate = banner.StartDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                banner.EndDate = banner.EndDate.ConvertTimeToUtc(EnumCountryKey.Vietnam);
                banner.Status = true;
                banner.BannerScopes = _mapper.Map<IList<BannerScope>>(request.BannerScopes);

                _bannerRepository.Add(banner);
                await _bannerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<BannerModel>(banner);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<VoidMethodResult> BannerScopeHandler(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.Type == EnumBannerType.Warning)
            {
                var contentBanner = await _bannerRepository.Queryable.FirstOrDefaultAsync(x => x.Type == EnumBannerType.Warning && request.StartDate.Date <= x.EndDate.Date && request.EndDate.Date >= x.StartDate.Date, cancellationToken);
                if (contentBanner != null)
                {
                    contentBanner.Status = false;
                    _bannerRepository.Update(contentBanner);
                }
            }
            else
            {
                var courseLevels = request.BannerScopes.Where(x => x.IsPriority).Select(x => x.CourseLevel).ToList();
                var priorityBanners = await _bannerScopeRepository.Queryable
                                                                  .Include(x => x.Banner)
                                                                  .Where(x => x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate)
                                                                  .Where(x => courseLevels.Contains(x.CourseLevel) && x.IsPriority)
                                                                  .ToListAsync(cancellationToken);

                if (priorityBanners != null && priorityBanners.Any())
                {
                    foreach (var priorityBanner in priorityBanners)
                    {
                        priorityBanner.IsPriority = false;
                    }

                    _bannerScopeRepository.UpdateList(priorityBanners);
                    await _bannerScopeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return methodResult;
        }

        private async Task<(bool, string, string, object?)> ValidateBanner(CreateBannerCommand request)
        {
            bool condition = true;
            string errorMessage = string.Empty;
            string field = string.Empty;
            object? value = default;

            if (await _bannerRepository.Queryable.AnyAsync(x => x.Code.ToLower().Trim() == request.Code.ToLower().Trim()))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.CodeAlreadyExist);
                field = nameof(request.Code);
                value = request.Code;
            }

            if (request.StartDate >= request.EndDate)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate);
                field = nameof(request.StartDate);
                value = request.StartDate;
            }

            if (request.Type == EnumBannerType.Popup && string.IsNullOrEmpty(request.FilePath))
            {
                condition = false;
                errorMessage = nameof(EnumSystemErrorCode.DataNotExist);
                field = nameof(EnumBannerType.Popup);
                value = request.FilePath;
            }
            else if (request.Type == EnumBannerType.Warning && string.IsNullOrEmpty(request.Content))
            {
                condition = false;
                errorMessage = nameof(EnumSystemErrorCode.DataNotExist);
                field = nameof(EnumBannerType.Warning);
                value = request.Content;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && (request.DisplayStartDate >= request.DisplayEndDate))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.DisplayStartDateGreaterThanDisplayEndDate);
                field = nameof(request.DisplayStartDate);
                value = request.DisplayStartDate;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && !request.DisplayStartDate.HasValue)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.CustomFrequencyRequiresDisplayDate);
                field = nameof(request.DisplayStartDate);
                value = request.DisplayStartDate;
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && !request.DisplayEndDate.HasValue)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.CustomFrequencyRequiresDisplayDate);
                field = nameof(request.DisplayStartDate);
                value = request.DisplayStartDate;
            }

            if (request.DisplayStartTime.HasValue && request.DisplayEndTime.HasValue && request.DisplayStartTime >= request.DisplayEndTime)
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.DisplayStartTimeGreaterThanDisplayEndTime);
                field = nameof(request.DisplayStartTime);
                value = request.DisplayStartTime;
            }

            if (request.BannerScopes.GroupBy(x => x.CourseLevel).Any(x => x.Count() > 1))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.EachBannerOnlyOneCourseLevel);
                field = nameof(request.BannerScopes);
                value = request.BannerScopes;
            }

            if (request.BannerScopes.Any(x => x.ApplicableUserGroups != null && x.ApplicableUserGroups.Any(c => c == EnumApplicableUserGroup.Event) && x.CompetitionEventIds == null))
            {
                condition = false;
                errorMessage = nameof(EnumBannerErrorCode.CompetitionEventIdsRequired);
                field = nameof(request.BannerScopes);
                value = request.BannerScopes;
            }

            return (condition, errorMessage, field, value);
        }
    }
}
