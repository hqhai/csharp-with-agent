// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Data;
    using System.IO;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.QueryModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using OfficeOpenXml;

    public class ExportReportPlacementTestEventDistrictSchoolQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCode { get; set; }
        public string? DistrictName { get; set; }
        public Guid PtProgramId { get; set; }
    }

    public class ExportReportPlacementTestEventDistrictSchoolQueryHandler : IRequestHandler<ExportReportPlacementTestEventDistrictSchoolQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ITestGroupResultRepository _placementTestGroupResultRepository;
        private readonly ILevelRepository _levelRepository;

        public ExportReportPlacementTestEventDistrictSchoolQueryHandler(
            IUserService userService,
            ITestGroupResultRepository placementTestGroupResultRepository,
            ILevelRepository levelRepository)
        {
            _userService = userService;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportPlacementTestEventDistrictSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventDistrictSchoolAsync(new GetCompetitionEventToEventParentQueryModel
            {
                DistrictName = request.DistrictName,
                EventCode = request.EventCode,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var reportPlacementTestEvents = new List<ReportPlacementTestEventModel>();
            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any())
                .SelectMany(x => x.StudentIds ?? new List<Guid>())
                .ToList() ?? new List<Guid>();

            var placementTestResultGroups = await _placementTestGroupResultRepository.ReadQueryable
                .Include(x => x.CurrentLevel)
                .Where(x => x.ProgramIdOfPt == request.PtProgramId)
                .WhereBulkContains(studentIds, x => x.StudentId)
                .Select(x => new PlacementTestResultReportGroupModel
                {
                    LevelId = x.CurrentLevelId,
                    LevelName = x.CurrentLevel != null ? x.CurrentLevel.Name : string.Empty,
                    StudentId = x.StudentId ?? new Guid(),
                    IsDonePT = x.Status == EnumResultStatus.Done || x.Status == EnumResultStatus.ByPass
                })
                .ToListAsync(cancellationToken);

            var programLevels = await _levelRepository.ReadQueryable
                .Where(x => x.ProgramId == request.PtProgramId)
                .OrderBy(x => x.LevelOrder)
                .ToListAsync(cancellationToken);

            foreach (var reportCompetitionEvent in reportCompetitionEvents)
            {
                var studentIdsSet = reportCompetitionEvent.StudentIds?.ToHashSet() ?? new HashSet<Guid>();
                var placementTestResultReports = placementTestResultGroups.Where(x => studentIdsSet.Contains(x.StudentId)).ToList();

                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    ReportPlacementTestEventSchools = reportCompetitionEvent.ReportCompetitionEventSchools.Select(eventSchool =>
                    {
                        var eventSchoolStudentIds = eventSchool.StudentIds?.ToHashSet() ?? new HashSet<Guid>();

                        var placementTestResultSchools = placementTestResultReports.Where(x => eventSchoolStudentIds.Contains(x.StudentId));
                        var numberStudentsCompletedPT = placementTestResultSchools.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                        int numberStudentsProcessPT = placementTestResultSchools.Where(x => !x.IsDonePT).Select(x => x.StudentId).Distinct().Count();

                        return new ReportPlacementTestEventSchoolModel
                        {
                            SchoolName = eventSchool.SchoolName,
                            NumberStudentsCompletedPT = numberStudentsCompletedPT,
                            NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                            NumberStudentsProcessPT = numberStudentsProcessPT,
                            NumberStudentCompleteVerify = eventSchool.NumberStudentCompleteVerify,
                            NumberStudentAccountRegister = eventSchool.NumberStudentAccountRegister,
                            ReportCourseLevels = programLevels
                                .Select(courseLevel =>
                                {
                                    var numberStudentOfLevel = placementTestResultSchools.Where(x => x.LevelId == courseLevel.Id && x.IsDonePT).Select(x => x.StudentId).Distinct().Count();
                                    return new ReportCourseLevelModel
                                    {
                                        LevelCode = courseLevel.Code,
                                        TotalStudent = numberStudentOfLevel,
                                        Percent = NumberHelper.GetPercent(numberStudentOfLevel, numberStudentsCompletedPT)
                                    };
                                })
                                .ToList()
                        };
                    }).ToList(),
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            }

            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents.ToList(), request, programLevels.Select(x => x.Code));
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents,
            ExportReportPlacementTestEventDistrictSchoolQuery request,
            IEnumerable<string> levelCodes)
        {
            var memoryStream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Luôn đọc template dưới dạng stream chỉ-đọc để tránh ghi đè file gốc
            using (var templateStream = new FileStream(ResourceSettings.ReportPTEventDistrictSchool, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var excelPackage = new ExcelPackage(templateStream))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];

                // add header for dynamic levels
                var levelRow = 8;
                var indexColumn = 12;
                var levelHeaderRow = new List<object>();
                for (var i = 0; i < indexColumn - 1; i++)
                {
                    levelHeaderRow.Add(string.Empty);
                }

                foreach (var levelCode in levelCodes)
                {
                    levelHeaderRow.Add(levelCode);
                    levelHeaderRow.Add("%");
                }

                excelWorksheet.Cells[levelRow, 1].LoadFromArrays(new List<object[]> { levelHeaderRow.ToArray() });

                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    int startRow = 9;
                    int index = 1;
                    var dataRows = new List<object[]>();
                    foreach (var item in reportPlacementTestEvents)
                    {
                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            var row = new List<object>
                            {
                                index++,
                                item.LocationName ?? string.Empty,
                                reportPt.SchoolName ?? string.Empty,
                                reportPt.NumberValidStudentAccount,
                                reportPt.NumberStudentAccountRegister,
                                reportPt.TotalStudentAccount,
                                reportPt.NumberStudentCompleteVerify,
                                reportPt.CompleteVerifyRate + "%",
                                reportPt.NumberStudentsProcessPT,
                                reportPt.NumberStudentsCompletedPT,
                                reportPt.CompletionRate + "%"
                            };

                            if (reportPt.ReportCourseLevels != null)
                            {
                                foreach (var reportLevel in reportPt.ReportCourseLevels)
                                {
                                    row.Add(reportLevel.TotalStudent);
                                    row.Add(reportLevel.Percent + "%");
                                }
                            }

                            dataRows.Add(row.ToArray());
                        }
                    }

                    if (dataRows.Count > 0)
                    {
                        excelWorksheet.Cells[startRow, 1].LoadFromArrays(dataRows);
                    }
                }

                // Lưu kết quả vào MemoryStream thay vì ghi đè file template
                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }
    }
}
