// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
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

    public class ExportReportLearningProcessToDistrictQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportLearningProcessToDistrictQueryHandler : IRequestHandler<ExportReportLearningProcessToDistrictQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IServiceProvider _serviceProvider;
        private const int RowExportReport = 6;

        public ExportReportLearningProcessToDistrictQueryHandler(
            IMapper mapper,
            IUserService userService,
            ILessonResultRepository lessonResultRepository,
            ICourseResultRepository courseResultRepository,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportLearningProcessToDistrictQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventAsync(new GetReportCompetitionEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                EventCodeStr = request.EventCodeStr,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new List<ReportPlacementTestEventModel>();
            var courseStudentResults = new List<CourseResultModel>();

            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList() ?? new List<Guid>();
            var placementTestResultGroups = new List<PlacementTestResultReportGroupModel>();

            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / BatchSize1000)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);
            if (request.CourseLevel.HasValue)
            {
                courseLevels = courseLevels.Where(x => x == request.CourseLevel.Value).ToList();
            }
            foreach (var batche in batches)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseResults = await courseResultRepository.Queryable.WhereBulkContains(batche, x => x.StudentId)
                                                                              .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                                              .Where(x => x.Course != null && courseLevels.Contains(x.Course.CourseLevel))
                                                                              .Select(x => _mapper.Map<CourseResultModel>(x))
                                                                              .ToListAsync(cancellationToken);

                    courseStudentResults.AddRange(courseResults);
                }
            };

            foreach (var batche in batches)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var placementTestGroupResultRepository = scope.ServiceProvider.GetRequiredService<IPlacementTestGroupResultRepository>();
                    var placementTestGroups = await placementTestGroupResultRepository.Queryable.WhereBulkContains(batche, x => x.StudentId)
                                                    .Where(x => x.Status == EnumResultStatus.Done)
                                                    .Select(x => new PlacementTestResultReportGroupModel
                                                    {
                                                        CourseLevel = x.SuggetLevel,
                                                        StudentId = x.StudentId,
                                                        IsDonePT = true
                                                    })
                                                    .ToListAsync(cancellationToken);
                    placementTestResultGroups.AddRange(placementTestGroups);
                }
            };

            foreach (var reportCompetitionEvent in reportCompetitionEvents)
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).Select(x => x.StudentId).Distinct().ToList();
                var studentDistrictIds = courseStudentResults.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).Select(x => x.StudentId).Distinct().ToList();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Count ?? default,
                    NumberStudentToLearn = studentDistrictIds.Count,
                    LearningProgressLearns = await GetStudyPositionAsync(request.CourseType, studentIds),
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            };

            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents, request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportLearningProcessToDistrictQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            var courseType = request.CourseType;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportLearningDistrictEvent)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["A3"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["A3"].Value, $"{request.EducationLevel.GetDescription()}");
                excelWorksheet.Cells["I5"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["I5"].Value, $"{request.CourseType.GetDescription()}");

                int startColumn = 10;
                int mergeRangeCount = courseType == EnumCourseType.Academic ? (CourseProgressValue.CountUnitAca * CourseProgressValue.CountLessonAca + CourseProgressValue.CountFinalTest) * 2
                    : courseType == EnumCourseType.Ielts ? (CourseProgressValue.CountUnitIELTS * CourseProgressValue.CountLessonIELTS + CourseProgressValue.CountUnitIELTS + CourseProgressValue.CountFullMockTest) * 2
                    : default;

                // Tạo danh sách các dải ô cần merge và thêm đường viền
                List<(int row, int startCol, int endCol)> ranges = new()
                {
                    (3, 1, startColumn + mergeRangeCount - 1),
                    (4, startColumn, startColumn + mergeRangeCount - 1),
                    (5, startColumn, startColumn + mergeRangeCount - 1)
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

                int dem = courseType == EnumCourseType.Ielts ? 17 : 19;
                if (courseType == EnumCourseType.Ielts)
                {
                    var courseSkillMockTest = 0;
                    int fullMockTestCounter = 0;
                    for (int i = 10; i < mergeRangeCount; i++)
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
                    for (int i = 12; i < mergeRangeCount; i++)
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

                int startRow = 7;
                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    foreach (var (item, index) in reportPlacementTestEvents.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.LocationName;
                        excelWorksheet.Cells[startRow, 3].Value = item.NumberRegisteredSchool;
                        excelWorksheet.Cells[startRow, 4].Value = item.NumberActualParticipatingSchool;
                        excelWorksheet.Cells[startRow, 5].Value = item.ActualSchoolParticipationRate + "%";
                        excelWorksheet.Cells[startRow, 6].Value = item.NumberValidStudentAccount;
                        excelWorksheet.Cells[startRow, 7].Value = item.NumberStudentsCompletedPT;
                        excelWorksheet.Cells[startRow, 8].Value = item.CompletionRate + "%";
                        excelWorksheet.Cells[startRow, 9].Value = item.NumberStudentToLearn;
                        if (item.LearningProgressLearns != null)
                        {
                            var rowReportLevel = 10;
                            foreach (var learningProgress in item.LearningProgressLearns)
                            {
                                excelWorksheet.Cells[startRow, rowReportLevel].Value = learningProgress.StudentCount;
                                excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = NumberHelper.GetPercent(learningProgress.StudentCount, item.NumberStudentToLearn) + "%";
                                rowReportLevel += 2;
                            }
                        }
                        startRow++;
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
            int batchSize = 500; // Số lượng bản ghi mỗi lần truy vấn

            // Chia danh sách thành từng nhóm
            var batches = studentIds.Select((id, index) => new { id, index })
                                    .GroupBy(x => x.index / batchSize)
                                    .Select(g => g.Select(x => x.id).ToList())
                                    .ToList();

            var listLearningProcess = new ConcurrentBag<LearningProgressLearnModel>();

            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseRepository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
                    var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                    var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                    var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();
                    var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                    var unitSkillMockTestRepository = scope.ServiceProvider.GetRequiredService<IUnitSkillMockTestRepository>();
                    var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                    if (courseType == EnumCourseType.Academic)
                    {
                        var query = from baseQ in courseResultRepository.Queryable
                                    join course in courseRepository.Queryable on baseQ.CourseId equals course.Id

                                    join courseUnitMockTest in courseUnitMockTestRepository.Queryable on course.Id equals courseUnitMockTest.CourseId

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
                                    };

                        var learningProgress = await query.OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder).ToListAsync(CancellationToken.None);
                        foreach (var item in learningProgress)
                        {
                            listLearningProcess.Add(item);
                        }
                    }
                    else if (courseType == EnumCourseType.Ielts)
                    {
                        var query = (from baseQ in courseResultRepository.Queryable
                                     join cum in courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId

                                     join ul in unitLessonRepository.Queryable
                                     on cum.UnitId equals (Guid?)ul.UnitId into ulGroup
                                     from ul in ulGroup.DefaultIfEmpty()

                                     join lr in lessonResultRepository.Queryable
                                     on new { baseQ.StudentId, ul.UnitId, baseQ.CourseId, ul.LessonId } equals new { lr.StudentId, lr.UnitId, lr.CourseId, lr.LessonId } into lrGroup
                                     from lr in lrGroup.DefaultIfEmpty()

                                     where
                                         baseQ.WorkingStatus == EnumWorkingStatus.Active && batche.Contains(baseQ.StudentId) &&
                                         cum.UnitId.HasValue
                                     group new { lr } by new { DisplayOrder = cum.DisplayOrder, LessonDisplayOrder = (int?)ul.DisplayOrder } into g
                                     select new LearningProgressLearnModel
                                     {
                                         UnitDisplayOrder = g.Key.DisplayOrder,
                                         DisplayOrder = g.Key.LessonDisplayOrder,
                                         StudentCount = g.Count(x => (x.lr != null && !new[] { EnumResultStatus.Done, EnumResultStatus.Unfinished }.Contains(x.lr.Status))),
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
                                               });

                        query = query.OrderBy(r => r.UnitDisplayOrder)
                                                          .OrderBy(r => r.DisplayOrder.HasValue ? 0 : 1)
                                                          .OrderBy(r => r.DisplayOrder)
                                                          .ThenByDescending(r => r.StudentCount);

                        var learningProgress = await query.ToListAsync(CancellationToken.None);

                        foreach (var item in learningProgress)
                        {
                            listLearningProcess.Add(item);
                        }
                    }
                }
            });

            return listLearningProcess.GroupBy(x => new { x.DisplayOrder, x.UnitDisplayOrder }).Select(x => new LearningProgressLearnModel
            {
                DisplayOrder = x.Key.DisplayOrder,
                UnitDisplayOrder = x.Key.UnitDisplayOrder,
                StudentCount = x.Sum(y => y.StudentCount),
            }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder.HasValue ? 0 : 1).ThenBy(x => x.DisplayOrder).ToList();
        }
    }
}
