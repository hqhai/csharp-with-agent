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

        public GetBannerByStudentQueryHandler(IBannerRepository bannerRepository,
                                              IMapper mapper,
                                              IUserService userService,
                                              IBannerStudentRepository bannerStudentRepository,
                                              ICourseService courseService,
                                              BannerPublisher bannerPublisher,
                                              IBannerSettingRepository bannerSettingRepository)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _userService = userService;
            _bannerStudentRepository = bannerStudentRepository;
            _courseService = courseService;
            _bannerPublisher = bannerPublisher;
            _bannerSettingRepository = bannerSettingRepository;
        }

        public async Task<MethodResult<IList<BannerStudentQueueModel>>> Handle(GetBannerByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<BannerStudentQueueModel>> methodResult = new MethodResult<IList<BannerStudentQueueModel>>();

            DateTime date = DateTime.UtcNow;

            DateTime dateVietNam = date.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var timeOfDay = dateVietNam.ConvertDateTimeToSeconds();

            var banners = await _bannerRepository.Queryable
                                                 .Include(x => x.BannerScopes)
                                                 .Include(x => x.BannerImages)
                                                 .Where(x => (x.StartDate <= date && x.EndDate >= date) && (x.Status) &&
                                                             ((x.DisplayStartTime.HasValue && x.DisplayEndTime.HasValue) ? (x.DisplayStartTime <= timeOfDay && x.DisplayEndTime >= timeOfDay) : (!x.DisplayStartTime.HasValue && !x.DisplayEndTime.HasValue)))
                                                 .ToListAsync(cancellationToken);

            var eventResults = await _userService.GetEventsByUserId(request.UserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var eventIds = eventResults.Content?.Result?.Select(x => x.Id).ToList();

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

            // banner thuộc course level và đúng trạng thái của user
            banners = banners.Where(x => x.BannerScopes.Any(c => c.CourseLevel == studentSetting.Level && c.TargetUsers != null && c.TargetUsers.Any(p => p == targetUser))).ToList();

            // banner trong event
            if (eventIds != null && eventIds.Any())
            {
                banners = banners.Where(x => x.BannerScopes.Any(c => c.CompetitionEventId.HasValue && eventIds.Contains(c.CompetitionEventId.Value))).ToList();
            }
            else
            {
                banners = banners.Where(x => x.BannerScopes.Any(c => c.ApplicableUser == EnumApplicableUserGroup.Default)).ToList();
            }

            // bỏ các banner đã hiện thị trong ngày
            var bannerUsedTodays = await _bannerStudentRepository.Queryable.Where(x => x.CreatedDate.Date == date.Date && x.StudentId == studentSetting.StudentId).ToListAsync(cancellationToken);
            var bannerIds = bannerUsedTodays.Select(x => x.BannerId).ToList();

            if (bannerIds != null)
            {
                banners = banners.Where(x => !bannerIds.Contains(x.Id)).ToList();
            }

            // check tần suất hiện thị của banner
            List<Banner> bannerRemoves = new List<Banner>();

            foreach (var item in banners)
            {
                await CheckBannerFrequency(item, date, studentSetting.StudentId, bannerRemoves);
            }

            banners.RemoveAll(c => bannerRemoves.Contains(c));

            // lấy banner được ưu tiên
            var bannerPriority = banners.FirstOrDefault(x => x.Type == EnumBannerType.Popup && x.BannerScopes.Any(c => c.IsPriority && c.CourseLevel == studentSetting.Level && c.TargetUsers != null && c.TargetUsers.Any(p => p == targetUser)));

            // lấy dữ liệu
            var banner = bannerPriority != null ? bannerPriority : banners.FirstOrDefault(x => x.Type == EnumBannerType.Popup);
            var heading = banners.FirstOrDefault(x => x.Type == EnumBannerType.Warning);
            var homes = banners.Where(x => x.Type == EnumBannerType.Home).ToList();
            var left = banners.FirstOrDefault(x => x.Type == EnumBannerType.Left);

            // gán dữ liệu
            List<BannerStudentQueueModel> bannerStudents = new List<BannerStudentQueueModel>();

            if (banner != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentQueueModel>(banner));
            }

            if (heading != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentQueueModel>(heading));
            }

            if (homes != null)
            {
                bannerStudents.AddRange(_mapper.Map<IList<BannerStudentQueueModel>>(homes));
            }

            if (left != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentQueueModel>(left));
            }

            // lấy time slide show
            var bannerSetting = await _bannerSettingRepository.Queryable.FirstOrDefaultAsync(cancellationToken);

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
