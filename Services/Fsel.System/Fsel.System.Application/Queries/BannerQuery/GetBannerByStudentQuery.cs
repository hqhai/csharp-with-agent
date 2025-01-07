// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.BannerQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetBannerByStudentQuery : IRequest<MethodResult<IList<BannerStudentModel>>>
    {
        public DateTime Date { get; set; }
    }

    public class GetBannerByStudentQueryHandler : IRequestHandler<GetBannerByStudentQuery, MethodResult<IList<BannerStudentModel>>>
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IBannerStudentRepository _bannerStudentRepository;
        private readonly ICourseService _courseService;

        public GetBannerByStudentQueryHandler(IBannerRepository bannerRepository,
                                              IMapper mapper,
                                              AuthContext authContext,
                                              IUserService userService,
                                              IBannerStudentRepository bannerStudentRepository,
                                              ICourseService courseService)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _bannerStudentRepository = bannerStudentRepository;
            _courseService = courseService;
        }

        public async Task<MethodResult<IList<BannerStudentModel>>> Handle(GetBannerByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<BannerStudentModel>> methodResult = new MethodResult<IList<BannerStudentModel>>();

            var timeOfDay = request.Date.ConvertDateTimeToSeconds();

            var banners = await _bannerRepository.Queryable
                                                 .Include(x => x.BannerScopes)
                                                 .Where(x => (x.StartDate <= request.Date && x.EndDate >= request.Date) && (x.Status) &&
                                                             ((x.DisplayStartDate.HasValue && x.DisplayEndDate.HasValue && x.BannerFrequency == EnumBannerFrequency.Custom) ? (x.DisplayStartDate <= request.Date && x.DisplayEndDate >= request.Date) : (!x.DisplayStartDate.HasValue && !x.DisplayEndDate.HasValue)) &&
                                                             ((x.DisplayStartTime.HasValue && x.DisplayEndTime.HasValue) ? (x.DisplayStartTime <= timeOfDay && x.DisplayEndTime >= timeOfDay) : (!x.DisplayStartTime.HasValue && !x.DisplayEndTime.HasValue)))
                                                 .ToListAsync(cancellationToken);

            var eventResults = await _userService.GetEventsByUserId(_authContext.CurrentUserId);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }
            var eventIds = eventResults.Content?.Result?.Select(x => x.Id).ToList();

            var studentSettingResult = await _courseService.GetStudentSetting();
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

            // banner thuốc course level và dúng trạng thái của user
            banners = banners.Where(x => x.BannerScopes.Any(c => c.CourseLevel == studentSetting.Level && c.TargetUsers != null && c.TargetUsers.Any(p => p == targetUser))).ToList();

            // banner trong event
            if (eventIds != null && eventIds.Any())
            {
                banners = banners.Where(x => x.BannerScopes.Any(c => c.CompetitionEventId.HasValue && eventIds.Contains(c.CompetitionEventId.Value) || (c.ApplicableUser == EnumApplicableUserGroup.Default))).ToList();
            }
            else
            {
                banners = banners.Where(x => x.BannerScopes.Any(c => c.ApplicableUser == EnumApplicableUserGroup.Default)).ToList();
            }

            // bỏ các banner đã hiện thị trong ngày
            var bannerUsedTodays = await _bannerStudentRepository.Queryable.Where(x => x.CreatedDate.Date == request.Date.Date && x.StudentId == studentSetting.StudentId).ToListAsync(cancellationToken);
            var bannerIds = bannerUsedTodays.Select(x => x.BannerId).ToList();

            if (bannerIds != null)
            {
                banners = banners.Where(x => !bannerIds.Contains(x.Id)).ToList();
            }

            // check tần suất hiện thị của banner

            List<Banner> bannerRemoves = new List<Banner>();

            foreach (var item in banners)
            {
                await CheckBannerFrequency(item, request.Date, studentSetting.StudentId, bannerRemoves);
            }

            banners.RemoveAll(c => bannerRemoves.Contains(c));

            // lấy banner được ưu tiên
            var bannerPriority = banners.FirstOrDefault(x => x.Type == EnumBannerType.Popup && x.BannerScopes.Any(c => c.IsPriority));

            var banner = bannerPriority != null ? bannerPriority : banners.FirstOrDefault(x => x.Type == EnumBannerType.Popup);
            var heading = banners.FirstOrDefault(x => x.Type == EnumBannerType.Warning);

            List<BannerStudentModel> bannerStudents = new List<BannerStudentModel>();

            if (banner != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentModel>(banner));
            }

            if (heading != null)
            {
                bannerStudents.Add(_mapper.Map<BannerStudentModel>(heading));
            }

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

            return methodResult;
        }
    }
}
