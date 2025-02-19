// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using AutoMapper;
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
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using System.Collections.Concurrent;

    public class ExportReportPlacementTestEventDistrictSchoolQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCode { get; set; }
        public string? DistrictName { get; set; }
    }

    public class ExportReportPlacementTestEventDistrictSchoolQueryHandler : IRequestHandler<ExportReportPlacementTestEventDistrictSchoolQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public ExportReportPlacementTestEventDistrictSchoolQueryHandler(
            IMapper mapper,
            IUserService userService,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _serviceProvider = serviceProvider;
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
            var reportPlacementTestEvents = new ConcurrentBag<ReportPlacementTestEventModel>();
            var placementTestResultGroups = new ConcurrentStack<PlacementTestResultReportGroupModel>();
            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();

            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / ValueSettings.BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            // Thực hiện truy vấn từng nhóm
            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var placementTestGroupResultRepository = scope.ServiceProvider.GetRequiredService<IPlacementTestGroupResultRepository>();
                    var placementTestGroups = await placementTestGroupResultRepository.Queryable
                                                    .Where(x => batche.Contains(x.StudentId))
                                                    .Select(x => new PlacementTestResultReportGroupModel
                                                    {
                                                        CourseLevel = x.SuggetLevel,
                                                        StudentId = x.StudentId,
                                                        IsDonePT = x.Status == EnumResultStatus.Done
                                                    })
                                                    .ToListAsync(cancellationToken);
                    placementTestResultGroups.PushRange(placementTestGroups.ToArray());
                }
            });

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).Distinct().ToList();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default,
                    ReportCourseLevels = placementTestResultReports != null && placementTestResultReports.Any() ? EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                    {
                        var numberStudentOfLevel = placementTestResultReports?.Where(x => x.CourseLevel == courseLevel).Select(x => x.StudentId).Distinct().Count() ?? default;
                        return new ReportCourseLevelModel
                        {
                            CourseLevel = courseLevel,
                            TotalStudent = numberStudentOfLevel,
                            Percent = NumberHelper.GetPercent(numberStudentOfLevel, placementTestResultReports?.Select(x => x.StudentId).Distinct().Count() ?? default)
                        };
                    }).ToList() : new List<ReportCourseLevelModel>(),
                    ReportPlacementTestEventSchools = reportCompetitionEvent.ReportCompetitionEventSchools.Select(eventSchool =>
                    {
                        var placementTestResultSchools = placementTestResultReports?.Where(x => eventSchool.StudentIds != null && eventSchool.StudentIds.Contains(x.StudentId)).ToList();
                        int numberStudentsCompletedPT = placementTestResultSchools?.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default;

                        return new ReportPlacementTestEventSchoolModel
                        {
                            SchoolName = eventSchool.SchoolName,
                            NumberStudentsCompletedPT = numberStudentsCompletedPT,
                            NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                            NumberStudentsProcessPT = placementTestResultSchools?.Where(x => !x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default,
                            NumberStudentCompleteVerify = eventSchool.NumberStudentCompleteVerify,
                            NumberStudentAccountRegister = eventSchool.NumberStudentAccountRegister,
                            ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                            {
                                var numberStudentOfLevel = placementTestResultSchools?.Where(x => x.CourseLevel == courseLevel && x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default;
                                return new ReportCourseLevelModel
                                {
                                    CourseLevel = courseLevel,
                                    TotalStudent = numberStudentOfLevel,
                                    Percent = NumberHelper.GetPercent(numberStudentOfLevel, numberStudentsCompletedPT)
                                };
                            }).ToList(),
                        };
                    }).ToList(),
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            });
            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents.ToList(), request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportPlacementTestEventDistrictSchoolQuery request)
        {
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportPTEventDistrictSchool)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    int startRow = 9;
                    int index = 1;

                    foreach (var item in reportPlacementTestEvents)
                    {
                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            excelWorksheet.Cells[startRow, 1].Value = index;
                            excelWorksheet.Cells[startRow, 2].Value = item.LocationName;
                            excelWorksheet.Cells[startRow, 3].Value = reportPt.SchoolName;
                            excelWorksheet.Cells[startRow, 4].Value = reportPt.NumberValidStudentAccount;
                            excelWorksheet.Cells[startRow, 5].Value = reportPt.NumberStudentAccountRegister;
                            excelWorksheet.Cells[startRow, 6].Value = reportPt.TotalStudentAccount;
                            excelWorksheet.Cells[startRow, 7].Value = reportPt.NumberStudentCompleteVerify;
                            excelWorksheet.Cells[startRow, 8].Value = reportPt.CompleteVerifyRate + "%";
                            excelWorksheet.Cells[startRow, 9].Value = reportPt.NumberStudentsProcessPT;
                            excelWorksheet.Cells[startRow, 10].Value = reportPt.NumberStudentsCompletedPT;
                            excelWorksheet.Cells[startRow, 11].Value = reportPt.CompletionRate + "%";
                            if (reportPt.ReportCourseLevels != null)
                            {
                                var rowReportLevel = 12;
                                foreach (var reportLevel in reportPt.ReportCourseLevels)
                                {
                                    excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalStudent;
                                    excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = reportLevel.Percent + "%";
                                    rowReportLevel += 2;
                                }
                            }
                            startRow++;
                            index++;
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
