// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
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
        public Guid? ModuleId { get; set; }
    }

    public class GetStudentProgressUnitQueryHandler : IRequestHandler<GetStudentProgressUnitQuery, MethodResult<UnitStudentProgressModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILearningService _learningService;

        public GetStudentProgressUnitQueryHandler(ICourseRepository courseRepository, ManagerProgressHelper managerProgressHelper, IUnitRepository unitRepository, IUserService userService, ISystemService systemService, ICourseResultRepository courseResultRepository, ILearningService learningService)
        {
            _courseRepository = courseRepository;
            _managerProgressHelper = managerProgressHelper;
            _unitRepository = unitRepository;
            _userService = userService;
            _systemService = systemService;
            _courseResultRepository = courseResultRepository;
            _learningService = learningService;
        }

        public async Task<MethodResult<UnitStudentProgressModel>> Handle(GetStudentProgressUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UnitStudentProgressModel>();

            var studentResults = await _userService.GetUserByStudentIdWithCache(request.StudentId);
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

            var userId = student.UserId;

            var learningTree = await _learningService.GetLearningTreeFromCourseToTest(
            student.Id,
            request.CourseId,
            request.ModuleId,
            cancellationToken);

            if (learningTree == null)
            {
                return methodResult;
            }

            var units = learningTree
               .GetAllItemByType<UnitComponentModel>().OrderBy(p => p.DisplayOrder)
               .ToList();

            var unit = units.FirstOrDefault(p => p.LearningTemplateId == request.UnitId);

            if (unit == null)
            {
                return methodResult;
            }

            var featureAccessTimeUnitResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
            {
                UserId = userId,
                FeatureAccessTimes = new List<FeatureAccessTimeQueryModel>()
                {
                    new FeatureAccessTimeQueryModel()
                    {
                        UserId = userId,
                        CourseId = request.CourseId,
                        UnitId = unit.LearningResultId
                    }
                },
            });

            var featureAccessTimeUnit = featureAccessTimeUnitResults?.Content?.Result?.FirstOrDefault();

            var model = new UnitStudentProgressModel
            {
                Type = unit.Type,
                ObjectId = unit.LearningTemplateId,
                Name = unit.ComponentName,
                Status = unit.Status ?? EnumResultStatus.Unfinished,
                DisplayOrder = unit.DisplayOrder,
                TotalLesson = unit.Children.Where(p => p.Type == EnumUnitConfigType.Lesson.ToString()).Count()
            };

            var totalContent = unit.Children.Sum(p => p.TotalContent);
            var totalContentComplete = unit.Children.Sum(p => p.TotalContentCompleted);

            model.ContentProgress = $"{totalContentComplete} / {totalContent}";
            model.TotalContent = totalContent;
            model.TotalContentComplete = totalContentComplete;

            if (featureAccessTimeUnit != null)
            {
                model.TimeSpent = featureAccessTimeUnit.AccessTime;
                model.LastVisited = featureAccessTimeUnit.LastVisited;
            }

            methodResult.Result = model;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}