// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models.Excels;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportFileProgressStudentsToEmailsQuery : BaseImportCommandModel, IRequest<MethodResult<Stream>>
    {
        public string? EventCode { get; set; }
    }

    public class ExportFileProgressStudentsToEmailsQueryHandler : IRequestHandler<ExportFileProgressStudentsToEmailsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ISystemService _systemService;
        private readonly ManagerProgressHelper _managerProgressHelper;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public ExportFileProgressStudentsToEmailsQueryHandler(IUserService userService,
            ICourseResultRepository courseResultRepository,
            ISystemService systemService,
            ManagerProgressHelper managerProgressHelper,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ILessonRepository lessonRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _systemService = systemService;
            _managerProgressHelper = managerProgressHelper;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _lessonRepository = lessonRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportFileProgressStudentsToEmailsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            if (request.FormFile == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.FormFile));
                return methodResult;
            }
            var studentProgressReports = new List<StudentProgressReportModel>();

            var result = request.FormFile.ImportAndValidateExcel(async (ImportStudentEmailModel x, IList<ImportStudentEmailModel> models, int rowIndex, IList<ValidateExcelModel> errors) =>
            {
                return true;
            });
            //var duplicateEmails = result.Datas.GroupBy(user => user.Email).Where(group => group.Count() > 1).Select(group => group.Key);
            //if (duplicateEmails.Any())
            //{
            //    methodResult.AddErrorBadRequest("Duplicate Emails");
            //    return methodResult;
            //}

            result.Datas = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email) && x.Email.IsValidEmail()).Distinct().ToList();

            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!).ToList();
            var studentResultToEmail = await _userService.GetStudentByEmailsAsync(listEmail);
            if (!studentResultToEmail.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResultToEmail.Error);
                return methodResult;
            }
            var students = studentResultToEmail.Content?.Result?.ToList();
            if (students == null || !students.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }
            var schoolIds = students.Where(x => x.SchoolId.HasValue).Select(x => x.SchoolId.GetValueOrDefault()).ToList();
            var studentIds = students.Select(x => x.Id).ToList() ?? new List<Guid>();

            var schoolResults = await _systemService.GetSchoolsAsync(schoolIds);
            if (!schoolResults.IsSuccessStatusCode)
            {
                methodResult.AddError(schoolResults.Error);
                return methodResult;
            }
            var schools = schoolResults.Content?.Result;

            var studentRankingResults = await _userService.GetLeaderBoardDataAsync(new GetStudentCompetitionByEventCodeQueryModel
            {
                WeekNumber = 1,
                EventCode = request.EventCode
            });
            if (!studentRankingResults.IsSuccessStatusCode)
            {
                methodResult.AddError(studentRankingResults.Error);
                return methodResult;
            }
            var studentRankings = studentRankingResults.Content?.Result?.Items;

            var courseResults = await _courseResultRepository.Queryable.Include(x => x.Course)
                .Where(x => studentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active)
                .ToListAsync(cancellationToken);
            foreach (var student in students)
            {
                var courseResult = courseResults.FirstOrDefault(x => x.StudentId == student.Id);
                var studentProgressReport = new StudentProgressReportModel
                {
                    FullName = student.Human?.FullName,
                    Email = student.Human?.Email,
                    School = student.School ?? schools?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name,
                    ProcessDate = courseResult?.ProcessDate,
                    ExpiredDate = student.ExpiredDate
                };
                var placementTestResultCurrent = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                                               .OrderByDescending(x => x.UpdatedDate)
                                                                                               .ThenByDescending(x => x.CreatedDate)
                                                                                               .FirstOrDefaultAsync(cancellationToken);
                if (placementTestResultCurrent != null)
                {
                    var placementTestResult = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == student.Id)
                                                                              .OrderBy(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
                    int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
                    var (levelCompleted, isLock) = placementTestResultCurrent.Level.GetLevelInScore(placementTestResultCurrent.Percent, IeltsScoreHelper.GetInitialAge(placementTestResult?.Level, age));
                    studentProgressReport.StatusUser = isLock ? "Hoàn Thành PT" : "Chưa Hoàn Thành PT";
                }
                if (courseResult != null)
                {
                    var courseResultModel = new CourseResultModel
                    {
                        CourseType = courseResult.Course?.CourseType,
                        CourseId = courseResult.CourseId,
                        StudentId = courseResult.StudentId,
                    };
                    var (currentProgress, progress) = await _managerProgressHelper.GetCompleteCourseAsync(courseResultModel);
                    studentProgressReport.CourseName = courseResult.Course?.Name;
                    studentProgressReport.ProgressPercent = NumberHelper.GetPercent(currentProgress, progress);
                    studentProgressReport.StatusUser = "Đang Học";
                    await SetProgressModuleAsync(studentProgressReport, courseResult);
                    await SetOverallPercentUnitAsync(studentProgressReport, courseResult);
                    var featureAccessTimeResults = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel
                    {
                        UserId = student.Human?.UserId ?? default,
                        CourseId = courseResult.CourseId
                    });

                    if (featureAccessTimeResults.IsSuccessStatusCode)
                    {
                        var featureAccessTimes = featureAccessTimeResults.Content?.Result;
                        studentProgressReport.LearnTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? default;
                        studentProgressReport.SocialTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? default;
                        studentProgressReport.OtherTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? default;
                        studentProgressReport.TotalVisit = featureAccessTimes?.Sum(x => x.TotalVisit) ?? default;
                    }
                    studentProgressReport.LeaderboardPercent = studentRankings?.FirstOrDefault(x => x.StudentId == student.Id && x.CourseResultId == courseResult.Id)?.OverallScore ?? default;
                }

                studentProgressReports.Add(studentProgressReport);
            }

            methodResult.Result = studentProgressReports.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetOverallPercentUnitAsync(StudentProgressReportModel studentProgressReport, CourseResult courseResult)
        {
            var unitResults = await GetUnitResultsAsync(courseResult, CancellationToken.None);
            foreach (var unitResult in unitResults)
            {
                var index = unitResults.IndexOf(unitResult);
                var unitField = typeof(StudentProgressReportModel).GetProperty($"OverallUnit{index + 1}");
                if (unitField != null)
                {
                    unitField.SetValue(studentProgressReport, unitResult.Percent);
                }
            }
        }

        private async Task<IList<UnitResult>> GetUnitResultsAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var unitIds = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.UnitId.HasValue)
                                                                                   .OrderBy(x => x.DisplayOrder)
                                                                                   .Select(x => x.UnitId!)
                                                                                   .ToListAsync(cancellationToken);
            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                        .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId && unitIds.Contains(x.UnitId))
                                                        .Where(x => x.Status == EnumResultStatus.Done)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ToListAsync(cancellationToken);
            return unitResults.OrderBy(x => unitIds.IndexOf(x.UnitId)).ToList();
        }

        private async Task<UnitResult?> GetUnitResultProgressAsync(CourseResult courseResult, CancellationToken cancellationToken)
        {
            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                        .Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId)
                                                        .Where(x => x.Status != EnumResultStatus.Unfinished)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .ToListAsync(cancellationToken);
            var unitResult = unitResults.Where(x => x.Status != EnumResultStatus.Done).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (unitResult == null)
            {
                var courseUnitMockTest = await _courseUnitMockTestRepository.Queryable.Where(x => x.CourseId == courseResult.CourseId && x.UnitId.HasValue)
                    .OrderByDescending(x => x.DisplayOrder)
                    .FirstOrDefaultAsync(cancellationToken);
                if (courseUnitMockTest != null)
                {
                    unitResult = unitResults.FirstOrDefault(x => x.CourseId == courseResult.CourseId && x.UnitId == courseUnitMockTest.UnitId);
                }
            }
            return unitResult;
        }

        private async Task SetProgressModuleAsync(StudentProgressReportModel studentProgressReport, CourseResult courseResult)
        {
            var unitResult = await GetUnitResultProgressAsync(courseResult, CancellationToken.None);
            if (unitResult == null)
            {
                return;
            }
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson).Where(x => x.StudentId == courseResult.StudentId && x.UnitId == unitResult.UnitId)
                                                                      .Where(x => x.Status != EnumResultStatus.Unfinished && x.CourseId == courseResult.CourseId)
                                                                      .OrderByDescending(x => x.CreatedDate)
                                                                      .ThenByDescending(x => x.UpdatedDate)
                                                                      .FirstOrDefaultAsync();
            var lesson = lessonResult?.Lesson;
            if (lesson == null)
            {
                lesson = await _lessonRepository.Queryable.Include(x => x.UnitLessons.Where(y => y.UnitId == unitResult.UnitId)).Where(x => x.UnitLessons.Any(y => y.UnitId == unitResult.UnitId))
                                                                                      .OrderBy(x => x.UnitLessons.Max(x => x.DisplayOrder))
                                                                                      .FirstOrDefaultAsync();
            }
            studentProgressReport.CurrentPosition = string.Concat(lesson?.Name, "_", unitResult.Unit?.Name);
        }
    }
}
