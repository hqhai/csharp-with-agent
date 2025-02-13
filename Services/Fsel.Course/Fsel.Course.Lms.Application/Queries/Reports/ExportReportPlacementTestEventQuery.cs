// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.Reports
{
    using System.Collections.Concurrent;
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

    public class ExportReportPlacementTestEventQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportPlacementTestEventQueryHandler : IRequestHandler<ExportReportPlacementTestEventQuery, MethodResult<Stream>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IServiceProvider _serviceProvider;

        public ExportReportPlacementTestEventQueryHandler(
            IMapper mapper,
            IUserService userService,
            IServiceProvider serviceProvider)
        {
            _mapper = mapper;
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportPlacementTestEventQuery request, CancellationToken cancellationToken)
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
            var reportPlacementTestEvents = new ConcurrentBag<ReportPlacementTestEventModel>();
            var placementTestResultGroups = new ConcurrentStack<PlacementTestResultReportGroupModel>();

            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();

            // Chia danh sách thành từng nhóm
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

            foreach (var reportCompetitionEvent in reportCompetitionEvents)
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).Distinct().ToList();
                int numberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default;
                int numberStudentsProcessPT = placementTestResultReports?.Where(x => !x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default;
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsProcessPT = numberStudentsProcessPT,
                    NumberStudentCompleteVerify = reportCompetitionEvent.NumberStudentCompleteVerify,
                    NumberStudentsCompletedPT = numberStudentsCompletedPT,
                    ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                    {
                        var numberStudentOfLevel = placementTestResultReports?.Where(x => x.CourseLevel == courseLevel && x.IsDonePT).Select(x => x.StudentId).Distinct().Count() ?? default;
                        return new ReportCourseLevelModel
                        {
                            CourseLevel = courseLevel,
                            TotalStudent = numberStudentOfLevel,
                            Percent = NumberHelper.GetPercent(numberStudentOfLevel, numberStudentsCompletedPT)
                        };
                    }).ToList()
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            }
            methodResult.Result = ExportExcelTemplate(reportPlacementTestEvents.ToList(), request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<ReportPlacementTestEventModel>? reportPlacementTestEvents, ExportReportPlacementTestEventQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportPTEvent)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                excelWorksheet.Cells["A5"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["A5"].Value, $"{request.EducationLevel.GetDescription()}");

                int startRow = 9;
                if (reportPlacementTestEvents != null && reportPlacementTestEvents.Any())
                {
                    foreach (var item in reportPlacementTestEvents)
                    {
                        excelWorksheet.Cells[startRow, 1].Value = reportPlacementTestEvents.IndexOf(item) + 1;
                        excelWorksheet.Cells[startRow, 2].Value = item.LocationName;
                        excelWorksheet.Cells[startRow, 3].Value = item.NumberRegisteredSchool;
                        excelWorksheet.Cells[startRow, 4].Value = item.NumberActualParticipatingSchool;
                        excelWorksheet.Cells[startRow, 5].Value = item.ActualSchoolParticipationRate + "%";
                        excelWorksheet.Cells[startRow, 6].Value = item.NumberValidStudentAccount;
                        excelWorksheet.Cells[startRow, 7].Value = item.NumberStudentCompleteVerify;
                        excelWorksheet.Cells[startRow, 8].Value = item.CompleteVerifyRate + "%";
                        excelWorksheet.Cells[startRow, 9].Value = item.NumberStudentsProcessPT;
                        excelWorksheet.Cells[startRow, 10].Value = item.NumberStudentsCompletedPT;
                        excelWorksheet.Cells[startRow, 11].Value = item.CompletionRate + "%";
                        if (item.ReportCourseLevels != null)
                        {
                            var rowReportLevel = 12;
                            foreach (var reportLevel in item.ReportCourseLevels)
                            {
                                excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalStudent;
                                excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = reportLevel.Percent + "%";
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
    }
}
