// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreQuery : IRequest<MethodResult<OverallScoreModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreQueryHandler : IRequestHandler<GetOverallScoreQuery, MethodResult<OverallScoreModel>>
    {
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUserService _userService;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ICategoryService _categoryService;

        public GetOverallScoreQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , IUnitResultRepository unitResultRepository
            , IUserService userService
            , ITestGroupResultRepository testGroupResultRepository
            , ITestResultRepository testResultRepository
            , ICategoryService categoryService)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
            _userService = userService;
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _categoryService = categoryService;
        }

        public async Task<MethodResult<OverallScoreModel>> Handle(GetOverallScoreQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallScoreModel>();
            var overallScoreModel = new OverallScoreModel();
            var method = await GetStudentAsync();
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentId = method.Result!.Id;

            var course = await _courseRepository.ReadQueryable.Include(x => x.Level)
                                                .Include(x => x.Program)
                                                .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.ReadQueryable.Where(x => x.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active)
                                                            .Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)
                                                            .FirstOrDefaultAsync(cancellationToken);

            if (courseResult != null && courseResult.Status == EnumResultStatus.Done)
            {
                overallScoreModel.SkillScores = courseResult.SkillScores;
                overallScoreModel.Percent = courseResult.Percent;
            }
            else
            {
                var unitResults = await _unitResultRepository.ReadQueryable.Include(x => x.Unit)
                                            .Where(x => x.Status == EnumResultStatus.Done)
                                            .Where(x => courseResult != null && x.CourseResultId == courseResult.Id)
                                            .ToListAsync(cancellationToken);

                if (unitResults != null && unitResults.Any())
                {
                    overallScoreModel.SkillScores = unitResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                        .SelectMany(x => x.SkillScores!)
                        .GroupBy(x => new { x.SkillId })
                        .Select(x => new SkillScores
                        {
                            SkillFilePath = x.Where(x => x.SkillFilePath != null).FirstOrDefault()?.SkillFilePath,
                            SkillName = x.Where(x => x.SkillName != null).FirstOrDefault()?.SkillName,
                            SkillId = x.Key.SkillId,
                            CorrectCount = x.Sum(x => x.CorrectCount),
                            TotalCount = x.Sum(x => x.TotalCount),
                            Scores = x.Average(x => x.Scores),
                            CountQuestion = x.Sum(x => x.CountQuestion),
                            TotalQuestion = x.Sum(x => x.TotalQuestion),
                        }).ToList();
                    overallScoreModel.Percent = courseResult?.Percent ?? default;
                }
                else
                {
                    var testGroupResult = await _testGroupResultRepository.ReadQueryable
                                                .Where(x => x.StudentId == studentId && x.TestType == EnumTestType.PlacementTest)
                                                .FirstOrDefaultAsync(cancellationToken);
                    if (testGroupResult != null && testGroupResult.Status == EnumResultStatus.ByPass)
                    {
                        overallScoreModel.SkillScores = await _categoryService.GetDefaultSkillScoresAsync(course.ProgramId, cancellationToken);
                        overallScoreModel.IsPlacement = true;
                    }
                    else
                    {
                        var testResult = await _testResultRepository.ReadQueryable.Include(x => x.Test)
                                   .Where(x => x.Status == EnumResultStatus.Done)
                                   .Where(x => testGroupResult != null && x.TestGroupResultId == testGroupResult.Id)
                                   .OrderByDescending(x => x.CreatedDate)
                                   .FirstOrDefaultAsync(cancellationToken);
                        if (testResult == null)
                        {
                            return methodResult;
                        }
                        overallScoreModel.BandScores = testResult.Score ?? default;
                        overallScoreModel.ScoringFormulaType = testResult.Test?.ScoringFormulaType;
                        overallScoreModel.SkillScores = testResult.SkillScores;
                        overallScoreModel.IsPlacement = true;
                        overallScoreModel.Percent = testResult.PercentModule;
                    }
                }
            }
            overallScoreModel.ProgramId = course.ProgramId;
            overallScoreModel.ProgramName = course.Program?.Name;

            overallScoreModel.LevelId = course.LevelId;
            overallScoreModel.LevelName = course.Level?.Name;

            overallScoreModel.CourseLevel = course.CourseLevel;
            overallScoreModel.CourseType = course.CourseType;

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreModel;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }
    }
}
