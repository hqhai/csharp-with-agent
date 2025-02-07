// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.ExportReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.QueryModel;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.QueryModels;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.Repositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Collections.Concurrent;
    using System.Drawing;

    public class ExportReportSurveyQuestionEventSchoolQuery : IRequest<MethodResult<Stream>>
    {
        public string? HighEventCode { get; set; }
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportSurveyQuestionEventSchoolQueryHandler : IRequestHandler<ExportReportSurveyQuestionEventSchoolQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public ExportReportSurveyQuestionEventSchoolQueryHandler(IUserService userService,
            ICourseService courseService,
            IServiceProvider serviceProvider,
            ISurveyQuestionRepository surveyQuestionRepository)
        {
            _userService = userService;
            _courseService = courseService;
            _serviceProvider = serviceProvider;
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportSurveyQuestionEventSchoolQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var competitionEventResults = await _userService.GetEventToEventCodeStrAsync(new GetReportCompetitionEventQueryModel
            {
                EventCodeStr = request.HighEventCode
            });
            var competitionEvents = competitionEventResults.Content?.Result;
            if (competitionEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventCodeStr));
                return methodResult;
            }
            var reportCompetitionEventResults = await _userService.GetReportCompetitionEventSchoolAsync(new GetReportCompetitionEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                EventCodeStr = request.EventCodeStr,
                DistrictName = request.DistrictName,
            });

            var reportCompetitionEvents = reportCompetitionEventResults?.Content?.Result;
            if (reportCompetitionEvents == null)
            {
                return methodResult;
            }
            var userIds = reportCompetitionEvents.SelectMany(x => x.UserIds ?? new List<Guid>()).ToList();
            var listCustomerSurveyGroup = new ConcurrentStack<CustomerSurveyGroup>();
            var listQuestionReport = new ConcurrentStack<CustomerSurveyQuestionReportModel>();
            var listSubQuestionReport = new ConcurrentStack<CustomerSurveyQuestionReportModel>();

            // Chia danh sách thành từng nhóm
            var batches = userIds
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / ValueSettings.BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            await Parallel.ForEachAsync(batches, async (batche, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var customerSurveyGroupRepository = scope.ServiceProvider.GetRequiredService<ICustomerSurveyGroupRepository>();
                    var customerSurveyGroups = await customerSurveyGroupRepository.Queryable.Where(x => batche.Contains(x.UserId) && x.Status != EnumSurveyGroupStatus.Process)
                                                       .Where(x => x.CompetitionEventId.HasValue && competitionEvents.Select(x => x.Id).Contains(x.CompetitionEventId.Value))
                                                       .ToArrayAsync(cancellationToken);
                    listCustomerSurveyGroup.PushRange(customerSurveyGroups);
                }
            });
            // Chia danh sách thành từng nhóm
            var batchetCustomerSurveys = listCustomerSurveyGroup
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / ValueSettings.BatchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            var surveyQuestions = await _surveyQuestionRepository.Queryable.Where(x => x.CompetitionEventId.HasValue && competitionEvents.Select(x => x.Id).Contains(x.CompetitionEventId.Value))
                                                              .OrderBy(x => x.DisplayLevel)
                                                              .ThenBy(x => x.DisplayOrder)
                                                              .ToListAsync(cancellationToken);
            await Parallel.ForEachAsync(batchetCustomerSurveys, async (batchetCustomerSurvey, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var customerSurveyRepository = scope.ServiceProvider.GetRequiredService<ICustomerSurveyRepository>();
                    var customerSurveyGroups = await customerSurveyRepository.Queryable.Where(x => x.CustomerSurveyGroupId.HasValue && batchetCustomerSurvey.Select(x => x.Id).Contains(x.CustomerSurveyGroupId.Value))
                                                                                       .ToListAsync(cancellationToken);
                    foreach (var surveyQuestion in surveyQuestions)
                    {
                        var answers = ConvertHelper.Deserialize<IList<ChooseDirectionQuestionAnswers>>(surveyQuestion.Answers);
                        var answerIds = answers?.Select(a => a.Id).ToList();
                        var customerSurveyQuestions = customerSurveyGroups.Where(x => x.SurveyQuestionId == surveyQuestion.Id).ToList();
                        var customerSurveyQuestionAnswers = customerSurveyQuestions.Select(x => new
                        {
                            UserId = x.UserId,
                            SurveyQuestionId = surveyQuestion.Id,
                            Answers = ConvertHelper.Deserialize<IList<ChooseDirectionQuestionAnswers>>(x.Answer) ?? new List<ChooseDirectionQuestionAnswers>()
                        }).ToList();

                        if (answers == null || !answers.Any())
                        {
                            continue;
                        }
                        listQuestionReport.PushRange(reportCompetitionEvents.SelectMany(report =>
                        {
                            return report.ReportCompetitionEventSchools.Select(reportSchool =>
                            {
                                var totalStudent = reportSchool.UserIds.Join(customerSurveyQuestionAnswers.ToList(), userId => userId, record => record.UserId, (userId, record) => record)
                                                                       .Count(x => x.Answers.Any());
                                return new CustomerSurveyQuestionReportModel
                                {
                                    LocationId = report.LocationId,
                                    SchoolId = reportSchool.SchoolId,
                                    DisplayLevel = surveyQuestion.DisplayLevel,
                                    DisplayOrder = surveyQuestion.DisplayOrder,
                                    SurveyQuestionId = surveyQuestion.Id,
                                    TotalStudent = totalStudent
                                };
                            });
                        }).ToArray());

                        listSubQuestionReport.PushRange(reportCompetitionEvents.SelectMany(um =>
                        {
                            return um.ReportCompetitionEventSchools.Select(reportSchool =>
                            {
                                return new CustomerSurveyQuestionReportModel
                                {
                                    LocationId = um.LocationId,
                                    SchoolId = reportSchool.SchoolId,
                                    DisplayLevel = surveyQuestion.DisplayLevel,
                                    DisplayOrder = surveyQuestion.DisplayOrder,
                                    SurveyQuestionId = surveyQuestion.Id,
                                    SurveyQuestionUserReports = answers.Select(a => a.Id)
                                                        .GroupJoin(
                                                            reportSchool.UserIds
                                                                .Join(customerSurveyQuestionAnswers, userId => userId, record => record.UserId, (userId, record) => record)
                                                                .SelectMany(x => x.Answers),
                                                            answerId => answerId,
                                                            studentAnswer => studentAnswer.Id,
                                                            (answerId, matchingAnswers) => new SurveyQuestionUserReportModel
                                                            {
                                                                Id = answerId,
                                                                TotalCount = matchingAnswers.Count()
                                                            }
                                                        ).ToList()
                                };
                            }).ToList();
                        }).ToArray());
                    }
                }
            });

            var customerSurveyQuestionReports = reportCompetitionEvents.SelectMany(reportEvent =>
            {
                var listQuestionReportLocation = listQuestionReport.Where(x => x.LocationId == reportEvent.LocationId);
                return reportEvent.ReportCompetitionEventSchools.Select(reportSchool =>
                {
                    var listQuestionReportSchool = listQuestionReportLocation.Where(x => x.SchoolId == reportSchool.SchoolId);
                    return new CustomerSurveyQuestionReportModel
                    {
                        LocationId = reportEvent.LocationId,
                        SchoolId = reportSchool.SchoolId,
                        SurveyQuestionUserReports = listQuestionReportSchool.GroupBy(x => new { x.SurveyQuestionId, x.DisplayOrder, x.DisplayLevel }).Select(report =>
                        {
                            return new SurveyQuestionUserReportModel
                            {
                                SurveyQuestionId = report.Key.SurveyQuestionId,
                                DisplayLevel = report.Key.DisplayLevel,
                                DisplayOrder = report.Key.DisplayOrder,
                                TotalCount = report.Sum(x => x.TotalStudent)
                            };
                        }).OrderBy(x => x.DisplayLevel).ThenBy(x => x.DisplayOrder).ToList()
                    };
                });
            }).ToList();

            var customerSubSurveyQuestionReports = reportCompetitionEvents.SelectMany(um =>
            {
                var subQuestionReports = listSubQuestionReport.Where(x => x.LocationId == um.LocationId);
                return um.ReportCompetitionEventSchools.Select(reportSchool =>
                {
                    return new CustomerSurveyQuestionReportModel
                    {
                        LocationId = um.LocationId,
                        SchoolId = reportSchool.SchoolId,
                        SurveyQuestionUserReports = subQuestionReports.Where(x => x.SchoolId == reportSchool.SchoolId)
                                                                      .SelectMany(report => report.SurveyQuestionUserReports
                                                                      .Select(userReport => new SurveyQuestionUserReportModel
                                                                      {
                                                                          Id = userReport.Id,
                                                                          TotalCount = userReport.TotalCount,
                                                                          DisplayLevel = report.DisplayLevel,
                                                                          DisplayOrder = report.DisplayOrder,
                                                                          SurveyQuestionId = report.SurveyQuestionId
                                                                      })).GroupBy(x => new { x.SurveyQuestionId, x.DisplayLevel, x.DisplayOrder, x.Id })
                                                                      .Select(userReport => new SurveyQuestionUserReportModel
                                                                      {
                                                                          Id = userReport.Key.Id,
                                                                          TotalCount = userReport.Sum(x => x.TotalCount),
                                                                          DisplayLevel = userReport.Key.DisplayLevel,
                                                                          DisplayOrder = userReport.Key.DisplayOrder,
                                                                          SurveyQuestionId = userReport.Key.SurveyQuestionId
                                                                      }).OrderBy(x => x.DisplayLevel).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id).ToList()
                    };
                });
            }).ToList();

            var reportPlacementTestEventResults = await _courseService.GetReportPlacementTestEventSchoolAsync(new GetReportPlacementTestEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                EventCodeStr = request.EventCodeStr,
            });
            var reportPlacementTestEvents = reportPlacementTestEventResults.Content?.Result;
            var surveyQuestionReportDistricts = new ConcurrentBag<SurveyQuestionReportDistrictModel>();

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var reportPlacementTestEvent = reportPlacementTestEvents?.FirstOrDefault(x => x.LocationName == reportCompetitionEvent.DistrictName);
                var reportSurveyQuestionEvents = customerSubSurveyQuestionReports?.Where(x => x.LocationId == reportCompetitionEvent.LocationId);
                var reportSurveyQuestionReports = customerSurveyQuestionReports?.Where(x => x.LocationId == reportCompetitionEvent.LocationId);
                var surveyQuestionReportDistrict = new SurveyQuestionReportDistrictModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberActualParticipatingSchool = reportPlacementTestEvent?.NumberActualParticipatingSchool ?? default,
                    NumberRegisteredSchool = reportPlacementTestEvent?.NumberRegisteredSchool ?? default,
                    NumberStudentsCompletedPT = reportPlacementTestEvent?.NumberStudentsCompletedPT ?? default,
                    NumberStudentVerified = reportCompetitionEvent.NumberStudentCompleteVerify,
                    NumberValidStudentAccount = reportPlacementTestEvent?.NumberValidStudentAccount ?? default,
                    SurveyQuestionReportSchools = reportPlacementTestEvent?.ReportPlacementTestEventSchools.Select(report =>
                    {
                        var reportSubSurveyQuestionSchool = reportSurveyQuestionEvents?.FirstOrDefault(x => x.SchoolId == report.SchoolId);
                        var reportSurveyQuestionSchool = reportSurveyQuestionReports?.FirstOrDefault(x => x.SchoolId == report.SchoolId);

                        return new SurveyQuestionReportSchoolModel
                        {
                            LocatonName = report.SchoolName,
                            NumberStudentsCompletedPT = report.NumberStudentsCompletedPT,
                            NumberValidStudentAccount = report.NumberValidStudentAccount,
                            NumberStudentVerified = report.NumberStudentVerifiedSchool,
                            SurveyQuestionUserReports = reportSurveyQuestionSchool?.SurveyQuestionUserReports ?? new List<SurveyQuestionUserReportModel>(),
                            SubSurveyQuestionUserReports = reportSubSurveyQuestionSchool?.SurveyQuestionUserReports ?? new List<SurveyQuestionUserReportModel>(),
                        };
                    }).ToList() ?? new List<SurveyQuestionReportSchoolModel>(),
                };
                surveyQuestionReportDistricts.Add(surveyQuestionReportDistrict);
            });

            methodResult.Result = ExportExcelTemplate(surveyQuestionReportDistricts.ToList(), surveyQuestions, request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<SurveyQuestionReportDistrictModel>? surveyQuestionReportDistricts, IList<SurveyQuestion> surveyQuestions, ExportReportSurveyQuestionEventSchoolQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(surveyQuestions);
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportSurveyQuestionEventSchool)))
            {
                var originalWorksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if (originalWorksheet == null)
                {
                    throw new InvalidOperationException("The Excel file does not contain any worksheets.");
                }
                if (surveyQuestionReportDistricts != null && surveyQuestionReportDistricts.Any())
                {
                    foreach (var item in surveyQuestionReportDistricts)
                    {
                        var excelWorksheet = excelPackage.Workbook.Worksheets.Copy(originalWorksheet.Name, item.LocationName);
                        UpdateTemplateExcel(excelWorksheet, surveyQuestions, request);
                        int startRow = 5;
                        if (item.SurveyQuestionReportSchools != null && item.SurveyQuestionReportSchools.Any())
                        {
                            foreach (var surveyQuestionReport in item.SurveyQuestionReportSchools)
                            {
                                excelWorksheet.Cells[startRow, 1].Value = item.SurveyQuestionReportSchools.IndexOf(surveyQuestionReport) + 1;
                                excelWorksheet.Cells[startRow, 2].Value = surveyQuestionReport.LocatonName;
                                excelWorksheet.Cells[startRow, 3].Value = surveyQuestionReport.NumberValidStudentAccount;
                                excelWorksheet.Cells[startRow, 4].Value = surveyQuestionReport.NumberStudentVerified;
                                excelWorksheet.Cells[startRow, 5].Value = surveyQuestionReport.PercentStudentsVerified + "%";
                                excelWorksheet.Cells[startRow, 6].Value = surveyQuestionReport.NumberStudentsCompletedPT;
                                excelWorksheet.Cells[startRow, 7].Value = surveyQuestionReport.CompletionRate + "%";
                                var rowReportLevel = 8;
                                if (surveyQuestionReport.SurveyQuestionUserReports != null)
                                {
                                    foreach (var reportLevel in surveyQuestionReport.SurveyQuestionUserReports)
                                    {
                                        excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalCount;
                                        rowReportLevel++;
                                    }
                                }
                                if (surveyQuestionReport.SubSurveyQuestionUserReports != null)
                                {
                                    foreach (var reportLevel in surveyQuestionReport.SubSurveyQuestionUserReports)
                                    {
                                        var numberQuestion = surveyQuestionReport.SurveyQuestionUserReports?.FirstOrDefault(x => x.SurveyQuestionId == reportLevel.SurveyQuestionId)?.TotalCount ?? default;
                                        excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalCount;
                                        excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = NumberHelper.GetPercent(reportLevel.TotalCount, numberQuestion) + "%";
                                        rowReportLevel += 2;
                                    }
                                }
                                startRow++;
                            }
                        }
                    }
                }

                excelPackage.SaveAs(memoryStream);
            }

            memoryStream.Position = 0L;
            return memoryStream;
        }

        private static void UpdateTemplateExcel(ExcelWorksheet excelWorksheet, IList<SurveyQuestion> surveyQuestions, ExportReportSurveyQuestionEventSchoolQuery request)
        {
            int currentColumn = 8;
            excelWorksheet.Cells["A1"].Value = Shared.Helpers.StringHelper.FormatStringWithParam(excelWorksheet.Cells["A1"].Value, $"{request.EducationLevel.GetDescription()}");
            var valueHearder = excelWorksheet.Cells[3, currentColumn].Value.ToString() ?? string.Empty;
            excelWorksheet.Cells[3, currentColumn].Value = Shared.Helpers.StringHelper.FormatStringWithParam(valueHearder, 1);
            for (int i = 1; i < surveyQuestions.Count; i++)
            {
                excelWorksheet.InsertColumn(currentColumn + 1, 1); // Chèn 1 cột sau cột B
                excelWorksheet.Cells[2, currentColumn + 1].Value = excelWorksheet.Cells[2, currentColumn].Value;
                excelWorksheet.Cells[2, currentColumn + 1].StyleID = excelWorksheet.Cells[2, currentColumn].StyleID;

                // Sao chép style từ ô hàng 3 của currentColumn
                excelWorksheet.Cells[3, currentColumn + 1].StyleID = excelWorksheet.Cells[3, currentColumn].StyleID;
                excelWorksheet.Cells[4, currentColumn + 1].StyleID = excelWorksheet.Cells[4, currentColumn].StyleID;
                // Gộp ô hàng 3 và hàng 4 của cột mới
                excelWorksheet.Cells[3, currentColumn + 1, 4, currentColumn + 1].Merge = true;
                // Sao chép style vào ô gộp
                excelWorksheet.Cells[3, currentColumn + 1].Value = Shared.Helpers.StringHelper.FormatStringWithParam(valueHearder, i + 1);
                currentColumn++;
            }

            int surveyColumn = currentColumn;
            foreach (var surveyQuestion in surveyQuestions)
            {
                var answers = ConvertHelper.Deserialize<IList<ChooseDirectionQuestionAnswers>>(surveyQuestion.Answers);
                if (answers == null || answers.Count == 0)
                {
                    continue;
                }
                for (int i = 1; i < answers.Count * 2 + 1; i++)
                {
                    if (i % 2 == 0)
                    {
                        excelWorksheet.InsertColumn(currentColumn + 1, 1); // Chèn 1 cột sau cột B
                        excelWorksheet.Cells[4, currentColumn + 1].Value = "%";
                    }
                    else
                    {
                        excelWorksheet.InsertColumn(currentColumn + 1, 1); // Chèn 1 cột sau cột B
                        excelWorksheet.Cells[4, currentColumn + 1].Value = "Option" + (answers.IndexOf(answers[i / 2]) + 1);
                    }

                    excelWorksheet.Cells[4, currentColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    excelWorksheet.Cells[4, currentColumn + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    currentColumn++;
                }
                excelWorksheet.Cells[3, currentColumn + 1 - (answers.Count * 2), 3, currentColumn].Merge = true;
                excelWorksheet.Cells[3, currentColumn + 1 - (answers.Count * 2)].Value = surveyQuestion.Question;
                excelWorksheet.Cells[3, currentColumn + 1 - (answers.Count * 2)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                excelWorksheet.Cells[3, currentColumn + 1 - (answers.Count * 2)].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            if (currentColumn == 8)
            {
                return;
            }
            excelWorksheet.Cells[1, 1, 1, currentColumn].Merge = true;
            excelWorksheet.Cells[2, surveyColumn + 1].Value = "Câu hỏi Khảo sát trên Hệ thống FSEL";
            excelWorksheet.Cells[2, surveyColumn + 1, 2, currentColumn].Merge = true;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelWorksheet.Cells[2, surveyColumn + 1, 4, currentColumn].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFCCCC"));
        }
    }
}
