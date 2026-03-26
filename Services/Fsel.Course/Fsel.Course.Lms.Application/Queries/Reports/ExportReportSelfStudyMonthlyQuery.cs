// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.ExportEventModels;
    using Fsel.Course.Infrastructure;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;

    public enum EnumCrmLocationLevel
    {
        District,
        School,
        Student
    }

    public class ExportReportSelfStudyMonthlyQuery : IRequest<MethodResult<Stream>>
    {
        public string? FileName { get; set; }
        public string? EventCode { get; set; }
        public string? SubjectCode { get; set; }
        public EnumEducationLevel? EducationLevel { get; set; }
        public EnumCrmLocationLevel LocationLevel { get; set; }
    }

    public class ExportReportSelfStudyMonthlyQueryHandler : IRequestHandler<ExportReportSelfStudyMonthlyQuery, MethodResult<Stream>>
    {
        private readonly CourseDbContext _courseDbContext;
        public const double ContentCompleteWeight = 75.0;
        public const double FinalScoreWeight = 0.25;

        public ExportReportSelfStudyMonthlyQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportSelfStudyMonthlyQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            if (request.EventCode == null)
            {
                return methodResult;
            }

            if (request.LocationLevel == EnumCrmLocationLevel.Student)
            {
                if (string.IsNullOrEmpty(request.EventCode) || string.IsNullOrEmpty(request.SubjectCode))
                {
                    methodResult.StatusCode = 400;
                    return methodResult;
                }
                var exportStudentEvents = await _courseDbContext.Set<ExportStudentEventModelV2>()
                                                              .FromSqlRaw("EXEC [CollectEventStudentData] @EventCode ,@Category ,@SubjectCode",
                                                                   new SqlParameter("@EventCode", request.EventCode),
                                                                   new SqlParameter("@Category", "Student"),
                                                                   new SqlParameter("@SubjectCode", request.SubjectCode))
                                                              .AsNoTracking()
                                                              .ToListAsync(cancellationToken);
                exportStudentEvents = exportStudentEvents.Where(e => !string.IsNullOrEmpty(e.EventCode)).ToList();
                methodResult.Result = ExportExcelStudentTemplate(exportStudentEvents.GroupBy(x => x.EventCode ?? string.Empty));
                return methodResult;
            }

            var exportDistrictEvents = await _courseDbContext.Set<ExportDistrictEventModel>()
                                                             .FromSqlRaw("EXEC ExportDistrictDataToEventHaNoi @SchoolTypeLevel",
                                                                  new SqlParameter("@SchoolTypeLevel", request.EducationLevel ?? (object)DBNull.Value))
                                                             .AsNoTracking()
                                                             .ToListAsync(cancellationToken);

            var listData = new List<ExportDistrictModel>();

            if (request.LocationLevel == EnumCrmLocationLevel.District)
            {
                methodResult.Result = ExportExcelTemplate(exportDistrictEvents);
            }
            if (request.LocationLevel == EnumCrmLocationLevel.School)
            {
                foreach (var item in exportDistrictEvents)
                {
                    var exportSchoolEvents = await _courseDbContext.Set<ExportSchoolEventModel>()
                                                              .FromSqlRaw("EXEC [ExportDataDistrictEventHaNoi] @EventCode ,@LocationId ,@SchoolId",
                                                                   new SqlParameter("@EventCode", request.EventCode),
                                                                   new SqlParameter("@LocationId", item.LocationId),
                                                                   new SqlParameter("@SchoolId", (object)DBNull.Value))
                                                              .AsNoTracking()
                                                              .ToListAsync(cancellationToken);
                    listData.Add(new ExportDistrictModel
                    {
                        LocationName = item.LocationName,
                        ExportSchoolEvents = exportSchoolEvents
                    });
                }
                methodResult.Result = ExportExcelSchoolTemplate(listData);
            }

            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ExportDistrictEventModel>? exportDistrictEvents)
        {
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportDoetSelfstudy)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                int startRow = 4;
                if (exportDistrictEvents != null && exportDistrictEvents.Any())
                {
                    foreach (var item in exportDistrictEvents)
                    {
                        excelWorksheet.Cells[startRow, 1].Value = exportDistrictEvents.IndexOf(item) + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.LocationName;
                        excelWorksheet.Cells[startRow, 3].Value = item.NumberOfCompletedLessons;
                        excelWorksheet.Cells[startRow, 4].Value = item.TargetLessonCompletionRate + "%";
                        excelWorksheet.Cells[startRow, 5].Value = item.ScoreLevelLesson;
                        excelWorksheet.Cells[startRow, 6].Value = item.LevelCompletionRate;
                        excelWorksheet.Cells[startRow, 7].Value = item.AchievedScore + "%";
                        excelWorksheet.Cells[startRow, 8].Value = item.AssignmentClassForum + "%";
                        excelWorksheet.Cells[startRow, 9].Value = item.ScoreLevelClassForum;
                        excelWorksheet.Cells[startRow, 10].Value = item.NumberofCommentsonPosts;
                        excelWorksheet.Cells[startRow, 11].Value = item.ScoreLevelComment;
                        excelWorksheet.Cells[startRow, 12].Value = item.TotalScore;
                        startRow++;
                    }
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static Stream ExportExcelSchoolTemplate(IList<ExportDistrictModel>? exportDistrictModels)
        {
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportDistrictSchool)))
            {
                var originalWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (originalWorksheet == null)
                {
                    throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                }
                if (exportDistrictModels != null && exportDistrictModels.Any())
                {
                    int startRow = 4;
                    int index = 1;
                    foreach (var item in exportDistrictModels.SelectMany(x => x.ExportSchoolEvents))
                    {
                        originalWorksheet.Cells[startRow, 1].Value = index;
                        originalWorksheet.Cells[startRow, 2].Value = item.School;
                        originalWorksheet.Cells[startRow, 3].Value = item.NumberOfCompletedLessons;
                        originalWorksheet.Cells[startRow, 4].Value = item.TargetLessonCompletionRate + "%";
                        originalWorksheet.Cells[startRow, 5].Value = item.ScoreLevelLesson;
                        originalWorksheet.Cells[startRow, 6].Value = item.LevelCompletionRate;
                        originalWorksheet.Cells[startRow, 7].Value = item.AchievedScore + "%";
                        originalWorksheet.Cells[startRow, 8].Value = item.AssignmentClassForum + "%";
                        originalWorksheet.Cells[startRow, 9].Value = item.ScoreLevelClassForum;
                        originalWorksheet.Cells[startRow, 10].Value = item.NumberofCommentsonPosts;
                        originalWorksheet.Cells[startRow, 11].Value = item.ScoreLevelComment;
                        originalWorksheet.Cells[startRow, 12].Value = item.TotalScore;
                        startRow++;
                        index++;
                    }
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static Stream ExportExcelStudentTemplate(IEnumerable<IGrouping<string, ExportStudentEventModelV2>>? exportDistrictModels)
        {
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportSchoolStudent)))
            {
                var originalWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (originalWorksheet == null)
                {
                    throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                }
                if (exportDistrictModels != null && exportDistrictModels.Any())
                {
                    foreach (var item in exportDistrictModels)
                    {
                        var excelWorksheet = excelPackage.Workbook.Worksheets.Copy(originalWorksheet.Name, item.Key);
                        int startRow = 4;
                        var rows = item.ToList();
                        for (var i = 0; i < rows.Count; i++)
                        {
                            var itemReport = rows[i];
                            excelWorksheet.Cells[startRow, 1].Value = i + 1;
                            excelWorksheet.Cells[startRow, 2].Value = itemReport.FullName;
                            excelWorksheet.Cells[startRow, 3].Value = itemReport.UserName;
                            excelWorksheet.Cells[startRow, 4].Value = itemReport.Email;
                            excelWorksheet.Cells[startRow, 5].Value = itemReport.PhoneNumber;
                            excelWorksheet.Cells[startRow, 6].Value = itemReport.School;
                            excelWorksheet.Cells[startRow, 7].Value = itemReport.SchoolGrade;
                            excelWorksheet.Cells[startRow, 8].Value = itemReport.SchoolClass;
                            excelWorksheet.Cells[startRow, 9].Value = itemReport.NumberOfCompletedLessons;
                            excelWorksheet.Cells[startRow, 10].Value = itemReport.OverrallScore;
                            excelWorksheet.Cells[startRow, 11].Value = itemReport.NumberOfCompletedClassForum;
                            excelWorksheet.Cells[startRow, 12].Value = itemReport.NumberOfComments;
                            excelWorksheet.Cells[startRow, 13].Value = itemReport.NumberOfCompletedComponent;
                            excelWorksheet.Cells[startRow, 14].Value = itemReport.TotalComponent;
                            excelWorksheet.Cells[startRow, 15].Value = itemReport.LocalId;
                            excelWorksheet.Cells[startRow, 16].Value = itemReport.GlobalId;
                            excelWorksheet.Cells[startRow, 17].Value = itemReport.EventCode;
                            startRow++;
                        }
                    }
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }
    }
}
