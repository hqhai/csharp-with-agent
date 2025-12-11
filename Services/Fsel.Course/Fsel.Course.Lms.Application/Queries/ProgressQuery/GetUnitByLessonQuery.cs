// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByLessonQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetUnitByLessonQueryHandler : IRequestHandler<GetUnitByLessonQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly VideoConverter _videoConverter;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;

        public GetUnitByLessonQueryHandler(AuthContext authContext
            , VideoConverter videoConverter
            , IVideoResultRepository videoResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _videoConverter = videoConverter;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetUnitByLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();
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
            var studentId = student.Id;
            var videoResult = await _videoResultRepository.ReadQueryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var timeCodeScoreResult = await _videoConverter.GetVideoSkillScores(videoResult, cancellationToken);
            var skillScores = timeCodeScoreResult.Item1.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
            if (skillScores != null)
            {
                overallScoreReport.SkillScores = skillScores;
                overallScoreReport.CorrectCount = skillScores.Sum(x => x.CorrectCount);
                overallScoreReport.CorrectTotal = skillScores.Sum(x => x.TotalCount);
                overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
                overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
                overallScoreReport.CourseSkills = skillScores.Select(x => x!.Skill).Distinct().ToList();
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
