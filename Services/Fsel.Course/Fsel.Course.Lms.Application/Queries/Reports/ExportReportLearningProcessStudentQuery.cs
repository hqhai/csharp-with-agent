// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System;
    using System.IO;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
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
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using Refit;
    using static Fsel.Shared.Constants.ValueSettings;

    public class ExportReportLearningProcessStudentQuery : ExportReportStudentLearningProcessQueueModel, IRequest<MethodResult<Stream>>
    {
    }

    public class ExportReportLearningProcessStudentQueryHandler : IRequestHandler<ExportReportLearningProcessStudentQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IStorageService _storageService;
        private const int RowExportReport = 1;
        private const int NumberModuleLesson = 3;

        public ExportReportLearningProcessStudentQueryHandler(
            IMapper mapper,
            IUserService userService,
            IServiceProvider serviceProvider,
            ICourseResultRepository courseResultRepository,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IStorageService storageService)
        {
            _mapper = mapper;
            _userService = userService;
            _serviceProvider = serviceProvider;
            _courseResultRepository = courseResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _storageService = storageService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportLearningProcessStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var studentEventRegistrationResults = await _userService.GetStudentEventRegistrationsAsync(new GetReportCompetitionEventQueryModel
            {
                CourseType = request.CourseType,
                EventCodeStr = request.EventCodeStr,
                DistrictName = request.DistrictName,
                StudentId = request.StudentId,
            });

            var studentEventRegistrations = studentEventRegistrationResults?.Content?.Result;
            if (studentEventRegistrations == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new List<ReportPlacementTestEventModel>();
            var placementTestResultGroups = new List<PlacementTestResultReportGroupModel>();
            var listCourseComplete = new List<CourseCompleteModel>();
            var courseStudentResults = new List<CourseResultModel>();
            var studentEventLearnProcesses = new List<StudentEventLearnProcessModel>();
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(request.CourseType);

            // Chia danh sách thành từng nhóm
            var batches = studentEventRegistrations.Select(x => x.StudentId)
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

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

            foreach (var batche in batches)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseResults = await courseResultRepository.Queryable.WhereBulkContains(batche, x => x.StudentId).Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                                              .Where(x => x.Course != null && courseLevels.Contains(x.Course.CourseLevel))
                                                                              .Select(x => _mapper.Map<CourseResultModel>(x))
                                                                              .ToListAsync(cancellationToken);

                    courseStudentResults.AddRange(courseResults);
                }
            };
            studentEventRegistrations = studentEventRegistrations.Where(x => courseStudentResults.Select(x => x.StudentId).Contains(x.StudentId)).ToList();
            var studentIds = studentEventRegistrations.Select(x => x.StudentId).ToList();
            var courseResultGroups = studentEventRegistrations
                                    .Select((id, index) => new { id, index })
                                    .GroupBy(x => x.index / BatchSize)
                                    .Select(g => g.Select(x => x.id).ToList())
                                    .ToList();

            foreach (var courseResults in courseResultGroups)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var courseResultRepository = scope.ServiceProvider.GetRequiredService<ICourseResultRepository>();
                    var courseUnitMockTestRepository = scope.ServiceProvider.GetRequiredService<ICourseUnitMockTestRepository>();
                    var unitResultRepository = scope.ServiceProvider.GetRequiredService<IUnitResultRepository>();
                    var finalTestResultRepository = scope.ServiceProvider.GetRequiredService<IFinalTestResultRepository>();
                    var unitLessonRepository = scope.ServiceProvider.GetRequiredService<IUnitLessonRepository>();
                    var lessonResultRepository = scope.ServiceProvider.GetRequiredService<ILessonResultRepository>();
                    var unitSkillMockTestRepository = scope.ServiceProvider.GetRequiredService<IUnitSkillMockTestRepository>();
                    var videoResultRepository = scope.ServiceProvider.GetRequiredService<IVideoResultRepository>();
                    var mockTestResultRepository = scope.ServiceProvider.GetRequiredService<IMockTestResultRepository>();
                    var classForumResultRepository = scope.ServiceProvider.GetRequiredService<IClassForumResultRepository>();

                    var query = from baseQ in courseResultRepository.Queryable

                                join cum in courseUnitMockTestRepository.Queryable
                                on baseQ.CourseId equals cum.CourseId

                                join ur in unitResultRepository.Queryable
                                on new { baseQ.StudentId, baseQ.CourseId, UnitId = cum.UnitId } equals new { ur.StudentId, ur.CourseId, UnitId = (Guid?)ur.UnitId } into unitGroup
                                from ur in unitGroup.DefaultIfEmpty()

                                join skmt in mockTestResultRepository.Queryable on new { ur.StudentId, UnitId = (Guid?)ur.UnitId, ur.CourseId } equals new { skmt.StudentId, UnitId = skmt.UnitId, skmt.CourseId } into skmtGroup
                                from skmt in skmtGroup.DefaultIfEmpty()

                                join ftr in finalTestResultRepository.Queryable
                                on new { baseQ.StudentId, baseQ.CourseId, FinalTestId = cum.FinalTestId } equals new { ftr.StudentId, ftr.CourseId, FinalTestId = (Guid?)ftr.FinalTestId } into ftrGroup
                                from ftr in ftrGroup.DefaultIfEmpty()

                                join mtr in mockTestResultRepository.Queryable
                                on new { baseQ.StudentId, baseQ.CourseId, MockTestId = cum.MockTestId } equals new { mtr.StudentId, mtr.CourseId, MockTestId = (Guid?)mtr.MockTestId } into mtrGroup
                                from mtr in mtrGroup.DefaultIfEmpty()

                                join lr in lessonResultRepository.Queryable
                                on new { ur.StudentId, ur.UnitId, ur.CourseId } equals new { lr.StudentId, lr.UnitId, lr.CourseId } into lrGroup
                                from lr in lrGroup.DefaultIfEmpty()

                                join vr in videoResultRepository.Queryable
                                on lr.Id equals vr.LessonResultId into vrGroup
                                from vr in vrGroup.DefaultIfEmpty()

                                join clr in classForumResultRepository.Queryable
                                on lr.Id equals clr.LessonResultId into clrGroup
                                from clr in clrGroup.DefaultIfEmpty()

                                where courseResults.Select(x => x.StudentId).Contains(baseQ.StudentId)
                                && baseQ.WorkingStatus == EnumWorkingStatus.Active
                                group new { baseQ, ur, lr, vr, clr, mtr, ftr, skmt }
                                by new { baseQ.CourseId, baseQ.StudentId }
                                        into g
                                select new CourseCompleteModel
                                {
                                    StudentId = g.Key.StudentId,
                                    CourseId = g.Key.CourseId,
                                    TotalLessonDone = g.Select(x => x.lr).Where(x => x.Status == EnumResultStatus.Done).Select(x => x.Id).Distinct().Count(),
                                    CountComplete = g.Where(x => x.vr.Status == EnumResultStatus.Done)
                                                     .Select(x => x.vr.Id).Distinct().Count() +

                                                     g.Where(x => x.clr.Status.HasValue)
                                                     .Select(x => x.clr.Id).Distinct().Count() +

                                                     g.Where(x => x.lr.Status == EnumResultStatus.Done)
                                                     .Select(x => x.lr.Id).Distinct().Count() +

                                                     g.Where(x => x.ftr.Status == EnumResultStatus.Done)
                                                      .Select(x => x.ftr.Id).Distinct().Count() +

                                                     g.Where(x => x.mtr.Status == EnumResultStatus.Done)
                                                      .Select(x => x.mtr.Id).Distinct().Count() +

                                                     g.Where(x => x.skmt.Status == EnumResultStatus.Done)
                                                      .Select(x => x.skmt.Id).Distinct().Count(),
                                };
                    listCourseComplete.AddRange(await query.ToListAsync(cancellationToken));
                }
            };

            var courseCompleteTotalModules = await GetCompleteCourseTotalsAsync(studentEventRegistrations.Select(x => new CourseResultModel
            {
                CourseId = x.CourseId.GetValueOrDefault(),
                StudentId = x.StudentId
            }).ToList());
            var learningProgressLearns = await GetStudyPositionAsync(request.CourseType, studentIds);

            foreach (var item in studentEventRegistrations)
            {
                var courseCompleteModule = listCourseComplete.FirstOrDefault(x => x.StudentId == item.StudentId && x.CourseId == item.CourseId) ?? new CourseCompleteModel
                {
                    StudentId = item.StudentId,
                    CourseId = item.CourseId.GetValueOrDefault(),
                };
                var courseCompleteTotalModule = courseCompleteTotalModules.FirstOrDefault(x => x.CourseId == item.CourseId);
                var courseLevelCompletetion = placementTestResultGroups.FirstOrDefault(x => x.StudentId == item.StudentId)?.CourseLevel;

                var studentEventLearnProcess = new StudentEventLearnProcessModel
                {
                    CourseLevel = item.CourseLevel.HasValue ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(item.CourseLevel.Value) : null,
                    ParentPhoneNumber = item.ParentPhoneNumber,
                    District = item.District,
                    Email = item.Email,
                    ExpiredDate = item.ExpiredDate,
                    FullName = item.FirstName + " " + item.LastName,
                    School = item.School,
                    SchoolClass = item.SchoolClass,
                    StartDate = null,
                    NumberOfMonth = null,
                    StudentId = item.StudentId,
                    PhoneNumber = item.PhoneNumber,
                    LevelCompletetion = courseLevelCompletetion.HasValue ? EnumCourseLevelHelper.GetCodeByEnumCourseLevel(SendMailHelper.GetPreviousEnumValue(courseLevelCompletetion.Value)) : null,
                    ContentProgress = $"{courseCompleteModule.CountComplete} / {courseCompleteTotalModule?.Count ?? default}",
                    PercentProgress = NumberHelper.GetPercent(courseCompleteModule.CountComplete, courseCompleteTotalModule?.Count ?? default),
                    TotalLessonCompleted = courseCompleteModule.TotalLessonDone,
                    LearningProgressLearns = learningProgressLearns.Where(x => x.StudentId == item.StudentId).ToList(),
                };
                studentEventLearnProcesses.Add(studentEventLearnProcess);
            };
            methodResult.Result = ExportExcelTemplate(studentEventLearnProcesses.ToList(), request);
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

        public static Stream ExportExcelTemplate(IList<StudentEventLearnProcessModel>? studentEventLearnProcesses, ExportReportLearningProcessStudentQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();
            var courseType = request.CourseType;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.BaoCaoReportStudentEvent)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                int mergeRangeCount = courseType == EnumCourseType.Academic ? CourseProgressValue.CountUnitAca * CourseProgressValue.CountLessonAca + CourseProgressValue.CountFinalTest
                    : courseType == EnumCourseType.Ielts ? CourseProgressValue.CountUnitIELTS * CourseProgressValue.CountLessonIELTS + CourseProgressValue.CountUnitIELTS + CourseProgressValue.CountFullMockTest
                    : default;

                int dem = courseType == EnumCourseType.Ielts ? 20 : 21;
                if (courseType == EnumCourseType.Ielts)
                {
                    var courseSkillMockTest = 0;
                    int fullMockTestCounter = 0;
                    for (int i = 5; i < mergeRangeCount; i++)
                    {
                        if ((i - fullMockTestCounter) % 5 == 0)
                        {
                            AddSkillMockTest(excelWorksheet, ref dem, ref i, ref courseSkillMockTest);
                            var range = excelWorksheet.Cells[RowExportReport, dem].Address + ":" + excelWorksheet.Cells[RowExportReport + 1, dem].Address;
                            excelWorksheet.Cells[range].Merge = true;
                            var border = excelWorksheet.Cells[range].Style.Border;
                            border.Top.Style = ExcelBorderStyle.Thin;
                            border.Bottom.Style = ExcelBorderStyle.Thin;
                            border.Left.Style = ExcelBorderStyle.Thin;
                            border.Right.Style = ExcelBorderStyle.Thin;
                            if (courseSkillMockTest % 4 == 0)
                            {
                                AddFullMockTest(excelWorksheet, ref dem, ref fullMockTestCounter);
                                var rangeFull = excelWorksheet.Cells[RowExportReport, dem].Address + ":" + excelWorksheet.Cells[RowExportReport + 1, dem].Address;
                                excelWorksheet.Cells[rangeFull].Merge = true;
                                var borderFull = excelWorksheet.Cells[rangeFull].Style.Border;
                                borderFull.Top.Style = ExcelBorderStyle.Thin;
                                borderFull.Bottom.Style = ExcelBorderStyle.Thin;
                                borderFull.Left.Style = ExcelBorderStyle.Thin;
                                borderFull.Right.Style = ExcelBorderStyle.Thin;
                                i++;
                            }
                        }
                        else
                        {
                            AddLesson(excelWorksheet, ref dem, i, courseSkillMockTest, fullMockTestCounter);
                            var range = excelWorksheet.Cells[RowExportReport, dem].Address + ":" + excelWorksheet.Cells[RowExportReport + 1, dem].Address;
                            excelWorksheet.Cells[range].Merge = true;
                            var border = excelWorksheet.Cells[range].Style.Border;
                            border.Top.Style = ExcelBorderStyle.Thin;
                            border.Bottom.Style = ExcelBorderStyle.Thin;
                            border.Left.Style = ExcelBorderStyle.Thin;
                            border.Right.Style = ExcelBorderStyle.Thin;
                        }
                    }
                }
                else
                {
                    for (int i = 6; i < mergeRangeCount; i++)
                    {
                        ExcelReportHelper.ProcessCell(excelWorksheet, RowExportReport, dem - 1, dem + 1, $"{nameof(Lesson)} {i}");
                        dem++;

                        var range = excelWorksheet.Cells[RowExportReport, dem].Address + ":" + excelWorksheet.Cells[RowExportReport + 1, dem].Address;
                        excelWorksheet.Cells[range].Merge = true;
                        var border = excelWorksheet.Cells[range].Style.Border;
                        border.Top.Style = ExcelBorderStyle.Thin;
                        border.Bottom.Style = ExcelBorderStyle.Thin;
                        border.Left.Style = ExcelBorderStyle.Thin;
                        border.Right.Style = ExcelBorderStyle.Thin;
                    }
                    ExcelReportHelper.ProcessCell(excelWorksheet, RowExportReport, dem - 1, dem + 1, nameof(FinalTest));
                    dem++;
                    var rangeFinal = excelWorksheet.Cells[RowExportReport, dem].Address + ":" + excelWorksheet.Cells[RowExportReport + 1, dem].Address;
                    excelWorksheet.Cells[rangeFinal].Merge = true;
                    var borderFinal = excelWorksheet.Cells[rangeFinal].Style.Border;
                    borderFinal.Top.Style = ExcelBorderStyle.Thin;
                    borderFinal.Bottom.Style = ExcelBorderStyle.Thin;
                    borderFinal.Left.Style = ExcelBorderStyle.Thin;
                    borderFinal.Right.Style = ExcelBorderStyle.Thin;
                }

                // Ghi dữ liệu từ reportPlacementTestEvents
                int startRow = 3;
                if (studentEventLearnProcesses != null && studentEventLearnProcesses.Any())
                {
                    foreach (var (item, index) in studentEventLearnProcesses.Select((value, idx) => (value, idx)))
                    {
                        excelWorksheet.Cells[startRow, 1].Value = index + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.FullName;
                        excelWorksheet.Cells[startRow, 3].Value = item.Email;
                        excelWorksheet.Cells[startRow, 4].Value = item.PhoneNumber;
                        excelWorksheet.Cells[startRow, 5].Value = item.ParentPhoneNumber;
                        excelWorksheet.Cells[startRow, 6].Value = item.District;
                        excelWorksheet.Cells[startRow, 7].Value = item.School;
                        excelWorksheet.Cells[startRow, 8].Value = item.SchoolClass;
                        excelWorksheet.Cells[startRow, 9].Value = item.LevelCompletetion;
                        excelWorksheet.Cells[startRow, 10].Value = item.CourseLevel;
                        excelWorksheet.Cells[startRow, 11].Value = item.NumberOfMonth;
                        excelWorksheet.Cells[startRow, 12].Value = item.StartDate.HasValue ? item.StartDate.Value.ToString("dd/MM/yyyy") : null;
                        excelWorksheet.Cells[startRow, 13].Value = item.ExpiredDate.HasValue ? item.ExpiredDate.Value.ToString("dd/MM/yyyy") : null;
                        excelWorksheet.Cells[startRow, 14].Value = item.ContentProgress;
                        excelWorksheet.Cells[startRow, 15].Value = item.PercentProgress + "%";
                        excelWorksheet.Cells[startRow, 16].Value = item.TotalLessonCompleted;

                        if (item.LearningProgressLearns != null)
                        {
                            int rowReportLevel = 17;
                            foreach (var reportLevel in item.LearningProgressLearns)
                            {
                                excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.StudentCount == 1 ? "X" : null;
                                excelWorksheet.Cells[startRow, rowReportLevel].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                rowReportLevel++;
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

        public async Task<List<OverallModuleLearnModel>> GetCompleteCourseTotalsAsync(IList<CourseResultModel>? courseResults)
        {
            if (courseResults == null || !courseResults.Any())
            {
                return new List<OverallModuleLearnModel>();
            }
            var courseIds = courseResults.Select(x => x.CourseId).Distinct().ToList();
            var courses = await _courseUnitMockTestRepository.Queryable.Where(x => courseIds.Contains(x.CourseId))
                                                                      .GroupBy(x => x.CourseId)
                                                                      .Select(x => new
                                                                      {
                                                                          CourseId = x.Key,
                                                                          CountLesson = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitLessons).Count(),
                                                                          CountSkillMockTest = x.Where(x => x.UnitId.HasValue).Select(x => x.Unit).SelectMany(x => x.UnitSkillMockTests).Count(),
                                                                          CountMockTest = x.Where(x => x.MockTestId.HasValue).Count(),
                                                                          CountFinalTest = x.Where(x => x.FinalTestId.HasValue).Count(),
                                                                      }).ToListAsync();
            return courseResults.Join(courses,
                                      courseResult => courseResult.CourseId,
                                      course => course.CourseId,
                                      (courseResult, course) => course).Select(x => new OverallModuleLearnModel
                                      {
                                          CourseId = x.CourseId,
                                          Count = x.CountLesson * NumberModuleLesson + x.CountMockTest + x.CountSkillMockTest + x.CountFinalTest
                                      }).ToList();
        }

        // Thêm SkillMockTest
        private static void AddSkillMockTest(ExcelWorksheet worksheet, ref int dem, ref int i, ref int courseSkillMockTest)
        {
            ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem, dem + 1, $"{nameof(EnumMockTestType.SkillMockTest)} {i / 5}");
            courseSkillMockTest++;
            dem++;
        }

        private static void AddLesson(ExcelWorksheet worksheet, ref int dem, int i, int courseSkillMockTest, int fullMockTestCounter)
        {
            ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem, dem + 1, $"{nameof(Lesson)} {i - courseSkillMockTest - fullMockTestCounter}");
            dem++;
        }

        // Thêm FullMockTest
        private static void AddFullMockTest(ExcelWorksheet worksheet, ref int dem, ref int fullMockTestCounter)
        {
            fullMockTestCounter++;
            ExcelReportHelper.ProcessCell(worksheet, RowExportReport, dem, dem + 1, $"{nameof(EnumMockTestType.FullMockTest)} {fullMockTestCounter}");
            dem++;
        }

        public async Task<IList<LearningProgressLearnModel>> GetStudyPositionAsync(EnumCourseType courseType, IList<Guid> studentIds)
        {
            var courseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(courseType);

            // Chia danh sách thành từng nhóm
            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / BatchSize1000)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            var listLearningProcess = new List<LearningProgressLearnModel>();
            foreach (var batche in batches)
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
                                                      group new { ul, courseUnitMockTest, ftr, lr } by new { baseQ.StudentId, DisplayOrder = (int?)ul.DisplayOrder, UnitDisplayOrder = courseUnitMockTest.DisplayOrder } into groupedData
                                                      select new LearningProgressLearnModel
                                                      {
                                                          StudentId = groupedData.Key.StudentId,
                                                          DisplayOrder = groupedData.Key.DisplayOrder,
                                                          UnitDisplayOrder = groupedData.Key.UnitDisplayOrder,
                                                          StudentCount = groupedData.Count(g =>
                                                              (g.ftr != null && g.ftr.Status == EnumResultStatus.Done) ||
                                                              (g.lr != null && g.lr.Status == EnumResultStatus.Done))
                                                      }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder).ToListAsync(CancellationToken.None);
                        listLearningProcess.AddRange(learningProgress);
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
                                                          cum.UnitId.HasValue && lr.Status == EnumResultStatus.Done
                                                      group lr by new { baseQ.StudentId, DisplayOrder = cum.DisplayOrder, LessonDisplayOrder = (int?)ul.DisplayOrder } into g
                                                      select new LearningProgressLearnModel
                                                      {
                                                          StudentId = g.Key.StudentId,
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
                                                                group new { mtr, mtrs } by new { baseQ.StudentId, cum.DisplayOrder } into g
                                                                select new LearningProgressLearnModel
                                                                {
                                                                    StudentId = g.Key.StudentId,
                                                                    UnitDisplayOrder = g.Key.DisplayOrder,
                                                                    DisplayOrder = null,
                                                                    StudentCount = g.Count(x =>
                                                                    (x.mtr != null && x.mtr.Status == EnumResultStatus.Done) ||
                                                                    (x.mtrs != null && x.mtrs.Status == EnumResultStatus.Done))
                                                                }).OrderBy(r => r.UnitDisplayOrder)
                                                                  .OrderBy(r => r.DisplayOrder.HasValue ? 0 : 1)
                                                                  .OrderBy(r => r.DisplayOrder)
                                                                  .ThenByDescending(r => r.StudentCount)
                                                                  .ToListAsync(CancellationToken.None);
                        listLearningProcess.AddRange(learningProgress);
                    }
                }
            };
            return listLearningProcess.GroupBy(x => new { x.DisplayOrder, x.UnitDisplayOrder, x.StudentId }).Select(x => new LearningProgressLearnModel
            {
                StudentId = x.Key.StudentId,
                DisplayOrder = x.Key.DisplayOrder,
                UnitDisplayOrder = x.Key.UnitDisplayOrder,
                StudentCount = x.Sum(y => y.StudentCount),
            }).OrderBy(x => x.UnitDisplayOrder).ThenBy(x => x.DisplayOrder.HasValue ? 0 : 1).ThenBy(x => x.DisplayOrder).ToList();
        }
    }
}
