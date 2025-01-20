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
            var placementTestResultGroups = new ConcurrentBag<PlacementTestResultReportGroupModel>();

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

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var placementTestResultReports = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).ToList();
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default,
                    ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                    {
                        var numberStudentOfLevel = placementTestResultReports?.Where(x => x.IsDonePT && x.CourseLevel == courseLevel).Count() ?? default;
                        return new ReportCourseLevelModel
                        {
                            CourseLevel = courseLevel,
                            TotalStudent = numberStudentOfLevel,
                            Percent = NumberHelper.GetPercent(numberStudentOfLevel, placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default)
                        };
                    }).ToList()
                };
                reportPlacementTestEvents.Add(reportPlacementTestEvent);
            });
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
                        excelWorksheet.Cells[startRow, 7].Value = item.NumberStudentsCompletedPT;
                        excelWorksheet.Cells[startRow, 8].Value = item.CompletionRate + "%";
                        if (item.ReportCourseLevels != null)
                        {
                            var rowReportLevel = 9;
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