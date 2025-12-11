// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUserService _userService;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;

        public GetOverallScoreQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , IPlacementTestResultRepository placementTestResultRepository
            , IUnitResultRepository unitResultRepository
            , IUserService userService
            , IMockTestResultRepository mockTestResultRepository
            , IMapper mapper)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _userService = userService;
            _mockTestResultRepository = mockTestResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<OverallScoreModel>> Handle(GetOverallScoreQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreModel> methodResult = new MethodResult<OverallScoreModel>();
            OverallScoreModel overallScoreModel = new OverallScoreModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var level = student.CourseLevel;
            var studentId = student.Id;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var courseResult = await _courseResultRepository.ReadQueryable.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (courseResult != null && courseResult.Status == EnumResultStatus.Done)
            {
                overallScoreModel.SkillScores = courseResult.SkillScores;
                overallScoreModel.Percent = courseResult.Percent;
                overallScoreModel.NextCourseLevel = EnumCourseLevelHelper.GetEnumNextCourseLevel(course.CourseType, course.CourseLevel);
            }
            else
            {
                var unitResults = await _unitResultRepository.ReadQueryable.Include(x => x.Unit)
                                                             .Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done && x.CourseId == request.CourseId)
                                                             .ToArrayAsync(cancellationToken);
                if (unitResults != null && unitResults.Any())
                {
                    overallScoreModel.SkillScores = unitResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                        .SelectMany(x => x.SkillScores!)
                        .GroupBy(x => x.Skill)
                        .Select(x => new SkillScores
                        {
                            Skill = x.Key,
                            CorrectCount = x.Sum(x => x.CorrectCount),
                            TotalCount = x.Sum(x => x.TotalCount),
                            Scores = x.Average(x => x.Scores),
                            CountQuestion = x.Sum(x => x.CountQuestion),
                            TotalQuestion = x.Sum(x => x.TotalQuestion),
                        }).ToList();
                    overallScoreModel.Percent = NumberHelper.ConvertRound(unitResults.Any() ? unitResults.Average(x => x.Percent) : default);
                }
                else
                {
                    var placementTestScore = await _placementTestResultRepository.ReadQueryable.OrderByDescending(x => x.CreatedDate)
                                                        .FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done, cancellationToken);
                    if (placementTestScore == null)
                    {
                        methodResult.Result = overallScoreModel;
                        return methodResult;
                    }
                    overallScoreModel.SkillScores = placementTestScore.SkillScores;
                    overallScoreModel.IsPlacement = true;
                    overallScoreModel.Percent = placementTestScore.Percent;
                }
            }

            overallScoreModel.CourseLevel = course.CourseLevel;
            overallScoreModel.CourseType = course.CourseType;
            if (course.CourseType == Shared.Enums.EnumCourseType.Ielts)
            {
                var mockTestResult = await _mockTestResultRepository.ReadQueryable.Where(x => x.StudentId == studentId && x.CourseId == course.Id && !x.UnitId.HasValue)
                                                                    .OrderByDescending(x => x.CreatedDate)
                                                                    .ThenByDescending(x => x.UpdatedDate)
                                                                    .FirstOrDefaultAsync(cancellationToken);
                var mockTestResultModel = _mapper.Map<MockTestResultModel>(mockTestResult);
                overallScoreModel.BandScores = mockTestResultModel.Scores;
                overallScoreModel.TargetBandScores = course.CourseLevel.GetBandScore();
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreModel;
            return methodResult;
        }
    }
}
