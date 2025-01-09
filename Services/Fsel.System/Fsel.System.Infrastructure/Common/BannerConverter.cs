// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.EntityFrameworkCore;

    public class BannerConverter
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IBannerScopeRepository _bannerScopeRepository;

        public BannerConverter(IBannerRepository bannerRepository,
                               IBannerScopeRepository bannerScopeRepository)
        {
            _bannerRepository = bannerRepository;
            _bannerScopeRepository = bannerScopeRepository;
        }

        public async Task<VoidMethodResult> BannerScopeHandler(CreateBannerCommandModel request, Guid? bannerId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.Type == EnumBannerType.Warning || request.Type == EnumBannerType.Left)
            {
                var courseLevels = request.BannerScopes!.Where(x => x.CourseLevel.HasValue).Select(x => x.CourseLevel).ToList();
                var competitionEventIds = request.BannerScopes!.Where(x => x.CompetitionEventId.HasValue).Select(x => x.CompetitionEventId).ToList();

                var banners = await _bannerRepository.Queryable
                                                    .Include(x => x.BannerScopes)
                                                    .Where(x => x.Status && x.Type == request.Type && request.StartDate.Date <= x.EndDate.Date && request.EndDate.Date >= x.StartDate.Date &&
                                                                (x.BannerScopes.Any(c => courseLevels.Contains(c.CourseLevel) || competitionEventIds.Contains(c.CompetitionEventId))))
                                                    .ToListAsync(cancellationToken);

                if (bannerId.HasValue)
                {
                    banners = banners.Where(x => x.Id != bannerId).ToList();
                }

                if (banners != null && banners.Any())
                {
                    foreach (var banner in banners)
                    {
                        banner.Status = false;
                    }

                    _bannerRepository.UpdateList(banners);
                }
            }

            if (request.Type == EnumBannerType.Popup)
            {
                var courseLevels = request.BannerScopes!.Where(x => x.IsPriority).Select(x => x.CourseLevel).ToList();
                var priorityBanners = await _bannerScopeRepository.Queryable
                                                                  .Include(x => x.Banner)
                                                                  .Where(x => x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate)
                                                                  .Where(x => (courseLevels.Contains(x.CourseLevel) && x.IsPriority))
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

        public (bool, string, string, object?) ValidateBanner(CreateBannerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.StartDate >= request.EndDate)
            {
                return (false, nameof(EnumBannerErrorCode.StartDateGreaterThanEndDate), nameof(request.StartDate), request.StartDate);
            }

            if (request.Type != EnumBannerType.Warning && (request.BannerImages == null || !request.BannerImages.Any()))
            {
                return (false, nameof(EnumBannerErrorCode.BannerImageNotNull), nameof(EnumBannerType.Popup), request.BannerImages);
            }

            if (request.Type == EnumBannerType.Warning && string.IsNullOrEmpty(request.Content))
            {
                return (false, nameof(EnumBannerErrorCode.ContentNotNull), nameof(EnumBannerType.Warning), request.Content);
            }

            if (request.Type != EnumBannerType.Warning && request.BannerImages != null && request.BannerImages.Any(x => string.IsNullOrEmpty(x.FilePath)))
            {
                return (false, nameof(EnumBannerErrorCode.FilePathNotNull), nameof(request.BannerImages), request.BannerImages);
            }

            if (request.Type != EnumBannerType.Warning && request.BannerImages != null && request.BannerImages.Any(x => x.RouteScreen == EnumBannerLink.Custom && string.IsNullOrEmpty(x.Url)))
            {
                return (false, nameof(EnumBannerErrorCode.UrlNotNull), nameof(request.BannerImages), request.BannerImages);
            }

            if (request.BannerScopes == null || !request.BannerScopes.Any())
            {
                return (false, nameof(EnumBannerErrorCode.BannerScopeNotNull), nameof(request.BannerScopes), request.BannerScopes);
            }

            if (request.BannerFrequency == EnumBannerFrequency.Custom && (request.DisplayDates == null || !request.DisplayDates.Any()))
            {
                return (false, nameof(EnumBannerErrorCode.DisplayDatesNotNull), nameof(request.DisplayDates), request.DisplayDates);
            }

            if (request.DisplayDates != null && request.DisplayDates.Any(x => x.Date < request.StartDate.Date || x.Date > request.EndDate.Date))
            {
                return (false, nameof(EnumBannerErrorCode.DisplayDateOutOfRange), nameof(request.DisplayDates), request.DisplayDates);
            }

            if (request.DisplayStartTime.HasValue && request.DisplayEndTime.HasValue && request.DisplayStartTime >= request.DisplayEndTime)
            {
                return (false, nameof(EnumBannerErrorCode.DisplayStartTimeGreaterThanDisplayEndTime), nameof(request.DisplayStartTime), request.DisplayStartTime);
            }

            if (request.BannerScopes.Any(x => x.ApplicableUser == EnumApplicableUserGroup.Default && !x.CourseLevel.HasValue))
            {
                return (false, nameof(EnumBannerErrorCode.CourseLevelNotNull), nameof(request.BannerScopes), request.BannerScopes);
            }

            if (request.BannerScopes.Any(x => x.ApplicableUser == EnumApplicableUserGroup.Default && x.CourseLevel.HasValue && (x.TargetUsers == null || !x.TargetUsers.Any())))
            {
                return (false, nameof(EnumBannerErrorCode.TargetUserNotNull), nameof(request.BannerScopes), request.BannerScopes);
            }

            if (request.BannerScopes.Any(x => x.ApplicableUser == EnumApplicableUserGroup.Event && !x.CompetitionEventId.HasValue))
            {
                return (false, nameof(EnumBannerErrorCode.CompetitionEventIdNotNull), nameof(request.BannerScopes), request.BannerScopes);
            }

            return (true, string.Empty, string.Empty, default);
        }
    }
}
