// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery.V1i1
{
    using System.Data;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.DashboardModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCompetencyAssessmentRadarQuery : IRequest<MethodResult<CompetencyRadarModel>>
    {
        public DateTime? Date { get; set; }
    }

    public class GetCompetencyAssessmentRadarQueryHandler : IRequestHandler<GetCompetencyAssessmentRadarQuery, MethodResult<CompetencyRadarModel>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetCompetencyAssessmentRadarQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            AuthContext authContext,
            IUnitResultRepository unitResultRepository,
            IPlacementTestResultRepository placementTestResultRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _unitResultRepository = unitResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<CompetencyRadarModel>> Handle(GetCompetencyAssessmentRadarQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CompetencyRadarModel>();
            var competencyRadar = new CompetencyRadarModel();

            // 1) Student
            var methodStudent = await GetStudentModelAsync();
            if (!methodStudent.IsOK)
            {
                methodResult.AddErrorBadRequest(methodStudent.ErrorMessages);
                return methodResult;
            }
            var student = methodStudent.Result!;

            var methodCourse = await GetCourseResultAsync(student.Id, cancellationToken);
            if (!methodCourse.IsOK)
            {
                methodResult.AddErrorBadRequest(methodCourse.ErrorMessages);
                return methodResult;
            }
            var courseResult = methodCourse.Result!;
            if (courseResult.Status == EnumResultStatus.Done)
            {
                competencyRadar.SkillScores = courseResult.SkillScores;
            }
            else
            {
                var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                            .Where(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done && x.CourseId == courseResult.CourseId)
                                                            .ToArrayAsync(cancellationToken);
                if (unitResults != null && unitResults.Any())
                {
                    competencyRadar.SkillScores = unitResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                        .SelectMany(x => x.SkillScores!)
                        .GroupBy(x => x.SkillId)
                        .Select(x => new SkillScores
                        {
                            SkillId = x.Key,
                            SkillName = x.Select(n => n.SkillName).FirstOrDefault(),
                            CorrectCount = x.Sum(x => x.CorrectCount),
                            TotalCount = x.Sum(x => x.TotalCount),
                            Scores = x.Average(x => x.Scores),
                            CountQuestion = x.Sum(x => x.CountQuestion),
                            TotalQuestion = x.Sum(x => x.TotalQuestion),
                        }).ToList();
                }
                else
                {
                    var placementTestScore = await _placementTestResultRepository.Queryable.OrderByDescending(x => x.CreatedDate)
                                                                                 .FirstOrDefaultAsync(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done, cancellationToken);
                    competencyRadar.SkillScores = placementTestScore?.SkillScores;
                }
            }
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ResourceSettings.DashboardI18n);
            var rangeConfig = ConvertHelper.DeserializeFromFilePath<RotationConfig>(path) ?? new RotationConfig();

            competencyRadar.Texti18n = GetRotatingMessage((competencyRadar.SkillScores ?? new List<SkillScores>()).ToList(), rangeConfig, request.Date);
            methodResult.Result = competencyRadar;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<StudentModel>> GetStudentModelAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            methodResult.Result = student;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult>> GetCourseResultAsync(Guid studentId, CancellationToken ct)
        {
            var methodResult = new MethodResult<CourseResult>();

            var courseResult = await _courseResultRepository.Queryable
                .Include(x => x.Course)
                .Where(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active)
                .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            methodResult.Result = courseResult;
            return methodResult;
        }

        private static string GetRotatingMessage(
                    IReadOnlyList<SkillScores> skills,
                    RotationConfig config,
                    DateTimeOffset? now = null)
        {
            if (skills == null || skills.Count == 0 || config?.Rules == null || config.Rules.Count == 0)
            {
                return string.Empty;
            }

            var t = now ?? DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var periodSeconds = Math.Max(1, config.PeriodMinutes) * 60.0;
            var slot = (long)Math.Floor(t.ToUnixTimeSeconds() / periodSeconds);

            // Chỉ lấy rule hợp lệ (có message)
            var rules = config.Rules
                             .Where(r => r != null && r.Messages != null && r.Messages.Count > 0)
                             .OrderBy(x => x.Min).ThenBy(x => x.Max)
                             .ToList();

            foreach (var rule in rules)
            {
                var matchedSkills = skills.Where(s => InRangeInclusive(s.Percent, rule.Min ?? default, rule.Max ?? default)).ToList();
                var isMatch = rule.MatchMode == RuleMatchMode.All
                                ? matchedSkills.Count == skills.Count
                                : matchedSkills.Count > 0;

                if (!isMatch)
                {
                    continue;
                }
                return PickAggregate(rule, slot);
            }

            // 4) Fallback: nếu không có rule nào match, lấy rule đầu tiên
            var fallback = rules.First();
            return PickAggregate(fallback, slot);
        }

        private static bool InRangeInclusive(double value, double min, double max)
           => value >= min && value <= max;

        private static bool InRange(double value, double? min, double? max)
        {
            var lowerOk = !min.HasValue || value >= min.Value;
            var upperOk = !max.HasValue || value <= max.Value;
            return lowerOk && upperOk;
        }

        private static string PickAggregate(RangeConfigModel rule, long baseIdx)
        {
            static int Mod(long a, int m) => m <= 0 ? 0 : (int)((a % m + m) % m);
            if (rule == null || rule.Messages == null || rule.Messages.Count == 0)
            {
                return string.Empty;
            }
            var msgIdx = Mod(baseIdx, rule.Messages.Count);
            return rule.Messages[msgIdx] ?? string.Empty;
        }
    }
}
