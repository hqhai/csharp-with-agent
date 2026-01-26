// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ManagerReportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.Course.Domain.Models.QueryModels.ManagerReports;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetOverallReportLearningResultQuery : SearchReportLearningResultQueryModel, IRequest<MethodResult<OverallReportLearningResultModel>>
    {
        public GetOverallReportLearningResultQuery()
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
        }

        public GetOverallReportLearningResultQuery(SearchReportLearningResultQueryModel source)
        {
            ManagerReportType = EnumManagerReportType.ReportLearningResults;
            CopyFrom(source);
        }
    }

    public class GetOverallReportLearningResultQueryHandler
        : IRequestHandler<GetOverallReportLearningResultQuery, MethodResult<OverallReportLearningResultModel>>
    {
        private readonly IMediator _mediator;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitRepository _unitRepository;

        public GetOverallReportLearningResultQueryHandler(
               IMediator mediator,
               IUnitResultRepository unitResultRepository,
               ICourseRepository courseRepository,
               ICourseResultRepository courseResultRepository,
               ICourseModuleRepository courseModuleRepository,
               ICategoryRepository categoryRepository,
               IUnitRepository unitRepository)
        {
            _mediator = mediator;
            _unitResultRepository = unitResultRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _courseModuleRepository = courseModuleRepository;
            _categoryRepository = categoryRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<OverallReportLearningResultModel>> Handle(GetOverallReportLearningResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallReportLearningResultModel>();
            var studentResult = await _mediator.Send(new GetStudentReportQuery(request), cancellationToken);
            if (!studentResult.IsOK)
            {
                methodResult.AddErrorBadRequest(studentResult.ErrorMessages);
                return methodResult;
            }

            var students = studentResult.Result?.ToList() ?? new List<StudentDtoModel>();

            methodResult.Result = await BuildOverallReportAsync(request, students, cancellationToken);
            return methodResult;
        }

        #region Core Logic

        private async Task<OverallReportLearningResultModel> BuildOverallReportAsync(
            GetOverallReportLearningResultQuery request,
            List<StudentDtoModel> students,
            CancellationToken cancellationToken)
        {
            var program = await _categoryRepository.ReadQueryable
                .Include(x => x.Levels)
                .FirstOrDefaultAsync(x => x.Id == request.ProgramId, cancellationToken);

            if (program == null)
            {
                return new OverallReportLearningResultModel();
            }

            var courseIds = students.Where(x => x.CourseId.HasValue)
                                    .Select(x => x.CourseId!.Value)
                                    .Distinct()
                                    .ToList();

            var report = new OverallReportLearningResultModel
            {
                TotalStudent = students.Count,
                CourseLevelProgresses = BuildLevelProgress(program.Levels.ToList(), students),
                MaxUnitCount = await GetMaxUnitCountAsync(courseIds, cancellationToken)
            };

            (var unitModules, var overallAvgPercent) = await GetUnitResultGroupsAsync(request, students, cancellationToken);

            report.OverallModules = unitModules;
            report.OverallAvgPercent = overallAvgPercent;
            return report;
        }

        private async Task<int> GetMaxUnitCountAsync(
            IList<Guid> courseIds,
            CancellationToken cancellationToken)
        {
            if (!courseIds.Any())
            {
                return 0;
            }
            return await _courseRepository.ReadQueryable
                .WhereBulkContains(courseIds, x => x.Id)
                .MaxAsync(x => x.UnitCount, cancellationToken);
        }

        #endregion Core Logic

        #region Unit & Overall Result

        private async Task<(IList<OverallModuleReportModel>, double)> GetUnitResultGroupsAsync(
        GetOverallReportLearningResultQuery request,
        List<StudentDtoModel> students,
        CancellationToken cancellationToken)
        {
            var studentIds = students.Select(x => x.Id).ToList();

            var overallResults = await BuildStudentOverallQuery(request, studentIds)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken);

            var overallScores = request.ListOverallScore.ToList<EnumOverallScore>();

            var matchedStudentIds = overallResults
                .Where(x => OverallScorePolicy.Match(x.OverallPercent, overallScores))
                .Select(x => x.StudentId)
                .ToHashSet();

            students = students
                .Where(x => matchedStudentIds.Contains(x.Id))
                .ToList();

            var unitResults = await BuildUnitProgressQuery(request, matchedStudentIds.ToList())
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var unitModules = unitResults
                .GroupBy(x => x.DisplayOrder)
                .Select(g => new OverallModuleReportModel
                {
                    Index = g.Key,
                    DisplayOrder = g.Key,
                    Percent = g.Any() ? NumberHelper.ConvertRound(g.Average(x => x.Percent)) : ValueDefault,
                    TotalStudent = g.Count(),
                    Type = nameof(Domain.Entities.Unit)
                })
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            var overallAvg = overallResults.Any()
                ? NumberHelper.ConvertRound(overallResults.Average(x => x.OverallPercent))
                : ValueDefault;

            return (unitModules, overallAvg);
        }

        #endregion Unit & Overall Result

        #region Query Builders

        private IQueryable<StudentOverallResult> BuildStudentOverallQuery(
            GetOverallReportLearningResultQuery request,
            List<Guid> studentIds)
        {
            return
                from cr in _courseResultRepository.ReadQueryable
                    .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                    .WhereBulkContains(studentIds, x => x.StudentId)

                join ur in _unitResultRepository.ReadQueryable on cr.Id equals ur.CourseResultId
                where ur.Status == EnumResultStatus.Done
                && (!request.EndDate.HasValue || (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                group ur by cr.StudentId
                into g
                select new StudentOverallResult
                {
                    StudentId = g.Key,
                    OverallPercent = g.Any() ? Math.Round(g.Average(x => x.Percent)) : default
                };
        }

        private IQueryable<UnitProgressResult> BuildUnitProgressQuery(
            GetOverallReportLearningResultQuery request,
            List<Guid> studentIds)
        {
            return
                from cr in _courseResultRepository.ReadQueryable
                    .WhereBulkContains(studentIds, x => x.StudentId)
                join ur in _unitResultRepository.ReadQueryable
                    on cr.Id equals ur.CourseResultId
                join u in _unitRepository.ReadQueryable
                    on ur.UnitId equals u.Id
                join cm in _courseModuleRepository.ReadQueryable
                    on new { ur.CourseId, u.OriginalId }
                    equals new { cm.CourseId, cm.OriginalId }
                where ur.Status == EnumResultStatus.Done
                      && (!request.EndDate.HasValue ||
                          (ur.UpdatedDate ?? ur.CreatedDate).Date <= request.EndDate.Value.Date)
                select new UnitProgressResult
                {
                    StudentId = cr.StudentId,
                    DisplayOrder = cm.DisplayOrder,
                    Percent = ur.Percent
                };
        }

        #endregion Query Builders

        #region Helpers

        private static IList<CourseLevelProgressModel> BuildLevelProgress(
            List<Level> levels,
            List<StudentDtoModel> students)
        {
            return levels.OrderBy(x => x.LevelOrder).Select(level => new CourseLevelProgressModel
            {
                LevelId = level.Id,
                LevelName = level.Code,
                TotalStudent = students.Count(s => s.LevelId == level.Id)
            }).ToList();
        }

        #endregion Helpers
    }

    #region Policies & Read Models

    internal static class OverallScorePolicy
    {
        public static bool Match(double percent, IList<EnumOverallScore>? scores)
        {
            if (scores == null || scores.Count == 0)
            {
                return true;
            }
            return scores.Any(score => score switch
            {
                EnumOverallScore.Accuracy75OrMore => percent >= 75,
                EnumOverallScore.AccuracyBelow75 => percent < 75,
                _ => false
            });
        }
    }

    internal sealed class StudentOverallResult
    {
        public Guid StudentId { get; init; }
        public double OverallPercent { get; init; }
    }

    internal sealed class UnitProgressResult
    {
        public Guid StudentId { get; init; }
        public int DisplayOrder { get; init; }
        public double Percent { get; init; }
    }

    #endregion Policies & Read Models
}
