// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Collections.Concurrent;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportReportLearningProcessToSchoolsQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? DistrictName { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportLearningProcessToSchoolsQueryHandler : IRequestHandler<ExportReportLearningProcessToSchoolsQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IServiceProvider _serviceProvider;
        private const int RowExportReport = 5;

        public ExportReportLearningProcessToSchoolsQueryHandler(
            IMapper mapper,
            IUserService userService,
            ICourseResultRepository courseResultRepository,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportLearningProcessToSchoolsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventSchoolAsync(new GetReportCompetitionEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                DistrictName = request.DistrictName,
                EventCodeStr = request.EventCodeStr,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new ConcurrentBag<ReportPlacementTestEventModel>();
            var placementTestResultGroups = new ConcurrentBag<PlacementTestResultReportGroupModel>();
            var courseStudentResults = new ConcurrentBag<CourseResultModel>();

            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();

            int batchSize = 500; // Số lượng bản ghi mỗi lần truy vấn

            // Chia danh sách thành từng nhóm
            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            if (request.CourseLevel.HasValue)
            {
                courseLevels = courseLevels.Where(x => x == request.CourseLevel.Value).ToList();
            }
            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseResults = await courseResultRepository.Queryable.Where(x => batche.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active)
                                                                              .Where(x => x.Course != null && courseLevels.Contains(x.Course.CourseLevel))
                                                                              .ToListAsync(cancellationToken);
                    foreach (var courseResult in _mapper.Map<IList<CourseResultModel>>(courseResults))
                    {
                        courseStudentResults.Add(courseResult);
                    }
                }
            });
            // Thực hiện truy vấn từng nhóm
            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var placementTestResultRepository = scope.ServiceProvider.GetRequiredService<IPlacementTestResultRepository>();
                    var placementTestGroups = await placementTestResultRepository.Queryable
                        .Where(x => batche.Contains(x.StudentId) && x.Status == EnumResultStatus.Done)
                        .GroupBy(x => x.StudentId)
                        .Select(x => new PlacementTestGroupStudentResultModel
                        {
                            StudentId = x.Key,
                            PlacementTestStart = _mapper.Map<PlacementTestResultModel>(x.Select(x => x).OrderBy(x => x.CreatedDate).FirstOrDefault()),
                            PlacementTestEnd = _mapper.Map<PlacementTestResultModel>(x.Select(x => x).OrderByDescending(x => x.CreatedDate).FirstOrDefault()),
                        })
                        .ToListAsync(cancellationToken);
                    var placementTestResultReports = placementTestGroups?.Select(item =>
                    {
                        var placementTestResultEnd = item.PlacementTestEnd;
                        var placementTestResultStart = item.PlacementTestStart;
                        if (placementTestResultEnd != null)
                        {
                            var (levelCompleted, isLock) = placementTestResultEnd.Level.GetLevelInScore(placementTestResultEnd.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultStart?.Level, default));
                            return new PlacementTestResultReportGroupModel { StudentId = item.StudentId, IsDonePT = isLock, CourseLevel = levelCompleted };
                        }
                        return new PlacementTestResultReportGroupModel { StudentId = item.StudentId };
                    }).ToList() ?? new List<PlacementTestResultReportGroupModel>();
                    foreach (var item in placementTestResultReports)
                    {
                        placementTestResultGroups.Add(item);
                    }
                }
            });

            await Parallel.ForEachAsync(reportCompetitionEvents, async (reportCompetitionEvent, cancellationToken) =>
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).ToList();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default,
                    NumberStudentToLearn = courseStudentResults.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).Select(x => x.StudentId).Count(),
                };

                await Parallel.ForEachAsync(reportCompetitionEvent.ReportCompetitionEventSchools, async (eventSchool, cancellationToken) =>
                {
                    var placementTestResultSchools = placementTestResultReports?.Where(x => eventSchool.StudentIds != null && eventSchool.StudentIds.Contains(x.StudentId)).ToList();
                    var studentIds = courseStudentResults.Where(x => eventSchool.StudentIds != null && eventSchool.StudentIds.Contains(x.StudentId)).Select(x => x.StudentId).ToList();

                    reportPlacementTestEvent.ReportPlacementTestEventSchools.Add(new ReportPlacementTestEventSchoolModel
                    {
                        SchoolName = eventSchool.SchoolName,
                        NumberStudentsCompletedPT = placementTestResultSchools?.Where(x => x.IsDonePT).Count() ?? default,
                        NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                        NumberStudentToLearn = studentIds.Count,
                        LearningProgressLearns = await GetStudyPositionAsync(request.CourseType, studentIds),
                    });
                });
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            });
            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents.ToList(), request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportLearningProcessToSchoolsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            var courseType = request.CourseType;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportLearningSchoolEvent)))
            {
                var originalWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (originalWorksheet == null)
                {
                    throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                }
                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    foreach (var item in reportPlacementTestEvents)
                    {
                        var excelWorksheet = excelPackage.Workbook.Worksheets.Copy(originalWorksheet.Name, item.LocationName);
                        excelWorksheet.Cells["A2"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["A2"].Value, $"{request.EducationLevel.GetDescription()}");
                        excelWorksheet.Cells["F4"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["F4"].Value, $"{request.CourseType.GetDescription()}");

                        int startColumn = 7;
                        int mergeRangeCount = courseType == EnumCourseType.Academic ? (CourseProgressValue.CountUnitAca * CourseProgressValue.CountLessonAca + CourseProgressValue.CountFinalTest) * 2
                            : courseType == EnumCourseType.Ielts ? (CourseProgressValue.CountUnitIELTS * CourseProgressValue.CountLessonIELTS + CourseProgressValue.CountUnitIELTS + CourseProgressValue.CountFullMockTest) * 2
                            : default;

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

                        int dem = courseType == EnumCourseType.Ielts ? 13 : 15;
                        if (courseType == EnumCourseType.Ielts)
                        {
                            var courseSkillMockTest = 0;
                            int fullMockTestCounter = 0;
                            for (int i = 9; i < mergeRangeCount; i++)
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
                            for (int i = 11; i < mergeRangeCount; i++)
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
                        int startRow = 6;
                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            excelWorksheet.Cells[startRow, 1].Value = item.ReportPlacementTestEventSchools.IndexOf(reportPt) + 1;
                            excelWorksheet.Cells[startRow, 2].Value = reportPt.SchoolName;
                            excelWorksheet.Cells[startRow, 3].Value = reportPt.NumberValidStudentAccount;
                            excelWorksheet.Cells[startRow, 4].Value = reportPt.NumberStudentsCompletedPT;
                            excelWorksheet.Cells[startRow, 5].Value = reportPt.CompletionRate + "%";
                            excelWorksheet.Cells[startRow, 6].Value = reportPt.NumberStudentToLearn;
                            if (reportPt.LearningProgressLearns != null)
                            {
                                var rowReportLevel = 7;
                                foreach (var learningProgress in reportPt.LearningProgressLearns)
                                {
                                    excelWorksheet.Cells[startRow, rowReportLevel].Value = learningProgress.StudentCount;
                                    excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = NumberHelper.GetPercent(learningProgress.StudentCount, reportPt.NumberStudentToLearn) + "%";
                                    rowReportLevel += 2;
                                }
                            }
                            startRow++;
                        }
                    }
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
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

            int batchSize = 500; // Số lượng bản ghi mỗi lần truy vấn

            // Chia danh sách thành từng nhóm
            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();
            var listLearningProcess = new ConcurrentBag<LearningProgressLearnModel>();

            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                    var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                    var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();
                    var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                    var unitSkillMockTestRepository = scope.ServiceProvider.GetRequiredService<IUnitSkillMockTestRepository>();
                    var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                    if (courseType == EnumCourseType.Academic)
                    {
                        var learningProgress = await (from baseQ in courseResultRepository.Queryable

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
                                                      where baseQ.WorkingStatus == EnumWorkingStatus.Active && batche.Contains(baseQ.StudentId)
                                                      group new { ul, courseUnitMockTest, ftr, lr } by new { DisplayOrder = (int?)ul.DisplayOrder, UnitDisplayOrder = courseUnitMockTest.DisplayOrder } into groupedData
                                                      select new LearningProgressLearnModel
                                                      {
                                                          DisplayOrder = groupedData.Key.DisplayOrder,
                                                          UnitDisplayOrder = groupedData.Key.UnitDisplayOrder,
                                                          StudentCount = groupedData.Count(g =>
                                                              (g.ftr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.ftr.Status)) ||
                                                              (g.lr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(g.lr.Status)))
                                                      }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder).ToListAsync(CancellationToken.None);
                        foreach (var item in learningProgress)
                        {
                            listLearningProcess.Add(item);
                        }
                    }
                    else if (courseType == EnumCourseType.Ielts)
                    {
                        var learningProgress = await (from baseQ in courseResultRepository.Queryable
                                                      join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                                                      join ul in unitLessonRepository.Queryable
                                                      on cum.UnitId equals (Guid?)ul.UnitId into ulGroup
                                                      from ul in ulGroup.DefaultIfEmpty()

                                                      join lr in lessonResultRepository.Queryable
                                                      on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
                                                      from lr in lrGroup.DefaultIfEmpty()

                                                      where
                                                          baseQ.WorkingStatus == EnumWorkingStatus.Active && batche.Contains(baseQ.StudentId) &&
                                                          cum.UnitId.HasValue &&
                                                          !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(lr.Status)
                                                      group lr by new { DisplayOrder = cum.DisplayOrder, LessonDisplayOrder = (int?)ul.DisplayOrder } into g
                                                      select new LearningProgressLearnModel
                                                      {
                                                          UnitDisplayOrder = g.Key.DisplayOrder,
                                                          DisplayOrder = g.Key.LessonDisplayOrder,
                                                          StudentCount = g.Count(),
                                                      }).Concat(from baseQ in courseResultRepository.Queryable

                                                                join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                                                                join usm in unitSkillMockTestRepository.Queryable
                                                                on new { UnitId = cum.UnitId } equals new { UnitId = (Guid?)usm.UnitId } into usmGroup
                                                                from usm in usmGroup.DefaultIfEmpty()

                                                                join mtrs in mockTestResultRepository.Queryable
                                                                on new { baseQ.StudentId, UnitId = (Guid?)usm.UnitId, baseQ.CourseId, usm.MockTestId } equals new { mtrs.StudentId, UnitId = mtrs.UnitId, mtrs.CourseId, mtrs.MockTestId } into mtrsGroup
                                                                from mtrs in mtrsGroup.DefaultIfEmpty()

                                                                join mtr in mockTestResultRepository.Queryable
                                                                on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                                                                from mtr in mtrGroup.DefaultIfEmpty()

                                                                where baseQ.WorkingStatus == EnumWorkingStatus.Active && batche.Contains(baseQ.StudentId)
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

                        foreach (var item in learningProgress)
                        {
                            listLearningProcess.Add(item);
                        }
                    }
                }
            });

            var a = listLearningProcess.GroupBy(x => new { x.DisplayOrder, x.UnitDisplayOrder }).Select(x => new LearningProgressLearnModel
            {
                DisplayOrder = x.Key.DisplayOrder,
                UnitDisplayOrder = x.Key.UnitDisplayOrder,
                StudentCount = x.Sum(y => y.StudentCount),
            }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder.HasValue ? 0 : 1).ThenBy(x => x.DisplayOrder).ToList();

            return a;
        }
    }
}
