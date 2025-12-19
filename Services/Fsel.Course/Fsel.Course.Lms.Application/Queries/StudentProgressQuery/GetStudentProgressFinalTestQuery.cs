// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentProgressFinalTestQuery : IRequest<MethodResult<UnitStudentProgressModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetStudentProgressFinalTestQueryHandler : IRequestHandler<GetStudentProgressFinalTestQuery, MethodResult<UnitStudentProgressModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public GetStudentProgressFinalTestQueryHandler(ICourseRepository courseRepository, IMapper mapper, ISectionGroupRepository sectionGroupRepository, IFinalTestResultRepository finalTestResultRepository, IFinalTestRepository finalTestRepository, IUserService userService, ISystemService systemService, ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
            _userService = userService;
            _systemService = systemService;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<UnitStudentProgressModel>> Handle(GetStudentProgressFinalTestQuery request, CancellationToken cancellationToken)
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
            var userId = student?.UserId;

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            if (course.CourseType == EnumCourseType.Ielts)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var finalTestId = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.FinalTestId.HasValue).Select(x => x.FinalTestId).FirstOrDefaultAsync(cancellationToken);
            var finalTest = await _finalTestRepository.GetByIdAsync(finalTestId ?? default);
            if (finalTest == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.FinalTestId == finalTestId && x.CourseId == request.CourseId && x.StudentId == request.StudentId)
                                                                            .FirstOrDefaultAsync(cancellationToken);
            if (finalTestResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeAsync(new FeatureAccessTimeQueryModel
            {
                UserId = userId ?? default,
                ObjectId = finalTestResult.Id,
                EnumFeature = EnumFeature.FinalTest,
                CourseId = course.Id
            });
            var featureAccessTime = featureAccessTimeResult?.Content?.Result;
            var finalStudentProgress = await GetFinalTestAsync(finalTest, finalTestResult, cancellationToken);

            if (featureAccessTime != null)
            {
                finalStudentProgress.TimeSpent = featureAccessTime.AccessTime;
                finalStudentProgress.LastVisited = featureAccessTime.LastVisited ?? null;
            }

            methodResult.Result = finalStudentProgress;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<List<SectionGroup>> GetSectionGroupsAsync(FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            return await _sectionGroupRepository.Queryable.Include(x => x.FinalTestSections)
                                   .Include(x => x!.Sections)
                                   .ThenInclude(x => x.SectionQuestions)
                                   .ThenInclude(x => x.Question)
                                   .Include(x => x!.Sections)
                                   .ThenInclude(x => x.SectionQuestions)
                                   .ThenInclude(x => x.FinalTestAnswers.Where(x => x.FinalTestResultId == finalTestResult.Id))
                                   .Where(x => x.FinalTestSections.Any(x => x.FinalTestId == finalTestResult.FinalTestId))
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);
        }

        private async Task<UnitStudentProgressModel> GetFinalTestAsync(FinalTest finalTest, FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            var isDone = finalTestResult.Status == EnumResultStatus.Done;
            var finalStudentProgress = new UnitStudentProgressModel
            {
                Type = nameof(finalTestResult.FinalTest),
                ObjectId = finalTest.Id,
                Name = finalTest.Name,
                SkillScores = _mapper.Map<IList<TestSkillScores>>(finalTestResult.SkillScores),
                Status = finalTestResult.Status,
                ContentProgress = string.Format("{0} / {1}", isDone ? 1 : 0, 1)
            };
            if (finalTestResult.Status != EnumResultStatus.Done)
            {
                var sectionGroups = await GetSectionGroupsAsync(finalTestResult, cancellationToken);
                finalStudentProgress.SkillScores = sectionGroups.Select(x =>
                {
                    var sectionQuestions = x!.Sections.SelectMany(x => x.SectionQuestions).ToList();
                    var correctTotal = sectionQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal);
                    var correctCount = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Sum(x => x!.CorrectCount);
                    var totalQuestion = sectionQuestions.Select(x => x.Question).Count();
                    var countQuestion = sectionQuestions.SelectMany(x => x.FinalTestAnswers).Count();
                    var skillScores = new TestSkillScores
                    {
                        Skill = x.CourseSkill,
                        CorrectCount = correctCount,
                        TotalCount = correctTotal,
                        CountQuestion = countQuestion,
                        TotalQuestion = totalQuestion,
                    };
                    return skillScores;
                }).ToList();
            }
            var skillScores = finalStudentProgress.SkillScores;
            var skillDone = skillScores.Where(x => x.CountQuestion == x.TotalQuestion).Count();
            if (skillScores.Any())
            {
                finalStudentProgress.CorrectPercent = NumberHelper.ConvertPercentDouble(skillScores.Sum(x => x.CorrectCount) / skillScores.Sum(x => x.TotalCount));
                finalStudentProgress.ProcessPercent = NumberHelper.ConvertPercentDouble((double)skillScores.Average(x => x.CountQuestion / x.TotalQuestion));
            }
            finalStudentProgress.TotalSkill = skillScores.Count;
            return finalStudentProgress;
        }
    }
}
