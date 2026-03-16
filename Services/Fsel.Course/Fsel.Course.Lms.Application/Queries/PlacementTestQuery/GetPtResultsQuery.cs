// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.QueryModels.PlacementTests;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPtResultsQuery : IRequest<MethodResult<List<PtResultDto>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetPtResultsQueryHandler : IRequestHandler<GetPtResultsQuery, MethodResult<List<PtResultDto>>>
    {
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly ITestService _testService;
        private readonly IUserService _userService;

        public GetPtResultsQueryHandler(IRepository<TestGroupResult> testGroupResult, ITestService testService, IUserService userService)
        {
            _testGroupResult = testGroupResult;
            _testService = testService;
            _userService = userService;
        }

        public async Task<MethodResult<List<PtResultDto>>> Handle(GetPtResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PtResultDto>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var user = studentResult?.Content?.Result?.User;

            if (user == null)
            {
                return methodResult;
            }

            var ptResults = await _testGroupResult.ReadQueryable
                .Include(x => x.CurrentLevel)
                .Include(x => x.Category)
                .Include(x => x.TestResults)
                .ThenInclude(x => x.StepFlow)
                .ThenInclude(x => x.Level)
                .Where(x => x.StudentId == student.Id)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            var results = ptResults.Select(x => new PtResultDto
            {
                Id = x.Id,
                SubjectName = x.Category?.Name,
                CurrentLevel = x.CurrentLevel?.Name,
                ModuleResults = x.TestResults.OrderBy(x => x.CreatedDate).Select((tr, i) => new ModuleResult
                {
                    ModuleName = $"Module{i + 1}",
                    Percent = tr.Percent,
                    LevelOfModule = tr.StepFlow.Level.Name,
                    SkillResults = tr.SkillScores.Select(sr => new SkillResult
                    {
                        SkillName = sr.SkillName,
                        Percent = sr.Percent
                    }).OrderBy(x => x.SkillName).ToList()
                }).ToList()
            }).ToList();

            foreach (var ptResult in results)
            {
                var levels = await _testService.GetSuggestLevels(ptResult.Id,
                    DateTimeHelper.GetYearOld(student.User.Birthday),
                    isOpenOldLevel: false,
                    cancellationToken: cancellationToken);

                ptResult.SuggestLevel = levels.FirstOrDefault(x => x.CourseType == EnumSubjectConditionValueType.Recommended.ToString())?.Name ?? string.Empty;
            }
            methodResult.Result = results.ToList();
            return methodResult;
        }
    }
}
