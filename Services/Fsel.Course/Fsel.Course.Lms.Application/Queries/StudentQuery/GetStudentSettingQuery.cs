// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentSettingQuery : IRequest<MethodResult<StudentSettingModel>>
    {
    }

    public class SettingStudentCheckQueryHandler : IRequestHandler<GetStudentSettingQuery, MethodResult<StudentSettingModel>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public SettingStudentCheckQueryHandler(IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            IOrderService orderService,
            ICourseRepository courseRepository,
            IMapper mapper,
            ITrainingService trainingService,
            AuthContext authContext)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _orderService = orderService;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentSettingModel>> Handle(GetStudentSettingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentSettingModel>();
            var settingStudentModel = new StudentSettingModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            int age = DateTimeHelper.GetYearOld(student?.User?.Birthday);
            settingStudentModel.ExpiredDate = student?.ExpiredDate;
            if (student != null)
            {
                settingStudentModel.NumberOfToken = student.NumberOfToken;
                var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                              .OrderByDescending(x => x.CreatedDate)
                                                                              .ToListAsync(cancellationToken);

                var placementTestResultLast = placementTestResults.FirstOrDefault();

                var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                               .OrderBy(x => x.CreatedDate)
                                                                               .FirstOrDefaultAsync(cancellationToken);

                var (levelNext, isLock) = placementTestResultLast?.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age)) ?? (null, default);

                settingStudentModel.BeginnerGuide = student.BeginnerGuide;
                settingStudentModel.ModuleNumber = placementTestResults.Count + 1;
                settingStudentModel.Level = student.CourseLevel;
                settingStudentModel.BaseCourseLevel = student.BaseCourseLevel;
                settingStudentModel.IsPlacementTest = placementTestResultLast != null;
                settingStudentModel.ClassId = student.ClassId;
                settingStudentModel.PTLevel = placementTestResultLast?.Level;
                settingStudentModel.IsLockPT = isLock;
                settingStudentModel.StartPTLevel = placementTestResultInitial == null ? student.CourseLevel : placementTestResultInitial.Level.GetCourseLevelByPlacementTestLevel();

                var @eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);
                if (@eventResults.IsSuccessStatusCode && @eventResults.Content?.Result != null)
                {
                    var @events = @eventResults.Content?.Result;
                    var actions = @events?.Select(p => p.EventContent).Where(p => p != null && p.Actions != null && p.Actions.Count > 0).SelectMany(p => p.Actions!).ToList();
                    var actionConfigs = @events?.Select(p => p.EventContent).Where(p => p != null && p.ActionConfigs != null && p.ActionConfigs.Count > 0).SelectMany(p => p.ActionConfigs!).ToList();
                    settingStudentModel.Actions = actions;
                    settingStudentModel.ActionConfigs = actionConfigs;
                }

                var status = await _orderService.GetCurrentStatusAsync(_authContext.CurrentUserId);
                if (!status.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError), nameof(status));
                    return methodResult;
                }
                settingStudentModel.Status = status?.Content?.Result;
                var classResult = await _trainingService.GetClassByStudentId(student.Id);
                if (!classResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                    return methodResult;
                }
                var @class = classResult?.Content?.Result;
                if (@class == null)
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = settingStudentModel;
                    return methodResult;
                }
                var course = await _courseRepository.GetByIdAsync(@class.CourseId);
                if (course != null)
                {
                    settingStudentModel.Course = _mapper.Map<CourseModel>(course);
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = settingStudentModel;
            return methodResult;
        }
    }
}
