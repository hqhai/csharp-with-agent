// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.CommandModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentSettingQuery : IRequest<MethodResult<StudentSettingModel>>
    {
        public Guid? UserId { get; set; }
    }

    public class SettingStudentCheckQueryHandler : IRequestHandler<GetStudentSettingQuery, MethodResult<StudentSettingModel>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly IInteractionService _interactionService;
        private readonly AppSetting _appSetting;

        public SettingStudentCheckQueryHandler(IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            IOrderService orderService,
            ICourseRepository courseRepository,
            IMapper mapper,
            AuthContext authContext,
            IInteractionService interactionService,
            AppSetting appSetting)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _authContext = authContext;
            _interactionService = interactionService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<StudentSettingModel>> Handle(GetStudentSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentSettingModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId ?? _authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }
            var settingStudentModel = new StudentSettingModel
            {
                StudentId = student.Id,
                ExpiredDate = student.ExpiredDate,
                NumberOfToken = student.NumberOfToken,
                BeginnerGuide = student.BeginnerGuide,
                Level = student.CourseLevel,
                BaseCourseLevel = student.BaseCourseLevel,
                ClassId = student.ClassId,
                EmailConfirmed = student.User?.EmailConfirmed ?? default,
                TurnOnTouchpoint = _appSetting.TouchpointConfig?.TurnOnTouchpoint ?? false,
                UserStatus = student?.User?.Status
            };
            var role = _authContext.Roles?.FirstOrDefault();

            if (!string.IsNullOrEmpty(role) && role == EnumRole.StudentCampus.ToString())
            {
                settingStudentModel.IsLockPT = true;
                settingStudentModel.IsPlacementTest = true;
            }
            else
            {
                await GetPlacementTestAsync(settingStudentModel, student, cancellationToken);
            }

            var @eventResults = await _userService.GetEventByUserId(request.UserId ?? _authContext.CurrentUserId);

            var requestCheckSurvey = new CheckSurveyBySurveyFormTypeModel()
            {
                SurveyFormType = EnumSurveyFormType.Default
            };

            if (@eventResults.IsSuccessStatusCode && @eventResults.Content?.Result != null && @eventResults.Content.Result.Any())
            {
                var @events = @eventResults.Content?.Result;
                var actions = @events?.Select(p => p.EventContent).Where(p => p != null && p.Actions != null && p.Actions.Count > 0).SelectMany(p => p.Actions!).ToList();
                var actionConfigs = @events?.Select(p => p.EventContent).Where(p => p != null && p.ActionConfigs != null && p.ActionConfigs.Count > 0).SelectMany(p => p.ActionConfigs!).ToList();
                settingStudentModel.Actions = actions;
                settingStudentModel.ActionConfigs = actionConfigs;
                settingStudentModel.IsActivedAccount = DateTime.UtcNow >= (@events?.FirstOrDefault()?.EventContent?.StartDate ?? default);

                requestCheckSurvey.SurveyFormType = EnumSurveyFormType.Event;
                requestCheckSurvey.CompetitionEventId = events?.FirstOrDefault()?.Id;
                settingStudentModel.CompetitionEventId = events?.FirstOrDefault()?.Id;
            }

            var status = await _orderService.GetCurrentStatusAsync(request.UserId ?? _authContext.CurrentUserId);
            if (!status.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(status));
                return methodResult;
            }

            settingStudentModel.Status = status?.Content?.Result;
            if (student.CourseId.HasValue)
            {
                var course = await _courseRepository.GetByIdAsync(student.CourseId.Value);
                settingStudentModel.Course = _mapper.Map<CourseModel>(course);

                if (course != null)
                {
                    requestCheckSurvey.CourseLevel = course.CourseLevel;
                    requestCheckSurvey.CourseType = course.CourseType;

                    var surveyEvent = await _interactionService.CheckSurveyPT(requestCheckSurvey);
                    if (surveyEvent.IsSuccessStatusCode)
                    {
                        settingStudentModel.IsSurveyEvent = surveyEvent.Content?.Result ?? false;
                    }
                }
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = settingStudentModel;
            return methodResult;
        }

        private async Task GetPlacementTestAsync(StudentSettingModel settingStudentModel, StudentModel student, CancellationToken cancellationToken)
        {
            int age = DateTimeHelper.GetYearOld(student.User?.Birthday);
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                            .OrderByDescending(x => x.CreatedDate)
                                                                            .ToListAsync(cancellationToken);
            var placementTestResultLast = placementTestResults.FirstOrDefault();
            var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                           .OrderBy(x => x.CreatedDate)
                                                                           .FirstOrDefaultAsync(cancellationToken);
            var (levelNext, isLock) = placementTestResultLast?.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age)) ?? (null, default);
            settingStudentModel.StartPTLevel = placementTestResultInitial == null ? student.CourseLevel : placementTestResultInitial.Level.GetCourseLevelByPlacementTestLevel();
            settingStudentModel.ModuleNumber = placementTestResults.Count + 1;
            settingStudentModel.IsPlacementTest = placementTestResultLast != null;
            settingStudentModel.PTLevel = placementTestResultLast?.Level;
            settingStudentModel.IsLockPT = isLock;
        }
    }
}
