// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class BaseIntegrationQuery
    {
        private readonly IOrderService _orderService;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;

        public BaseIntegrationQuery(IOrderService orderService,
                                            IHumanRepository humanRepository,
                                            ILmsCourseService lmsCourseService,
                                            ISystemService systemService,
                                            IMapper mapper,
                                            IUserOtpCodeRepository userOtpCodeRepository,
                                            IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _mapper = mapper;
            _userOtpCodeRepository = userOtpCodeRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<IList<Guid>>> UserInteractedInRange(DateTime startDate, DateTime endDate, bool status, CancellationToken cancellationToken)
        {
            MethodResult<IList<Guid>> methodResult = new MethodResult<IList<Guid>>();

            var courseIntegrationHasTimeQueryModel = new CourseIntegrationQueryModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // lấy Pt có thay đổi trong khoảng thời gian
            var ptTestHasTimes = await _lmsCourseService.GetPalcementTestResults(courseIntegrationHasTimeQueryModel);
            if (!ptTestHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(ptTestHasTimes.Error);
                return methodResult;
            }
            var ptTestResultHasTimes = ptTestHasTimes.Content?.Result;
            if (ptTestResultHasTimes == null)
            {
                methodResult.AddError(ptTestHasTimes.Error);
                return methodResult;
            }
            var userPtTestHasTimeIds = ptTestResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            // lấy unit lesson có thay đổi trong khoảng thời gian
            var unitHasTimes = await _lmsCourseService.GetUnitResults(courseIntegrationHasTimeQueryModel);
            if (!unitHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(unitHasTimes.Error);
                return methodResult;
            }
            var unitResultHasTimes = unitHasTimes.Content?.Result;
            if (unitResultHasTimes == null)
            {
                methodResult.AddError(unitHasTimes.Error);
                return methodResult;
            }
            var userUnitResultHasTimeIds = unitResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            // lấy order có thay đổi trong khoảng thời gian
            var orderHasTimes = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = startDate, EndDate = endDate, Status = status });
            if (!orderHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(orderHasTimes.Error);
                return methodResult;
            }
            var orderResultHasTimes = orderHasTimes.Content?.Result;
            if (orderResultHasTimes == null)
            {
                methodResult.AddError(orderHasTimes.Error);
                return methodResult;
            }
            var userOrderIds = orderResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            //lấy User đăng ký trong khoảng thời gian
            var users = await _humanRepository.Queryable
                                              .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.UpdatedDate.Value >= startDate && x.UpdatedDate.Value <= endDate))
                                              .ToListAsync(cancellationToken);
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }
            var userIdentityHasTimeIds = users.Select(x => x.UserId ?? Guid.Empty).ToList();

            // hợp nhất UserId chưa có order
            var userIds = userIdentityHasTimeIds.Concat(userPtTestHasTimeIds).Concat(userUnitResultHasTimeIds).Concat(userOrderIds).ToList();

            methodResult.Result = userIds.Distinct().ToList();
            return methodResult;
        }

        public void SetUserData(IntegrationModel leadsIntegration, IList<Human> humans, out bool isCutOff)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var human = humans.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            if (human != null && human.User != null && (human.User.EmailConfirmed || human.User.PhoneNumberConfirmed))
            {
                leadsIntegration.Status = EnumIntegrationStatus.Confirm;
            }

            leadsIntegration.SchoolGrade = human?.Student?.SchoolGrade;
            leadsIntegration.SchoolClass = human?.Student?.SchoolClass;
            leadsIntegration.HumanCode = human?.Code;

            isCutOff = human?.User?.Status == EnumUserStatus.Disable;
        }

        public void SetPalcementTestData(IntegrationModel leadsIntegration, IList<PlacementTestResultModel> placementTestResults)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var ptTestResult = placementTestResults?.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            if (ptTestResult != null)
            {
                leadsIntegration.PTLevel = ptTestResult.Level;
                leadsIntegration.PlacementTestResults = ptTestResult.PlacementTestResults;
                leadsIntegration.Status = ptTestResult.Status == "Done" ? EnumIntegrationStatus.PTDone : EnumIntegrationStatus.PTProgress;
            }
        }

        public void SetSchoolData(IntegrationModel leadsIntegration, IList<SchoolModel> schools)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var school = schools.FirstOrDefault(x => x.Id == leadsIntegration.SchoolId);
            leadsIntegration.LongPathSchool = school?.LongPath;
            leadsIntegration.LongPathLocation = school?.Location?.LongPath;
        }

        public void SetFeatureAccessTimeData(IntegrationModel leadsIntegration, IList<GetFeatureAccessTimeQueryModel> featureAccessTimes)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var featureAccessTime = featureAccessTimes.FirstOrDefault(x => x.CreatedUserId == leadsIntegration.UserId);
            leadsIntegration.LastDate = featureAccessTime?.LastVisited;
            leadsIntegration.AccessTime = featureAccessTime?.AccessTime;
        }

        public void SetCourseData(IntegrationModel leadsIntegration, IList<InfoCourseIntegrationModel> infoCourses)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var infoCourseUsers = infoCourses.Where(x => x.UserId == leadsIntegration.UserId).Where(x => x.InfoCourseIntegrationDetails != null).SelectMany(x => x.InfoCourseIntegrationDetails!).ToList();
            leadsIntegration.CourseIntegrations = infoCourseUsers;
        }

        public void SetCourseSuggestData(IntegrationModel leadsIntegration, IList<CourseSuggestUsersModel> courseSuggestUsers)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var courseSuggestUser = courseSuggestUsers.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            leadsIntegration.CourseSuggests = courseSuggestUser?.CourseSuggestConfigs;
        }

        public void SetOtpData(IntegrationModel leadsIntegration, IList<OtpIntegrationModel> userOtps)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var userOtp = userOtps.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            leadsIntegration.OTPEmail = userOtp?.OTPEmail;
            leadsIntegration.OTPPhoneNumber = userOtp?.OTPPhoneNumber;
        }

        public void SetEventData(IntegrationModel leadsIntegration, IList<Human> humans, IList<EventUserIntegrationModel> eventUsers)
        {
            ArgumentNullException.ThrowIfNull(leadsIntegration);

            var human = humans.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            var eventUser = eventUsers.FirstOrDefault(x => x.StudentId == human?.Student?.Id);
            leadsIntegration.EventCode = eventUser?.EventCode;
        }
    }
}
