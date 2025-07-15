// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBannerByStudentQuery : IRequest<MethodResult<IList<BannerStudentQueueModel>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetBannerByStudentQueryHandler : IRequestHandler<GetBannerByStudentQuery, MethodResult<IList<BannerStudentQueueModel>>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly ICourseService _courseService;
        private readonly BannerPublisher _bannerPublisher;
        private readonly IBannerSettingRepository _bannerSettingRepository;
        private readonly IBannerScopeRepository _bannerScopeRepository;
        private readonly IBannerImageRepository _bannerImageRepository;

        public GetBannerByStudentQueryHandler(IBannerRepository bannerRepository,
                                              IMapper mapper,
                                              IUserService userService,
                                              IBannerStudentRepository bannerStudentRepository,
                                              ICourseService courseService,
                                              BannerPublisher bannerPublisher,
                                              IBannerSettingRepository bannerSettingRepository,
                                              IBannerScopeRepository bannerScopeRepository,
                                              IBannerImageRepository bannerImageRepository)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _userService = userService;
            _bannerStudentRepository = bannerStudentRepository;
            _courseService = courseService;
            _bannerPublisher = bannerPublisher;
            _bannerSettingRepository = bannerSettingRepository;
            _bannerScopeRepository = bannerScopeRepository;
            _bannerImageRepository = bannerImageRepository;
        }

        public async Task<MethodResult<IList<BannerStudentQueueModel>>> Handle(GetBannerByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<BannerStudentQueueModel>> methodResult = new MethodResult<IList<BannerStudentQueueModel>>();


            DateTime date = DateTime.UtcNow;
            DateTime dateVietNam = date.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var timeOfDay = dateVietNam.ConvertDateTimeToSeconds();

            var bannerScopes = _bannerScopeRepository.Queryable;
            var bannerQueries = _bannerRepository.Queryable;

            // lấy student
            var studentSettingResult = await _courseService.GetStudentSetting(request.UserId);
            if (!studentSettingResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentSettingResult.Error);
                return methodResult;
            }
            var studentSetting = studentSettingResult.Content?.Result;
            if (studentSetting == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentSetting));
                return methodResult;
            }

            // lấy banner đã hiện trong ngày
            var bannerUsedTodays = await _bannerStudentRepository.Queryable.Where(x => x.Banner != null && x.Banner.Type == EnumBannerType.Popup && x.CreatedDate.Date == date.Date && x.StudentId == studentSetting.StudentId).ToListAsync(cancellationToken);
            var bannerIds = bannerUsedTodays.Select(x => x.BannerId).ToList();

            // lấy setting banner
            var bannerSetting = await _bannerSettingRepository.Queryable.FirstOrDefaultAsync(cancellationToken);

            // check số banner đã hiện trong ngày
            if (bannerSetting != null && bannerUsedTodays.Count >= bannerSetting.MaximumPerDay)
            {
                methodResult.AddErrorBadRequest(nameof(EnumBannerErrorCode.MaximumBannerPerDay), nameof(bannerSetting.MaximumPerDay), bannerSetting.MaximumPerDay);
                return methodResult;
            }

            // check khoảng cách xuất hiện giữa các banner
            var lastBannerToday = bannerUsedTodays.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (lastBannerToday != null && bannerSetting != null && lastBannerToday.CreatedDate.AddMinutes(bannerSetting.DisplayIntervalTime) > DateTime.UtcNow)
            {
                bannerQueries = bannerQueries.Where(x => x.Type != EnumBannerType.Popup);
            }

            // lấy event của user
            var eventResults = await _userService.GetEventsByUserId(request.UserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var eventIds = eventResults.Content?.Result?.Select(x => x.Id).ToList();

            // banner trong event
            if (eventIds != null && eventIds.Any())
            {
                bannerScopes = bannerScopes.Where(c => c.CompetitionEventId.HasValue && eventIds.Contains(c.CompetitionEventId.Value));
            }
            else
            {
                bannerScopes = bannerScopes.Where(c => c.ApplicableUser == EnumApplicableUserGroup.Default);
            }

            // banner thuộc course level
            bannerScopes = bannerScopes.Where(x => x.CourseLevel == studentSetting.Level);

            var banners = await (from a in bannerScopes
                                 join b in bannerQueries on a.BannerId equals b.Id
                                 where (b.StartDate <= date && b.EndDate >= date) && (b.Status) &&
                                       ((b.DisplayStartTime.HasValue && b.DisplayEndTime.HasValue) ? (b.DisplayStartTime <= timeOfDay && b.DisplayEndTime >= timeOfDay) : (!b.DisplayStartTime.HasValue && !b.DisplayEndTime.HasValue) &&
                                       // bỏ các banner đã hiện thị trong ngày
                                       (bannerIds != null && !bannerIds.Contains(b.Id)))
                                 select new
                                 {
                                     Banner = b,
                                     BannerScope = a
                                 }).ToListAsync(cancellationToken);

            // trạng thái của student
            EnumTargetUser? targetUser = null;
            if (studentSetting.Status == EnumTrialRegistrationStatus.Trial || studentSetting.Status == EnumTrialRegistrationStatus.Finished)
            {
                targetUser = EnumTargetUser.Trial;
            }
            else if (studentSetting.Status == EnumTrialRegistrationStatus.Payment)
            {
                targetUser = EnumTargetUser.InProgress;
            }
            else if (studentSetting.Status == EnumTrialRegistrationStatus.Expired)
            {
                targetUser = EnumTargetUser.Expired;
            }

            // đúng trạng thái của user
            banners = banners.Where(x => x.BannerScope.TargetUsers != null && x.BannerScope.TargetUsers.Any(p => p == targetUser)).ToList();

            // check tần suất hiện thị của banner
            List<Banner> bannerRemoves = new List<Banner>();

            foreach (var item in banners)
            {
                await CheckBannerFrequency(item.Banner, date, studentSetting.StudentId, bannerRemoves);
            }

            banners.RemoveAll(c => bannerRemoves.Contains(c.Banner));

            // lấy banner được ưu tiên
            var bannerPriority = banners.FirstOrDefault(x => x.Banner.Type == EnumBannerType.Popup && x.BannerScope.IsPriority && x.BannerScope.CourseLevel == studentSetting.Level && x.BannerScope.TargetUsers != null && x.BannerScope.TargetUsers.Any(p => p == targetUser));

            // lấy dữ liệu
            var banner = bannerPriority != null ? bannerPriority : banners.FirstOrDefault(x => x.Banner.Type == EnumBannerType.Popup);
            var heading = banners.FirstOrDefault(x => x.Banner.Type == EnumBannerType.Warning);
            var homes = banners.Where(x => x.Banner.Type == EnumBannerType.Home).ToList();
            var left = banners.FirstOrDefault(x => x.Banner.Type == EnumBannerType.Left);

            // banner image
            var bannerResultIds = new List<Guid> { banner?.Banner.Id ?? Guid.Empty, heading?.Banner.Id ?? Guid.Empty, left?.Banner.Id ?? Guid.Empty };
            bannerResultIds.AddRange(homes.Select(x => x.Banner.Id));

            var bannerImages = await _bannerImageRepository.Queryable
                                                           .Include(x => x.Banner)
                                                           .WhereBulkContains(bannerResultIds, x => x.BannerId).ToListAsync(cancellationToken);

            // gán dữ liệu
            List<BannerStudentQueueModel> bannerStudents = new List<BannerStudentQueueModel>();

            if (banner != null)
            {
                BannerStudentQueueModel bannerStudentQueue = new BannerStudentQueueModel();
                var bannerImageBanners = bannerImages.Where(x => x.BannerId == banner.Banner.Id).ToList();
                _mapper.Map(bannerImageBanners.FirstOrDefault()?.Banner, bannerStudentQueue);
                bannerStudents.Add(bannerStudentQueue);
            }

            if (heading != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentQueueModel>(heading.Banner));
            }

            if (homes != null)
            {
                var bannerImageHomes = bannerImages.Where(x => homes.Any(c => c.Banner.Id == x.BannerId)).Select(x => x.Banner).DistinctBy(x => x.Id).ToList();
                foreach (var bannerImage in bannerImageHomes)
                {
                    BannerStudentQueueModel bannerStudentQueue = new BannerStudentQueueModel();
                    _mapper.Map(bannerImage, bannerStudentQueue);
                    bannerStudents.Add(bannerStudentQueue);
                }
            }

            if (left != null)
            {
                BannerStudentQueueModel bannerStudentQueue = new BannerStudentQueueModel();
                var bannerImageLefts = bannerImages.Where(x => x.BannerId == left.Banner.Id).ToList();
                _mapper.Map(bannerImageLefts.FirstOrDefault()?.Banner, bannerStudentQueue);
                bannerStudents.Add(bannerStudentQueue);
            }

            // bắn web socket
            await _bannerPublisher.Publish(new BannerStudentsQueueModel { UserId = request.UserId, TimeSlideShow = bannerSetting?.TimeSlideShow ?? default, BannerStudents = bannerStudents }, cancellationToken);

            methodResult.Result = bannerStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VoidMethodResult> CheckBannerFrequency(Banner banner, DateTime date, Guid studentId, IList<Banner> banners)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            if (banner.BannerFrequency == EnumBannerFrequency.OneTimeOnly)
            {
                var oneTimeOnly = await _bannerStudentRepository.Queryable.AnyAsync(x => x.BannerId == banner.Id && x.StudentId == studentId);
                if (oneTimeOnly)
                {
                    banners.Add(banner);
                }
            }
            else if (banner.BannerFrequency == EnumBannerFrequency.AlternateDays)
            {
                DateTime yesterday = date.AddDays(-1);
                var alternateDay = await _bannerStudentRepository.Queryable.AnyAsync(x => x.BannerId == banner.Id && x.CreatedDate.Date == yesterday.Date && x.StudentId == studentId);
                if (alternateDay)
                {
                    banners.Add(banner);
                }
            }
            else if (banner.BannerFrequency == EnumBannerFrequency.WeekendsOnly)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    banners.Add(banner);
                }
            }
            else if (banner.BannerFrequency == EnumBannerFrequency.WeekdaysOnly)
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    banners.Add(banner);
                }
            }
            else if (banner.BannerFrequency == EnumBannerFrequency.Custom && banner.DisplayDates != null && !banner.DisplayDates.Any(x => x.Date == date.Date))
            {
                banners.Add(banner);
            }

            return methodResult;
        }
    }
}
