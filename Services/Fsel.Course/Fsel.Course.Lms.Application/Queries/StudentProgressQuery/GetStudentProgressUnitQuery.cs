// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressUnitQuery : IRequest<MethodResult<UnitStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetStudentProgressUnitQueryHandler : IRequestHandler<GetStudentProgressUnitQuery, MethodResult<UnitStudentProgressModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;

        public GetStudentProgressUnitQueryHandler(ICourseRepository courseRepository, ManagerProgressHelper managerProgressHelper, IUnitRepository unitRepository, IUserService userService, ISystemService systemService)
        {
            _courseRepository = courseRepository;
            _managerProgressHelper = managerProgressHelper;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
        }

        public async Task<MethodResult<UnitStudentProgressModel>> Handle(GetStudentProgressUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UnitStudentProgressModel> methodResult = new MethodResult<UnitStudentProgressModel>();
            var studentResults = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var userId = student?.UserId ?? default;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unitProgress = await _managerProgressHelper.GetUnitManager(course.Id, unit.Id, student.Id);
            if (unitProgress == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel { CourseId = request.CourseId, UnitId = request.UnitId, UserId = userId });
            var featureAccessTime = featureAccessTimeResult?.Content?.Result;
            if (featureAccessTime != null)
            {
                unitProgress.TimeSpent = featureAccessTime.AccessTime;
                unitProgress.LastVisited = featureAccessTime.LastVisited ?? null;
            }
            methodResult.Result = unitProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
