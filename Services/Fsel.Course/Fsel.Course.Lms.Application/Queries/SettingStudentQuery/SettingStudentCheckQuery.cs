// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.SettingStudentQuery
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

    public class SettingStudentCheckQuery : IRequest<MethodResult<SettingStudentModel>>
    {
    }

    public class SettingStudentCheckQueryHandler : IRequestHandler<SettingStudentCheckQuery, MethodResult<SettingStudentModel>>
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

        public async Task<MethodResult<SettingStudentModel>> Handle(SettingStudentCheckQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<SettingStudentModel> methodResult = new MethodResult<SettingStudentModel>();
            SettingStudentModel settingStudentModel = new SettingStudentModel();
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
                settingStudentModel.LevelNext = placementTestResult?.Level.GetLevelInScore(placementTestResult.Percent) ?? default;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = settingStudentModel;
            return methodResult;
        }
    }
}
