// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.SectionGroupResultQuery
{
    using System.IO;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.BandScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetSectionGroupResultReportQuery : IRequest<MethodResult<SectionGroupResultModel>>
    {
        public Guid MockTestResultId { get; set; }
        public Guid SectionGroupId { get; set; }
    }

    public class GetSectionGroupResultReportQueryHandler : IRequestHandler<GetSectionGroupResultReportQuery, MethodResult<SectionGroupResultModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMapper _mapper;

        public GetSectionGroupResultReportQueryHandler(IUserService userService, AuthContext authContext, IMockTestResultRepository mockTestResultRepository, ISectionGroupResultRepository sectionGroupResultRepository, IMapper mapper)
        {
            _userService = userService;
            _authContext = authContext;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<SectionGroupResultModel>> Handle(GetSectionGroupResultReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SectionGroupResultModel>();
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentsResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult?.Content?.Result?.Id;

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId && x.StudentId == studentId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.MockTestResultId == request.MockTestResultId && x.SectionGroupId == request.SectionGroupId && x.StudentId == studentId && x.CreatedDate >= mockTestResult.CreatedDate, cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            if (sectionGroupResult.Status != EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusNotDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }
            var courseSkill = sectionGroupResult.SectionGroup!.CourseSkill;
            if ((courseSkill == EnumCourseSkill.Reading || courseSkill == EnumCourseSkill.Listening) && sectionGroupResult.SkillScores != null)
            {
                var sectionGroupResultReport = _mapper.Map<SectionGroupResultModel>(sectionGroupResult);
                var scores = sectionGroupResult.SkillScores.Select(x => x.Scores).FirstOrDefault();
                sectionGroupResultReport.BandScoresReport = GetBandScoresReport(sectionGroupResult, scores);
                (sectionGroupResultReport.IsCheckScoreColor, sectionGroupResultReport.TargetBandScore) = mockTestResult.Course!.CourseLevel.CheckScoreColor(scores);
                methodResult.Result = sectionGroupResultReport;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private BandScoresReport GetBandScoresReport(SectionGroupResult sectionGroupResult, double scores)
        {
            var bandScores = GetBandScoreConfigs(sectionGroupResult);
            var bandScore = GetBandScore(bandScores, scores);
            var bandScoreReport = _mapper.Map<BandScoresReport>(bandScore);
            var bandScoreStudent = bandScores?.FirstOrDefault(x => x.Scores == scores);
            if (bandScoreStudent != null)
            {
                bandScoreReport.ScoresStudent = bandScoreStudent.Scores;
                bandScoreReport.CorrectAnswerStudents = bandScoreStudent.CorrectAnswers;
            }
            return bandScoreReport;
        }

        private static IList<BandScores>? GetBandScoreConfigs(SectionGroupResult sectionGroupResult)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.BandScoreFileName);
            var bandScores = ConvertHelper.DeserializeFromFilePath<IList<BandScores>>(path);
            bandScores = bandScores?.Where(x => x.CourseSkill == sectionGroupResult.SectionGroup!.CourseSkill).OrderByDescending(x => x.Scores).ToList();
            return bandScores;
        }

        private static BandScores? GetBandScore(IList<BandScores>? bandScores, double scores)
        {
            if (scores > 0)
            {
                return bandScores?.FirstOrDefault(x => x.Scores < scores);
            }
            return bandScores?.FirstOrDefault(x => x.Scores == scores);
        }
    }
}
