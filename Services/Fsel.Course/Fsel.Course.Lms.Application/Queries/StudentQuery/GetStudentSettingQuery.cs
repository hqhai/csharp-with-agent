// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
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
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public SettingStudentCheckQueryHandler(IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            IOrderService orderService,
            ITrainingService trainingService,
            AuthContext authContext)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _orderService = orderService;
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
            int age = DateTimeHelper.GetYearOld(student?.Human?.Birthday);
            if (student != null)
            {
                var classResult = await _trainingService.GetClassByStudentId(student.Id);
                if (!classResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                    return methodResult;
                }
                var @class = classResult?.Content?.Result;
                if (@class == null)
                {
                    var isLockOrder = await _orderService.IsCheckStatusUser(new IsCheckPaymentStatusByUserModel { ClassId = @class.Id, CourseId = @class.CourseId, PackageId = @class.PackageId, UserId = _authContext.CurrentUserId });
                    if (!isLockOrder.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isLockOrder));
                        return methodResult;
                    }
                    settingStudentModel.IsLockOrder = isLockOrder?.Content?.Result ?? default;
                }

                var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                                .OrderByDescending(x => x.CreatedDate)
                                                                                .FirstOrDefaultAsync(cancellationToken);
                var (levelNext, isLock) = placementTestResult?.Level.GetLevelInScore(placementTestResult.Percent, age) ?? (null, default);

                settingStudentModel.Level = student.CourseLevel;
                settingStudentModel.IsPlacementTest = placementTestResult != null;
                settingStudentModel.ClassId = student.ClassId ?? null;
                settingStudentModel.PTLevel = placementTestResult?.Level ?? null;
                settingStudentModel.IsLockPT = isLock;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = settingStudentModel;
            return methodResult;
        }
    }
}
