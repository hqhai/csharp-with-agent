// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressUnitsQuery : IRequest<MethodResult<IList<UnitStudentProgressModel>>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public bool? IsGetTime { get; set; }
    }

    public class GetStudentProgressUnitsQueryHandler : IRequestHandler<GetStudentProgressUnitsQuery, MethodResult<IList<UnitStudentProgressModel>>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILearningService _learningService;
        private readonly IServiceProvider _serviceProvider;

        public GetStudentProgressUnitsQueryHandler(ICourseRepository courseRepository, IUnitRepository unitRepository, ManagerProgressHelper managerProgressHelper, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository, ICourseResultRepository courseResultRepository, ILearningService learningService, IServiceProvider serviceProvider)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _managerProgressHelper = managerProgressHelper;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _courseResultRepository = courseResultRepository;
            _learningService = learningService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<IList<UnitStudentProgressModel>>> Handle(GetStudentProgressUnitsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitStudentProgressModel>>();

            var unitStudentProgress = new List<UnitStudentProgressModel>();

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
            null,
            cancellationToken);

            if (learningTree == null)
            {
                return methodResult;
            }

            var units = learningTree
               .GetAllItemByType<UnitComponentModel>().OrderBy(p => p.DisplayOrder)
               .ToList();

            var unitIds = units.Where(p => p.Type == EnumCourseConfigType.Unit.ToString()).Select(x => x.LearningTemplateId).ToList();
            var testIds = units.Where(p => p.Type == EnumCourseConfigType.Test.ToString()).Select(x => x.LearningTemplateId).ToList();

            var featureAccessTimeUnits = new List<FeatureAccessTimeModel>();
            var featureAccessTimeTests = new List<FeatureAccessTimeModel>();

            if (!request.IsGetTime.HasValue || request.IsGetTime.Value)
            {
                var featureAccessTimeUnitResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
                {
                    UserId = userId,
                    FeatureAccessTimes = unitIds.Select(x => new FeatureAccessTimeQueryModel
                    {
                        UserId = userId,
                        UnitId = x,
                        CourseId = request.CourseId
                    }).ToList(),
                });
                featureAccessTimeUnits = featureAccessTimeUnitResults?.Content?.Result?.ToList();

                var featureAccessTimeTestResults = await _systemService.GetFeatureAccessTimesAsync(new FeatureAccessTimesQueryModel
                {
                    UserId = userId,
                    FeatureAccessTimes = testIds.Select(x => new FeatureAccessTimeQueryModel
                    {
                        UserId = userId,
                        ObjectId = x,
                        CourseId = request.CourseId
                    }).ToList(),
                });
                featureAccessTimeTests = featureAccessTimeTestResults?.Content?.Result?.ToList();
            }

            foreach (var unit in units)
            {
                var model = new UnitStudentProgressModel
                {
                    Type = unit.Type,
                    ObjectId = unit.LearningTemplateId,
                    Name = unit.ComponentName,
                    Status = unit.Status ?? EnumResultStatus.Unfinished,
                    DisplayOrder = unit.DisplayOrder,
                    ModuleId = unit.Id,
                    TotalLesson = unit.Children.Count(p => p.Type == EnumUnitConfigType.Lesson.ToString())
                };

                if (unit.Type == EnumCourseConfigType.Unit.ToString())
                {
                    var totalContent = unit.Children.Sum(p => p.TotalContent);
                    var totalContentComplete = unit.Children.Sum(p => p.TotalContentCompleted);

                    model.ContentProgress = $"{totalContentComplete} / {totalContent}";
                    model.TotalContent = totalContent;
                    model.TotalContentComplete = totalContentComplete;

                    var featureAccessTime = featureAccessTimeUnits?
                        .FirstOrDefault(x => x.UnitId == unit.LearningTemplateId);

                    if (featureAccessTime != null)
                    {
                        model.TimeSpent = featureAccessTime.AccessTime;
                        model.LastVisited = featureAccessTime.LastVisited;
                    }
                }
                else
                {
                    model.TotalSkill = unit.NumberSkill;
                    model.ContentProgress = unit.Status == EnumResultStatus.Done ? "1 / 1" : "0 / 1";
                    model.TotalContent = 1;
                    model.TotalContentComplete = unit.Status == EnumResultStatus.Done ? 1 : 0;

                    model.SkillScores = unit.SkillScores?
                        .Select(p => new TestSkillScores
                        {
                            SkillId = p.SkillId,
                            SkillFilePath = p.SkillFilePath,
                            SkillName = p.SkillName,
                            CorrectCount = p.CorrectCount,
                            CorrectQuestion = p.CorrectQuestion,
                            CountQuestion = p.CountQuestion,
                            TotalCount = p.TotalCount,
                            Percent = p.Percent,
                            Scores = p.Scores,
                        })
                        .ToList() ?? new List<TestSkillScores>();

                    var totalCount = model.SkillScores.Sum(x => x.TotalCount);
                    var correctCount = model.SkillScores.Sum(x => x.CorrectCount);
                    if (totalCount > 0)
                    {
                        model.CorrectPercent = NumberHelper.ConvertPercentDouble(
                  model.SkillScores.Sum(x => x.CorrectCount) /
                  model.SkillScores.Sum(x => x.TotalCount)
              );
                    }
                    else
                    {
                        model.CorrectPercent = 0;
                    }

                    var featureAccessTime = featureAccessTimeTests?
                        .FirstOrDefault(x => x.ObjectId == unit.LearningTemplateId);

                    if (featureAccessTime != null)
                    {
                        model.TimeSpent = featureAccessTime.AccessTime;
                        model.LastVisited = featureAccessTime.LastVisited;
                    }
                }

                model.ProcessPercent = NumberHelper.GetPercent(model.TotalContentComplete, model.TotalContent);
                model.CurrentLesson = unit.Children.FirstOrDefault(p => p.Status == EnumResultStatus.New || p.Status == EnumResultStatus.Process)?.DisplayOrder;

                unitStudentProgress.Add(model);
            }

            methodResult.Result = unitStudentProgress.OrderBy(p => p.DisplayOrder).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
