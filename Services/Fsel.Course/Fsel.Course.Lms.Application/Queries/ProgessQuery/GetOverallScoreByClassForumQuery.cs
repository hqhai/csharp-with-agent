// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Services.UserServices;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
    public class GetOverallScoreByClassForumQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByClassForumQueryHandler : IRequestHandler<GetOverallScoreByClassForumQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitRepository _unitRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByClassForumQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IClassForumRepository classForumRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _classForumRepository = classForumRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                  .ThenInclude(x => x.Lesson)
                  .ThenInclude(x => x!.ClassForum)
                  .Include(x => x.CourseUnitMockTests)
                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                  .ToListAsync(cancellationToken);

            var classForumIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).Select(x => x!.ClassForum).Select(x => x!.Id).ToList();
            var classForums = await _classForumRepository.Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                         .ThenInclude(x => x.ClassForumScores)
                                                                         .Where(x => classForumIds.Contains(x.Id))
                                                                         .ToListAsync(cancellationToken);
            var skillScores = classForums.GroupBy(x => x.CourseSkill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CorrectCount = x.SelectMany(x => x.ClassForumResults).SelectMany(x => x.ClassForumScores).Sum(x => x.Score),
                TotalCount = 36,
            }).ToList();
            skillScores.ForEach(x => x.Percent = x.CorrectCount / x.TotalCount);
            overallScoreReport.SkillScores = skillScores;
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
