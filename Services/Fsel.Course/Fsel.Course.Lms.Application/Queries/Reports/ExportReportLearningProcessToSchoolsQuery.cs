// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StorageServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using Refit;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportReportLearningProcessToSchoolsQuery : IRequest<MethodResult<Stream>>
    {
        public string? FileName { get; set; }
        public string? EventCodeStr { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? DistrictName { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }

    public class ExportReportLearningProcessToSchoolsQueryHandler : IRequestHandler<ExportReportLearningProcessToSchoolsQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IStorageService _storageService;
        private const int RowExportReport = 5;

        public ExportReportLearningProcessToSchoolsQueryHandler(
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IServiceProvider serviceProvider,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ILessonResultRepository lessonResultRepository,
            IStorageService storageService)
        {
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _serviceProvider = serviceProvider;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _storageService = storageService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportLearningProcessToSchoolsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventSchoolAsync(new GetReportCompetitionEventQueryModel
            {
                DistrictName = request.DistrictName,
                EventCodeStr = request.EventCodeStr,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new List<ReportPlacementTestEventModel>();

            var studentIdsSet = reportCompetitionEvents.SelectMany(x => x.StudentIds ?? new List<Guid>()).ToHashSet();

            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            if (request.CourseLevel.HasValue)
            {
                courseLevels = courseLevels.Where(x => x == request.CourseLevel.Value).ToList();
            }
            var courseStudentIds = await _courseResultRepository.Queryable.WhereBulkContains(studentIdsSet.ToList(), x => x.StudentId)
                                                                          .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                                          .Where(x => x.Course != null && courseLevels.Contains(x.Course.CourseLevel))
                                                                          .Select(x => x.StudentId)
                                                                          .ToListAsync(cancellationToken);

            var placementTestResultGroups = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIdsSet.ToList(), x => x.StudentId)
                                                     .Where(x => x.Status == EnumResultStatus.Done)
                                                     .Select(x => new PlacementTestResultReportGroupModel
                                                     {
                                                         CourseLevel = x.SuggetLevel,
                                                         StudentId = x.StudentId,
                                                     })
                                                     .ToListAsync(cancellationToken);

            var studentToLearns = await (from baseQ in _courseResultRepository.Queryable.WhereBulkContains(courseStudentIds, x => x.StudentId)
                                         join lr in _lessonResultRepository.Queryable on new { baseQ.CourseId, baseQ.StudentId } equals new { lr.CourseId, lr.StudentId }
                                         where baseQ.WorkingStatus == EnumWorkingStatus.Active
                                         select baseQ.StudentId).Distinct().ToListAsync(cancellationToken);

            var studentToLearnHashSet = studentToLearns.ToHashSet();
            var placementTestResultReports = placementTestResultGroups.Select(x => x.StudentId).ToHashSet();
            var resultStudentIds = courseStudentIds.ToHashSet();
            foreach (var reportCompetitionEvent in reportCompetitionEvents)
            {
                var studentDistrictIdsSet = reportCompetitionEvent.StudentIds?.ToHashSet() ?? new HashSet<Guid>();
                var placementTestResultDistrictReports = placementTestResultReports.Where(x => studentDistrictIdsSet.Contains(x));
                var studentResultDistrictIds = resultStudentIds.Where(x => studentDistrictIdsSet.Contains(x)).ToHashSet();
                var studentLearnHashSet = studentToLearnHashSet.Where(x => studentDistrictIdsSet.Contains(x)).ToHashSet();

                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    ReportPlacementTestEventSchools = new List<ReportPlacementTestEventSchoolModel>()
                };
                var reportPlacementTestEventSchools = new List<ReportPlacementTestEventSchoolModel>();
                foreach (var eventSchool in reportCompetitionEvent.ReportCompetitionEventSchools)
                {
                    var eventStudentIdsSet = eventSchool.StudentIds?.ToHashSet() ?? new HashSet<Guid>();
                    var placementTestResultSchools = placementTestResultDistrictReports.Where(studentId => eventStudentIdsSet.Contains(studentId)).Distinct().Count();
                    var studentResultSchoolIds = studentResultDistrictIds.Where(studentId => eventStudentIdsSet.Contains(studentId)).Distinct().ToList();

                    var studentLearnSchool = studentLearnHashSet.Where(x => eventStudentIdsSet.Contains(x)).Distinct().ToList();

                    var learningProgressLearns = await GetStudyPositionAsync(request.CourseType, studentResultSchoolIds);
                    reportPlacementTestEventSchools.Add(new ReportPlacementTestEventSchoolModel
                    {
                        SchoolName = eventSchool.SchoolName,
                        TotalStudentToLearn = studentLearnSchool.Count,
                        NumberStudentCompleteVerify = eventSchool.NumberStudentCompleteVerify,
                        NumberStudentVerifiedSchool = eventSchool.NumberStudentCompleteVerify,
                        NumberStudentsCompletedPT = placementTestResultSchools,
                        NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                        NumberStudentToLearn = studentResultSchoolIds.Count,
                        LearningProgressLearns = learningProgressLearns,
                    });
                }
                reportPlacementTestEvent.ReportPlacementTestEventSchools = reportPlacementTestEventSchools;
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            }
            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents.ToList(), request);
            await UploadFileExcel(methodResult.Result, request.FileName);
            return methodResult;
        }

        private async Task UploadFileExcel(Stream stream, string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            var filePart = new StreamPart(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            await _storageService.UpLoadFile(EnumFolderType.Files, EnumBucketType.FselPublic, filePart, isAddSuffix: false);
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportLearningProcessToSchoolsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportLearningSchoolEvent)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                TemplateExecel(excelWorksheet, request.CourseType, request.CourseLevel);

                int startRow = 6;
                int indexReport = 1;
                var dataRows = new List<object[]>();
                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    foreach (var item in reportPlacementTestEvents)
                    {
                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            var row = new List<object>
                            {
                                indexReport++,
                                item.LocationName ?? string.Empty,
                                reportPt.SchoolName ?? string.Empty,
                                reportPt.TotalStudentAccount,
                                reportPt.NumberStudentsCompletedPT,
                                reportPt.CompletionRate + "%",
                                reportPt.NumberStudentToLearn,
                                reportPt.TotalStudentToLearn
                            };
                            if (reportPt.LearningProgressLearns != null)
                            {
                                foreach (var learningProgress in reportPt.LearningProgressLearns)
                                {
                                    row.Add(learningProgress.StudentCount);
                                    row.Add(NumberHelper.GetPercent(learningProgress.StudentCount, reportPt.NumberStudentToLearn) + "%");
                                }
                            }
                            dataRows.Add(row.ToArray());
                        }
                    }
                }
                excelWorksheet.Cells[startRow, 1].LoadFromArrays(dataRows);
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void TemplateExecel(ExcelWorksheet excelWorksheet, EnumCourseType courseType, EnumCourseLevel? courseLevel = null)
        {
            excelWorksheet.Cells["G4"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["G4"].Value, $"{courseType.GetDescription()}");
            excelWorksheet.Cells["H4"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["H4"].Value, $"{courseType.GetDescription()}");

            int startColumn = 9;
            int mergeRangeCount = courseType == EnumCourseType.Academic ? (CourseProgressValue.CountUnitAca * CourseProgressValue.CountLessonAca + CourseProgressValue.CountFinalTest) * 2
                : courseType == EnumCourseType.Ielts ? (CourseProgressValue.CountUnitIELTS * CourseProgressValue.CountLessonIELTS + CourseProgressValue.CountUnitIELTS + CourseProgressValue.CountFullMockTest) * 2
                : courseType == EnumCourseType.EnglishFoundation && courseLevel.HasValue && courseLevel.Value == EnumCourseLevel.EFA1 ? (CourseProgressValue.CountUnitRFIA1 * CourseProgressValue.CountLessonRFI + CourseProgressValue.CountFinalTest) * 2
                : (CourseProgressValue.CountUnitRFIA2 * CourseProgressValue.CountLessonRFI + CourseProgressValue.CountFinalTest) * 2;

            // Tạo danh sách các dải ô cần merge và thêm đường viền
            List<(int row, int startCol, int endCol)> ranges = new()
            {
                (2, 1, startColumn + mergeRangeCount - 1),
                (3, startColumn, startColumn + mergeRangeCount - 1),
                (4, startColumn, startColumn + mergeRangeCount - 1),
            };

            foreach (var (row, startCol, endCol) in ranges)
            {
                string startCell = ExcelReportHelper.GetExcelColumnName(startCol) + row;
                string endCell = ExcelReportHelper.GetExcelColumnName(endCol) + row;
                string range = $"{startCell}:{endCell}";

                excelWorksheet.Cells[range].Merge = true;

                var border = excelWorksheet.Cells[range].Style.Border;
                border.Top.Style = ExcelBorderStyle.Thin;
                border.Bottom.Style = ExcelBorderStyle.Thin;
                border.Left.Style = ExcelBorderStyle.Thin;
                border.Right.Style = ExcelBorderStyle.Thin;
            }

            int dem = courseType == EnumCourseType.Ielts ? 11 : 13;
            if (courseType == EnumCourseType.Ielts)
            {
                var courseSkillMockTest = 0;
                int fullMockTestCounter = 0;
                for (int i = 5; i < mergeRangeCount; i++)
                {
                    if ((i - fullMockTestCounter * 2) % 10 == 0)
                    {
                        AddSkillMockTest(excelWorksheet, ref dem, ref i, ref courseSkillMockTest);
                        if (courseSkillMockTest % 4 == 0)
                        {
                            AddFullMockTest(excelWorksheet, ref dem, ref i, ref fullMockTestCounter);
                        }
                    }
                    else
                    {
                        AddLesson(excelWorksheet, ref dem, i, courseSkillMockTest, fullMockTestCounter);
                    }
                }
            }
            else
            {
                for (int i = 7; i < mergeRangeCount; i++)
                {
                    ExcelReportHelper.ProcessCell(excelWorksheet, RowExportReport, dem - 1, dem + 1, $"{nameof(Lesson)} {i / 2}");
                    dem++;
                }

                for (int i = 0; i < CourseProgressValue.CountFinalTest * 2; i++)
                {
                    ExcelReportHelper.ProcessCell(excelWorksheet, RowExportReport, dem - 1, dem + 1, nameof(FinalTest));
                    dem++;
                }
            }
        }

        // Thêm SkillMockTest
        private static void AddSkillMockTest(ExcelWorksheet worksheet, ref int dem, ref int i, ref int courseSkillMockTest)
        {
            for (int j = 0; j < 2; j++)
            {
                ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem - 1, dem + 1, $"{nameof(EnumMockTestType.SkillMockTest)} {i / 10}");
                dem++;
            }
            i++;
            courseSkillMockTest++;
        }

        private static void AddLesson(ExcelWorksheet worksheet, ref int dem, int i, int courseSkillMockTest, int fullMockTestCounter)
        {
            ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem - 1, dem + 1, $"{nameof(Lesson)} {i / 2 - courseSkillMockTest - fullMockTestCounter}");
            dem++;
        }

        // Thêm FullMockTest
        private static void AddFullMockTest(ExcelWorksheet worksheet, ref int dem, ref int i, ref int fullMockTestCounter)
        {
            fullMockTestCounter++;
            for (int j = 0; j < 2; j++, i++)
            {
                ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem - 1, dem + 1, $"{nameof(EnumMockTestType.FullMockTest)} {fullMockTestCounter}");
                dem++;
            }
        }

        public async Task<IList<LearningProgressLearnModel>> GetStudyPositionAsync(EnumCourseType courseType, IList<Guid> studentIds)
        {
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(courseType);
            var listLearningProcess = new List<LearningProgressLearnModel>();

            if (!studentIds.Any())
            {
                return listLearningProcess;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();
                var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                var unitSkillMockTestRepository = scope.ServiceProvider.GetRequiredService<IUnitSkillMockTestRepository>();
                var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                if (courseType == EnumCourseType.Academic || courseType == EnumCourseType.EnglishFoundation)
                {
                    var query = (from baseQ in courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)

                                 join courseUnitMockTest in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals courseUnitMockTest.CourseId

                                 join ftr in finalTestResultRepository.Queryable
                                  on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = courseUnitMockTest.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
                                 from ftr in ftrGroup.DefaultIfEmpty()

                                 join ul in unitLessonRepository.Queryable
                                 on courseUnitMockTest.UnitId equals (Guid?)ul.UnitId into ulGroup
                                 from ul in ulGroup.DefaultIfEmpty()

                                 join lr in lessonResultRepository.Queryable
                                 on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
                                 from lr in lrGroup.DefaultIfEmpty()
                                 where baseQ.WorkingStatus == EnumWorkingStatus.Active && courseUnitMockTest.MockTestId == null
                                 group new { ftr, lr } by new { DisplayOrder = (int?)ul.DisplayOrder, UnitDisplayOrder = courseUnitMockTest.DisplayOrder } into groupedData
                                 select new LearningProgressLearnModel
                                 {
                                     DisplayOrder = groupedData.Key.DisplayOrder,
                                     UnitDisplayOrder = groupedData.Key.UnitDisplayOrder,
                                     StudentCount = groupedData.Count(g =>
                                         (g.ftr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.ftr.Status)) ||
                                         (g.lr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.lr.Status)))
                                 }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder);

                    var learningProgress = await (query).ToListAsync(CancellationToken.None);
                    listLearningProcess.AddRange(learningProgress);
                }
                else if (courseType == EnumCourseType.Ielts)
                {
                    var learningProgress = await (from baseQ in courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                                  join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                                                  join ul in unitLessonRepository.Queryable
                                                  on cum.UnitId equals (Guid?)ul.UnitId into ulGroup
                                                  from ul in ulGroup.DefaultIfEmpty()

                                                  join lr in lessonResultRepository.Queryable
                                                  on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
                                                  from lr in lrGroup.DefaultIfEmpty()

                                                  where baseQ.WorkingStatus == EnumWorkingStatus.Active && cum.UnitId.HasValue
                                                  group lr by new { DisplayOrder = cum.DisplayOrder, LessonDisplayOrder = (int?)ul.DisplayOrder } into g
                                                  select new LearningProgressLearnModel
                                                  {
                                                      UnitDisplayOrder = g.Key.DisplayOrder,
                                                      DisplayOrder = g.Key.LessonDisplayOrder,
                                                      StudentCount = g.Count(x => (x != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(x.Status))),
                                                  }).Concat(from baseQ in courseResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)

                                                            join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                                                            join usm in unitSkillMockTestRepository.Queryable on new { UnitId = cum.UnitId } equals new { UnitId = (Guid?)usm.UnitId } into usmGroup
                                                            from usm in usmGroup.DefaultIfEmpty()

                                                            join mtrs in mockTestResultRepository.Queryable on new { baseQ.StudentId, UnitId = (Guid?)usm.UnitId, baseQ.CourseId, usm.MockTestId } equals new { mtrs.StudentId, UnitId = mtrs.UnitId, mtrs.CourseId, mtrs.MockTestId } into mtrsGroup
                                                            from mtrs in mtrsGroup.DefaultIfEmpty()

                                                            join mtr in mockTestResultRepository.Queryable on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                                                            from mtr in mtrGroup.DefaultIfEmpty()

                                                            where baseQ.WorkingStatus == EnumWorkingStatus.Active && cum.FinalTestId == null
                                                            group new { mtr, mtrs } by new { cum.DisplayOrder } into g
                                                            select new LearningProgressLearnModel
                                                            {
                                                                UnitDisplayOrder = g.Key.DisplayOrder,
                                                                DisplayOrder = null,
                                                                StudentCount = g.Count(x =>
                                                                (x.mtr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(x.mtr.Status)) ||
                                                                (x.mtrs != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(x.mtrs.Status)))
                                                            }).OrderBy(r => r.UnitDisplayOrder)
                                                              .OrderBy(r => r.DisplayOrder.HasValue ? 0 : 1)
                                                              .OrderBy(r => r.DisplayOrder)
                                                              .ThenByDescending(r => r.StudentCount)
                                                              .ToListAsync(CancellationToken.None);

                    listLearningProcess.AddRange(learningProgress);
                }
            }

            return listLearningProcess.GroupBy(x => new { x.DisplayOrder, x.UnitDisplayOrder }).Select(x => new LearningProgressLearnModel
            {
                DisplayOrder = x.Key.DisplayOrder,
                UnitDisplayOrder = x.Key.UnitDisplayOrder,
                StudentCount = x.Sum(y => y.StudentCount),
            }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder.HasValue ? 0 : 1).ThenBy(x => x.DisplayOrder).ToList();
        }
    }
}
