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
    using OfficeOpenXml;

    public class ExportReportPlacementTestEventSchoolQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportPlacementTestEventSchoolQueryHandler : IRequestHandler<ExportReportPlacementTestEventSchoolQuery, MethodResult<Stream>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public ExportReportPlacementTestEventSchoolQueryHandler(IPlacementTestResultRepository placementTestResultRepository,
            IMapper mapper,
            IUserService userService)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _mapper = mapper;
            _userService = userService;
        }

        private class PlacementTestGroupStudentResultModel
        {
            public Guid StudentId { get; set; }
            public PlacementTestResultModel? PlacementTestStart { get; set; }
            public PlacementTestResultModel? PlacementTestEnd { get; set; }
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
            var reportPlacementTestEvents = new ConcurrentBag<ReportPlacementTestEventModel>();

            var studentIds = reportCompetitionEvents.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds ?? new List<Guid>()).ToList();

            var placementTestResultGroups = new List<PlacementTestGroupStudentResultModel>();

            int batchSize = 2000; // Số lượng bản ghi mỗi lần truy vấn

            // Chia danh sách thành từng nhóm
            var batches = studentIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            // Thực hiện truy vấn từng nhóm
            foreach (var batch in batches)
            {
                var placementTestGroups = await _placementTestResultRepository.Queryable
                                    .Where(x => batch.Contains(x.StudentId) && x.Status == EnumResultStatus.Done)
                                    .GroupBy(x => x.StudentId)
                                    .Select(x => new PlacementTestGroupStudentResultModel
                                    {
                                        StudentId = x.Key,
                                        PlacementTestStart = _mapper.Map<PlacementTestResultModel>(x.Select(x => x).OrderBy(x => x.CreatedDate).FirstOrDefault()),
                                        PlacementTestEnd = _mapper.Map<PlacementTestResultModel>(x.Select(x => x).OrderByDescending(x => x.CreatedDate).FirstOrDefault()),
                                    })
                                    .ToListAsync(cancellationToken);

                // Thêm vào danh sách kết quả
                placementTestResultGroups.AddRange(placementTestGroups);
            }

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var placementTestResultGroupStudents = placementTestResultGroups.Where(x => reportCompetitionEvent.StudentIds != null && reportCompetitionEvent.StudentIds.Contains(x.StudentId)).ToList();
                var placementTestResultReports = reportCompetitionEvent.StudentIds?.Select(item =>
                {
                    var placementTestGroupResult = placementTestResultGroups.FirstOrDefault(x => x.StudentId == item);
                    var placementTestResultEnd = placementTestGroupResult?.PlacementTestEnd;
                    var placementTestResultStart = placementTestGroupResult?.PlacementTestStart;
                    if (placementTestResultEnd != null)
                    {
                        var (levelCompleted, isLock) = placementTestResultEnd.Level.GetLevelInScore(placementTestResultEnd.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultStart?.Level, default));
                        return new PlacementTestResultReportGroupModel { StudentId = item, IsDonePT = isLock, CourseLevel = levelCompleted };
                    }
                    return new PlacementTestResultReportGroupModel { StudentId = item };
                });
                var reportPlacementTestEvent = new ReportPlacementTestEventModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberRegisteredSchool = reportCompetitionEvent.NumberRegisteredSchool,
                    NumberActualParticipatingSchool = reportCompetitionEvent.NumberActualParticipatingSchool,
                    NumberValidStudentAccount = reportCompetitionEvent.NumberValidStudentAccount,
                    NumberStudentsCompletedPT = placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default,
                    ReportCourseLevels = placementTestResultReports != null && placementTestResultReports.Any() ? EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                    {
                        var numberStudentOfLevel = placementTestResultReports?.Where(x => x.IsDonePT && x.CourseLevel == courseLevel).Count() ?? default;
                        return new ReportCourseLevelModel
                        {
                            CourseLevel = courseLevel,
                            TotalStudent = numberStudentOfLevel,
                            Percent = NumberHelper.GetPercent(numberStudentOfLevel, placementTestResultReports?.Where(x => x.IsDonePT).Count() ?? default)
                        };
                    }).ToList() : new List<ReportCourseLevelModel>(),
                    ReportPlacementTestEventSchools = reportCompetitionEvent.ReportCompetitionEventSchools.Select(eventSchool =>
                    {
                        var placementTestResultSchools = placementTestResultReports?.Where(x => eventSchool.StudentIds != null && eventSchool.StudentIds.Contains(x.StudentId)).ToList();
                        return new ReportPlacementTestEventSchoolModel
                        {
                            SchoolName = eventSchool.SchoolName,
                            NumberStudentsCompletedPT = placementTestResultSchools?.Where(x => x.IsDonePT).Count() ?? default,
                            NumberValidStudentAccount = eventSchool.NumberValidStudentAccount,
                            ReportCourseLevels = EnumCourseLevelHelper.GetEnumCourseLevels(EnumCourseType.Academic).Select(courseLevel =>
                            {
                                var numberStudentOfLevel = placementTestResultSchools?.Where(x => x.IsDonePT && x.CourseLevel == courseLevel).Count() ?? default;
                                return new ReportCourseLevelModel
                                {
                                    CourseLevel = courseLevel,
                                    TotalStudent = numberStudentOfLevel,
                                    Percent = NumberHelper.GetPercent(numberStudentOfLevel, placementTestResultSchools?.Where(x => x.IsDonePT).Count() ?? default)
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
                        var excelWorksheet = excelPackage.Workbook.Worksheets.Copy(originalWorksheet.Name, item.LocationName);
                        excelWorksheet.Cells["A5"].Value = GetData(excelWorksheet.Cells["A5"].Value, new[] { request.EducationLevel.GetDescription(), item.LocationName });
                        int startRow = 9;

                        foreach (var reportPt in item.ReportPlacementTestEventSchools)
                        {
                            excelWorksheet.Cells[startRow, 1].Value = item.ReportPlacementTestEventSchools.IndexOf(reportPt) + 1;
                            excelWorksheet.Cells[startRow, 2].Value = reportPt.SchoolName;
                            excelWorksheet.Cells[startRow, 3].Value = reportPt.NumberValidStudentAccount;
                            excelWorksheet.Cells[startRow, 4].Value = reportPt.NumberStudentsCompletedPT;
                            excelWorksheet.Cells[startRow, 5].Value = reportPt.CompletionRate + "%";
                            if (reportPt.ReportCourseLevels != null)
                            {
                                var rowReportLevel = 6;
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

        private static string GetData(object data, object[]? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param);
        }

        private class ReportCourseLevelPTModel
        {
            public EnumCourseLevel CourseLevel { get; set; }
            public double Percent { get; set; }
        }

        private class PlacementTestResultReportGroupModel
        {
            public Guid StudentId { get; set; }
            public bool IsDonePT { get; set; }
            public EnumCourseLevel? CourseLevel { get; set; }
        }
    }
}
