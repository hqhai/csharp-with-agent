// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.Banners;
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

            var applicableUserDefaults = request.BannerScopes?.Where(x => x.ApplicableUser == EnumApplicableUserGroup.Default).ToList();
            var applicableUserEvents = request.BannerScopes?.Where(x => x.ApplicableUser == EnumApplicableUserGroup.Event).ToList();

            if (request.Type == EnumBannerType.Warning || request.Type == EnumBannerType.Left)
            {
                var bannerQuerys = await _bannerRepository.Queryable
                                                          .Include(x => x.BannerScopes)
                                                          .Where(x => x.Status && x.Type == request.Type && request.StartDate.Date <= x.EndDate.Date && request.EndDate.Date >= x.StartDate.Date)
                                                          .ToListAsync(cancellationToken);

                List<Banner> banners = new List<Banner>();

                if (applicableUserDefaults != null && applicableUserDefaults.Any())
                {
                    var bannerDefaults = bannerQuerys.Where(x => x.BannerScopes.Any(c => !c.CompetitionEventId.HasValue && applicableUserDefaults.Any(p => p.CourseLevel == c.CourseLevel))).ToList();
                    banners.AddRange(bannerDefaults);
                }

                if (applicableUserEvents != null && applicableUserEvents.Any())
                {
                    var bannerEvents = bannerQuerys.Where(x => (x.BannerScopes.Any(c => applicableUserEvents.Any(p => p.CourseLevel == c.CourseLevel && p.CompetitionEventId == c.CompetitionEventId)))).ToList();
                    banners.AddRange(bannerEvents);
                }

                if (bannerId.HasValue)
                {
                    banners = banners.Where(x => x.Id != bannerId).ToList();
                }

                if (banners.Any())
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
                var bannerScopeQuerys = await _bannerScopeRepository.Queryable
                                                                    .Include(x => x.Banner)
                                                                    .Where(x => x.Banner != null && request.StartDate <= x.Banner.EndDate && request.EndDate >= x.Banner.StartDate && x.IsPriority)
                                                                    .ToListAsync(cancellationToken);

                List<BannerScope> priorityBanners = new List<BannerScope>();

                if (applicableUserDefaults != null && applicableUserDefaults.Any())
                {
                    var bannerScopeDefaults = bannerScopeQuerys.Where(x => !x.CompetitionEventId.HasValue && applicableUserDefaults.Any(p => p.IsPriority && p.CourseLevel == x.CourseLevel)).ToList();
                    priorityBanners.AddRange(bannerScopeDefaults);
                }

                if (applicableUserEvents != null && applicableUserEvents.Any())
                {
                    var bannerScopeEvents = bannerScopeQuerys.Where(x => applicableUserEvents.Any(p => p.IsPriority && p.CourseLevel == x.CourseLevel && p.CompetitionEventId == x.CompetitionEventId)).ToList();
                    priorityBanners.AddRange(bannerScopeEvents);
                }

                if (priorityBanners.Any())
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

            if (request.BannerScopes.Any(x => !x.CourseLevel.HasValue))
            {
                return (false, nameof(EnumBannerErrorCode.CourseLevelNotNull), nameof(request.BannerScopes), request.BannerScopes);
            }

            if (request.BannerScopes.Any(x => x.CourseLevel.HasValue && (x.TargetUsers == null || !x.TargetUsers.Any())))
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
