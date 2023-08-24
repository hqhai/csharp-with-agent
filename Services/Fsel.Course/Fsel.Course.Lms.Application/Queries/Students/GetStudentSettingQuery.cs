// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Students
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly AuthContext _authContext;

        public SettingStudentCheckQueryHandler(IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            AuthContext authContext)
        {
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
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
            if (student != null)
            {
                var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == student.Id)
                                                                                .OrderByDescending(x => x.CreatedDate)
                                                                                .FirstOrDefaultAsync(cancellationToken);
                settingStudentModel.Level = student.CourseLevel;
                settingStudentModel.IsPlacementTest = placementTestResult != null;
                settingStudentModel.ClassId = student.ClassId ?? null;
                settingStudentModel.PTLevel = placementTestResult?.Level ?? null;
                settingStudentModel.PTNextLevel = placementTestResult?.Level.GetLevelInScore(placementTestResult.Percent) ?? null;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = settingStudentModel;
            return methodResult;
        }
    }
}
