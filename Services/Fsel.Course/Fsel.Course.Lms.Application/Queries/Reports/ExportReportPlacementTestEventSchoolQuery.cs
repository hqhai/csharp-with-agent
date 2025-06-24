// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
    using Microsoft.Extensions.Logging;
    using OfficeOpenXml;

    public class ExportReportPlacementTestEventSchoolQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportPlacementTestEventSchoolQueryHandler : IRequestHandler<ExportReportPlacementTestEventSchoolQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ILogger<ExportReportPlacementTestEventSchoolQueryHandler> _logger;

        public ExportReportPlacementTestEventSchoolQueryHandler(
            IUserService userService,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            ILogger<ExportReportPlacementTestEventSchoolQueryHandler> logger)
        {
            _userService = userService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _logger = logger;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportPlacementTestEventSchoolQuery request, CancellationToken cancellationToken)
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
            _logger.LoggerRequest($"ExportReportPlacementTestEventSchoolQueryHandler : {reportCompetitionEvents.Select(x => x.DistrictName).Serialize()}");

            var reportPlacementTestEvents = new List<ReportPlacementTestEventModel>();
            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();
            var placementTestResultGroups = await _placementTestGroupResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId)
                                                          .Select(x => new PlacementTestResultReportGroupModel
                                                          {
                                                              CourseLevel = x.SuggetLevel,
                                                              StudentId = x.StudentId,
                                                              IsDonePT = x.Status == EnumResultStatus.Done
                                                          })
                                                          .ToListAsync(cancellationToken);
            foreach (var reportCompetitionEvent in reportCompetitionEvents)
            {
                var studentDistrictIds = reportCompetitionEvent.StudentIds?.ToHashSet() ?? new HashSet<Guid>();
                var placementTestResultReports = placementTestResultGroups.Where(x => studentDistrictIds.Contains(x.StudentId));

                int numberStudentsCompletedPT = placementTestResultReports.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                int numberStudentsProcessPT = placementTestResultReports.Where(x => !x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    ReportPlacementTestEventSchools = reportCompetitionEvent.ReportCompetitionEventSchools.Select(eventSchool =>
                    {
                        var schoolStudentIds = eventSchool.StudentIds?.ToHashSet() ?? new HashSet<Guid>();
                        var placementTestResultSchools = placementTestResultReports.Where(x => schoolStudentIds.Contains(x.StudentId));
                        int numberStudentsCompletedPTSchool = placementTestResultSchools.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                        int numberStudentsProcessPTSchool = placementTestResultSchools.Where(x => !x.IsDonePT).Select(x => x.StudentId).Distinct().Count();

                        return new ReportPlacementTestEventSchoolModel
                        {
                            SchoolName = eventSchool.SchoolName,
                            NumberStudentsCompletedPT = numberStudentsCompletedPTSchool,
                            NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                            NumberStudentsProcessPT = numberStudentsProcessPTSchool,
                            NumberStudentCompleteVerify = eventSchool.NumberStudentCompleteVerify,
                            ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic)
                            .Select(courseLevel =>
                            {
                                int numberStudentOfLevel = placementTestResultSchools.Where(x => x.CourseLevel == courseLevel && x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                                return new ReportCourseLevelModel
                                {
                                    CourseLevel = courseLevel,
                                    TotalStudent = numberStudentOfLevel,
                                    Percent = NumberHelper.GetPercent(numberStudentOfLevel, numberStudentsCompletedPTSchool)
                                };
                            }).ToList(),
                        };
                    }).ToList(),
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            }
            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents, request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportPlacementTestEventSchoolQuery request)
        {
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportPTEventSchool)))
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
                        var index = reportPlacementTestEvents.Where(x => x.LocationName == item.LocationName).Count();
                        var excelWorksheet = excelPackage.Workbook.Worksheets.Copy(originalWorksheet.Name, index == 1 ? item.LocationName : $"{item.LocationName} {reportPlacementTestEvents.IndexOf(item)}");
                        excelWorksheet.Cells["A5"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["A5"].Value, new[] { request.EducationLevel.GetDescription(), item.LocationName });
                        int startRow = 9;

                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            excelWorksheet.Cells[startRow, 1].Value = item.ReportPlacementTestEventSchools.IndexOf(reportPt) + 1;
                            excelWorksheet.Cells[startRow, 2].Value = reportPt.SchoolName;
                            excelWorksheet.Cells[startRow, 3].Value = reportPt.NumberValidStudentAccount;
                            excelWorksheet.Cells[startRow, 4].Value = reportPt.NumberStudentCompleteVerify;
                            excelWorksheet.Cells[startRow, 5].Value = reportPt.CompleteVerifyRate + "%";
                            excelWorksheet.Cells[startRow, 6].Value = reportPt.NumberStudentsProcessPT;
                            excelWorksheet.Cells[startRow, 7].Value = reportPt.NumberStudentsCompletedPT;
                            excelWorksheet.Cells[startRow, 8].Value = reportPt.CompletionRate + "%";
                            if (reportPt.ReportCourseLevels != null)
                            {
                                var rowReportLevel = 9;
                                foreach (var reportLevel in reportPt.ReportCourseLevels)
                                {
                                    excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalStudent;
                                    excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = reportLevel.Percent + "%";
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
    }
}
