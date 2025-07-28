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
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

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
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public ExportFileProgressStudentsToEmailsQueryHandler(IUserService userService,
            ICourseResultRepository courseResultRepository,
            ISystemService systemService,
            ManagerProgressHelper managerProgressHelper,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ILessonRepository lessonRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository)
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
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
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
                if (string.IsNullOrEmpty(x.Email) || !x.Email.IsValidEmail())
                {
                    errors.Add(new ValidateExcelModel { RowIndex = rowIndex, ColumnName = nameof(x.Email), Message = "Email is null or malformed" });
                }
                return await Task.FromResult(errors.Count == 0);
            });
            result.Datas = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email) && x.Email.IsValidEmail()).Distinct().ToList();
            if (result.Stream != null)
            {
                methodResult.Result = result.Stream;
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var listEmail = result.Datas.Where(x => !string.IsNullOrEmpty(x.Email)).Select(x => x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture)).ToList();
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
            var studentRankings = new List<StudentRankingModel>();
            if (!string.IsNullOrEmpty(request.EventCode))
            {
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
                studentRankings = studentRankingResults.Content?.Result?.Items?.ToList();
            }

            // 1️⃣ Lấy danh sách CourseResults và ánh xạ vào Dictionary để tra cứu nhanh hơn
            var courseResults = await _courseResultRepository.Queryable
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                .Include(x => x.Course)
                .ToDictionaryAsync(x => x.StudentId, cancellationToken);

            // 2️⃣ Lấy danh sách Placement Test Results và chuyển thành Dictionary
            var placemenTestResultGroups = (await _placementTestGroupResultRepository.Queryable
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Where(x => x.Status == EnumResultStatus.Done)
                .ToListAsync(cancellationToken))
                .ToDictionary(x => x.StudentId);

            // 3️⃣ Lấy danh sách UnitResults theo StudentId bằng ToLookup() (tối ưu hơn GroupBy)
            var unitResultGroups = (await (from baseQ in _courseResultRepository.Queryable
                                           .WhereBulkContains(studentIds, x => x.StudentId)
                                           join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                                           join ur in _unitResultRepository.Queryable
                                               on new { cum.CourseId, baseQ.StudentId, UnitId = cum.UnitId }
                                               equals new { ur.CourseId, ur.StudentId, UnitId = (Guid?)ur.UnitId }
                                           where baseQ.WorkingStatus == EnumWorkingStatus.Active
                                           select new { ur.StudentId, ur })
                                          .ToListAsync(cancellationToken))
                .ToLookup(x => x.StudentId, x => x.ur);

            // 4️⃣ Lấy danh sách Progress Completes
            var courseProgressCompletes = await _managerProgressHelper
                .GetProgressCompleteModuleExportAsync(courseResults
                    .Select(x => new CourseResultModel { CourseId = x.Value.CourseId, StudentId = x.Key })
                    .ToList());

            // 5️⃣ Lấy danh sách LessonResults
            var lessonResultIds = courseProgressCompletes
                .Where(x => x.LessonResult != null)
                .Select(x => x.LessonResult.Id)
                .ToList();

            var lessonResults = (await _lessonResultRepository.Queryable.WhereBulkContains(lessonResultIds, x => x.Id)
                                                                .Select(x => new { x.StudentId, LessonName = x.Lesson != null ? x.Lesson.Name : string.Empty })
                                                                .ToListAsync(cancellationToken))
                                                                .ToDictionary(x => x.StudentId);

            // 🔥 Bắt đầu vòng lặp chính
            foreach (var student in students)
            {
                var courseResult = courseResults.GetValueOrDefault(student.Id);
                var courseComplete = courseProgressCompletes.FirstOrDefault(x => x.StudentId == student.Id);
                var placemenTestResultGroup = placemenTestResultGroups.GetValueOrDefault(student.Id);
                var unitResults = unitResultGroups[student.Id]?.OrderBy(x => x.CreatedDate).ToList();
                var lessonResult = lessonResults.GetValueOrDefault(student.Id);

                var studentProgressReport = new StudentProgressReportModel
                {
                    FullName = student.Human?.FullName,
                    Email = student.Human?.Email,
                    School = student.School ?? schools?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name,
                    ProcessDate = courseResult?.ProcessDate,
                    ExpiredDate = student.ExpiredDate,
                    CourseName = student.ExpiredDate.HasValue
                        ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(courseResult?.Course?.CourseLevel)
                        : string.Empty,
                    StatusUser = courseResult != null ? ValueStatusUser.InProgress :
                                 placemenTestResultGroup != null ? ValueStatusUser.CompletedPlacementTest :
                                 ValueStatusUser.NotCompletedPlacementTest
                };

                if (unitResults != null && unitResults.Any())
                {
                    SetOverallPercentUnitAsync(studentProgressReport, unitResults);
                }

                if (courseComplete != null)
                {
                    studentProgressReport.TotalLessonDone = courseComplete.TotalLessonDone;
                    studentProgressReport.ProgressPercent = NumberHelper.GetPercent(courseComplete.CountComplete, courseComplete.TotalComplete);
                    studentProgressReport.CurrentPosition = lessonResult == null
                        ? $"Unit{courseComplete.UnitDisplayOrder}"
                        : $"{lessonResult.LessonName}_Unit{courseComplete.UnitDisplayOrder}";
                }

                if (courseResult != null)
                {
                    await SetFeatureAccessTimeAsync(studentProgressReport, student, courseResult);
                    studentProgressReport.LeaderboardPercent = studentRankings?
                        .FirstOrDefault(x => x.StudentId == student.Id && x.CourseResultId == courseResult.Id)?
                        .OverallScore ?? default;
                }

                studentProgressReports.Add(studentProgressReport);
            }

            methodResult.Result = studentProgressReports.OrderBy(x => listEmail.IndexOf(x.Email!.ToLower(System.Globalization.CultureInfo.CurrentCulture))).ToList().ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetFeatureAccessTimeAsync(StudentProgressReportModel studentProgressReport, StudentModel student, CourseResult courseResult)
        {
            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimeBusiness(new GetFeatureAccessTimeBusinessQueryModel
            {
                UserId = student.Human?.UserId ?? default,
                CourseId = courseResult.CourseId
            });

            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                return;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result;
            studentProgressReport.LearnTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Learn).Sum(p => p.AccessTime) ?? default;
            studentProgressReport.SocialTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Social).Sum(p => p.AccessTime) ?? default;
            studentProgressReport.OtherTime = featureAccessTimes?.Where(x => x.FeatureBusinessType == EnumFeatureBussinessType.Other).Sum(p => p.AccessTime) ?? default;
            studentProgressReport.TotalVisit = featureAccessTimes?.Sum(x => x.TotalVisit) ?? default;
        }

        private static void SetOverallPercentUnitAsync(StudentProgressReportModel studentProgressReport, IList<UnitResult>? unitResults)
        {
            if (unitResults == null || !unitResults.Any())
            {
                return;
            }
            foreach (var unitResult in unitResults)
            {
                var index = unitResults.IndexOf(unitResult);
                var unitField = typeof(StudentProgressReportModel).GetProperty($"OverallUnit{index + 1}");
                if (unitField == null)
                {
                    continue;
                }
                unitField.SetValue(studentProgressReport, unitResult.Percent);
            }
        }
    }
}
