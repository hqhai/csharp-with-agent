// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ExamPracticeQuery.V1i1
{
    using System.IO;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Configs;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices;
    using Fsel.ExamPractice.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDashboardReportByTypeQuery : IRequest<MethodResult<ExamDashboardModel>>
    {
        public EnumExamPracticeType Type { get; set; }
    }

    public class GetDashboardReportByTypeQueryHandler : IRequestHandler<GetDashboardReportByTypeQuery, MethodResult<ExamDashboardModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;

        public GetDashboardReportByTypeQueryHandler(
            AuthContext authContext,
            IUserService userService,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeRepository examPracticeRepository,
            IMapper mapper)
        {
            _authContext = authContext;
            _userService = userService;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ExamDashboardModel>> Handle(GetDashboardReportByTypeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamDashboardModel>();

            var methodResultStudent = await GetStudentAsync();
            if (!methodResultStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodResultStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodResultStudent.Result ?? new StudentModel();

            ExamDashboardModel examDashboard = new ExamDashboardModel();
            var examPracticeResults = await (from epr in _examPracticeResultRepository.Queryable
                                             join ep in _examPracticeRepository.Queryable on epr.ExamPracticeId equals ep.Id
                                             where epr.StudentId == student.Id && ep.Type == request.Type
                                             && epr.WorkingStatus == EnumWorkingStatus.Active && epr.Status == EnumResultStatus.Done
                                             select epr).AsNoTracking().ToListAsync(cancellationToken);
            var totalExamPractice = await (from ep in _examPracticeRepository.Queryable
                                           where ep.Type == request.Type && !ep.IsArchive && ep.Status == EnumExamPracticeStatus.Active
                                           select ep).AsNoTracking().CountAsync(cancellationToken);

            examDashboard.CompletedCount = examPracticeResults.Count;
            examDashboard.TotalCount = totalExamPractice;
            var examPracticeResultModels = _mapper.Map<IList<ExamPracticeResultModel>>(examPracticeResults);

            var examBandScores = GetSkillScoreDescriptors();
            var score = examPracticeResultModels.Any() ? examPracticeResultModels.Average(x => x.Score) : default;
            var scoreOvrall = examPracticeResultModels.OrderByDescending(x => x.CreatedDate).FirstOrDefault()?.Score ?? 0;

            examDashboard.AverageScore = score;
            examDashboard.AverageLevel = MapScoreToLevel(examBandScores, score);
            examDashboard.OverallScore = scoreOvrall;
            examDashboard.OverallLevel = MapScoreToLevel(examBandScores, scoreOvrall);
            methodResult.Result = examDashboard;
            return methodResult;
        }

        private static IList<SkillScoreDescriptorModel>? GetSkillScoreDescriptors()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.ExamBandScores);
            return ConvertHelper.DeserializeFromFilePath<IList<SkillScoreDescriptorModel>>(path);
        }

        private static string? MapScoreToLevel(IList<SkillScoreDescriptorModel>? bands, double score)
        {
            if (bands?.Count == 0)
            {
                return null;
            }
            var band = bands?.FirstOrDefault(b => score >= b.MinInclusive && score <= b.MaxInclusive);
            return band?.Level;
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            MethodResult<StudentModel> methodResult = new();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }
    }
}
