// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.ExportReportQuery
{
    using System.Collections.Concurrent;
    using System.Drawing;
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
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ExportReportSurveyQuestionEventQuery : IRequest<MethodResult<Stream>>
    {
        public string? EventCodeStr { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }

    public class ExportReportSurveyQuestionEventQueryHandler : IRequestHandler<ExportReportSurveyQuestionEventQuery, MethodResult<Stream>>
    {
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;

        public ExportReportSurveyQuestionEventQueryHandler(IUserService userService,
            ICourseService courseService,
            IServiceProvider serviceProvider,
            ICustomerSurveyRepository customerSurveyRepository,
            ISurveyQuestionRepository surveyQuestionRepository)
        {
            _userService = userService;
            _courseService = courseService;
            _serviceProvider = serviceProvider;
            _customerSurveyRepository = customerSurveyRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
        }

        public class SubQuestionAnswerResultModel
        {
            public int SubQuestionId { get; set; }
            public int DisplayOrder { get; set; }
            public long CountStudent { get; set; }
        }

        public async Task<MethodResult<Stream>> Handle(ExportReportSurveyQuestionEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var competitionEventResults = await _userService.GetEventToEventCodeStrAsync(new GetReportCompetitionEventQueryModel
            {
                EventCodeStr = request.EventCodeStr
            });
            var competitionEvents = competitionEventResults.Content?.Result;
            if (competitionEvents == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.EventCodeStr));
                return methodResult;
            }
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
            var userIds = reportCompetitionEvents.SelectMany(x => x.UserIds ?? new List<Guid>()).ToList();
            var listCustomerSurveyGroup = new ConcurrentStack<CustomerSurveyGroup>();
            var listCustomerSurveyReport = new ConcurrentStack<CustomerSurveyUserReportModel>();
            var listCustomerSurveyQuestionReport = new ConcurrentBag<CustomerSurveyQuestionReportModel>();

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
                    var customerSurveyGroups = await customerSurveyGroupRepository.Queryable.Where(x => batche.Contains(x.UserId) && x.Status == EnumSurveyGroupStatus.Done)
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

            await Parallel.ForEachAsync(batchetCustomerSurveys, async (batchetCustomerSurvey, cancellationToken) =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var customerSurveyRepository = scope.ServiceProvider.GetRequiredService<ICustomerSurveyRepository>();
                    var surveyQuestionRepository = scope.ServiceProvider.GetRequiredService<ISurveyQuestionRepository>();
                    var surveyQuestions = await surveyQuestionRepository.Queryable.Where(x => x.CompetitionEventId.HasValue && competitionEvents.Select(x => x.Id).Contains(x.CompetitionEventId.Value))
                                                                                  .OrderBy(x => x.DisplayLevel)
                                                                                  .ThenBy(x => x.DisplayOrder)
                                                                                  .ToListAsync(cancellationToken);

                    var customerSurveyGroups = await customerSurveyRepository.Queryable.Where(x => x.CustomerSurveyGroupId.HasValue && batchetCustomerSurvey.Select(x => x.Id).Contains(x.CustomerSurveyGroupId.Value))
                                                                                       .ToListAsync(cancellationToken);
                    foreach (var surveyQuestion in surveyQuestions)
                    {
                        var answers = ConvertHelper.Deserialize<IList<ChooseDirectionQuestionAnswers>>(surveyQuestion.Answers);
                        var customerSurveyQuestions = customerSurveyGroups.Where(x => x.SurveyQuestionId == surveyQuestion.Id).ToList();
                        var customerSurveyQuestionAnswers = customerSurveyQuestions.Select(x => new
                        {
                            UserId = x.UserId,
                            Answers = ConvertHelper.Deserialize<IList<ChooseDirectionQuestionAnswers>>(x.Answer) ?? new List<ChooseDirectionQuestionAnswers>()
                        }).ToList();

                        if (answers == null)
                        {
                            continue;
                        }
                        listCustomerSurveyReport.PushRange(customerSurveyQuestionAnswers.SelectMany(x =>
                        {
                            return answers?.Select(answer =>
                            {
                                return new CustomerSurveyUserReportModel
                                {
                                    Id = answer.Id,
                                    DisplayLevel = surveyQuestion.DisplayLevel,
                                    SurveyQuestionId = surveyQuestion.Id,
                                    DisplayOrder = surveyQuestion.DisplayOrder,
                                    NumberSubQuestion = answers.Where(x => x.Id == answer.Id).Count(),
                                    UserId = x.UserId
                                };
                            }) ?? new List<CustomerSurveyUserReportModel>();
                        }).ToArray());
                    }
                }
            });
            var customerSurveyQuestionReports = reportCompetitionEvents.Select(um => new CustomerSurveyQuestionReportModel
            {
                LocationId = um.LocationId,
                SurveyQuestionUserReports = um.UserIds
                                              .Join(listCustomerSurveyReport.ToList(), userId => userId, record => record.UserId, (userId, record) => record)
                                              .GroupBy(x => new { x.DisplayLevel, x.DisplayOrder, x.Id, x.SurveyQuestionId })
                                              .Select(x => new SurveyQuestionUserReportModel
                                              {
                                                  Id = x.Key.Id,
                                                  DisplayLevel = x.Key.DisplayLevel,
                                                  DisplayOrder = x.Key.DisplayOrder,
                                                  SurveyQuestionId = x.Key.SurveyQuestionId,
                                                  TotalCount = x.Sum(x => x.NumberSubQuestion),
                                              }).OrderBy(x => x.DisplayLevel)
                                                .ThenBy(x => x.DisplayOrder)
                                                .ThenBy(x => x.Id)
                                                .ToList()
            }).ToList();

            var listGroupSurveyReport = listCustomerSurveyReport.GroupBy(x => new { x.Id, x.DisplayLevel, x.DisplayOrder }).Select(x => new CustomerSurveyUserReportModel
            {
                Id = x.Key.Id,
                DisplayLevel = x.Key.DisplayLevel,
                DisplayOrder = x.Key.DisplayOrder,
                NumberSubQuestion = x.Sum(x => x.NumberSubQuestion),
            }).ToList();

            var reportPlacementTestEventResults = await _courseService.GetReportPlacementTestEventAsync(new GetReportPlacementTestEventQueryModel
            {
                EducationLevel = request.EducationLevel,
                EventCodeStr = request.EventCodeStr,
            });
            var reportPlacementTestEvents = reportPlacementTestEventResults.Content?.Result;
            var surveyQuestionReportDistricts = new ConcurrentBag<SurveyQuestionReportDistrictModel>();

            Parallel.ForEach(reportCompetitionEvents, reportCompetitionEvent =>
            {
                var reportPlacementTestEvent = reportPlacementTestEvents?.FirstOrDefault(x => x.LocationName == reportCompetitionEvent.DistrictName);
                var groupSurveyReport = customerSurveyQuestionReports.FirstOrDefault(x => x.LocationId == reportCompetitionEvent.LocationId);

                var surveyQuestionReportDistrict = new SurveyQuestionReportDistrictModel
                {
                    LocationName = reportCompetitionEvent.DistrictName,
                    NumberActualParticipatingSchool = reportPlacementTestEvent?.NumberActualParticipatingSchool ?? default,
                    NumberRegisteredSchool = reportPlacementTestEvent?.NumberRegisteredSchool ?? default,
                    NumberStudentsCompletedPT = reportPlacementTestEvent?.NumberStudentsCompletedPT ?? default,
                    NumberStudentVerified = reportCompetitionEvent.NumberStudentVerifiedDistrict,
                    NumberValidStudentAccount = reportPlacementTestEvent?.NumberValidStudentAccount ?? default,
                    NumberStudentsCompletedSurvey = listCustomerSurveyGroup.Where(x => reportCompetitionEvent.UserIds != null && reportCompetitionEvent.UserIds.Contains(x.UserId)).Count(),
                    SurveyQuestionUserReports = groupSurveyReport?.SurveyQuestionUserReports.ToList() ?? new List<SurveyQuestionUserReportModel>()
                };
                surveyQuestionReportDistricts.Add(surveyQuestionReportDistrict);
            });
            var surveyQuestions = await _surveyQuestionRepository.Queryable.Where(x => x.CompetitionEventId.HasValue && competitionEvents.Select(x => x.Id).Contains(x.CompetitionEventId.Value))
                                                                           .OrderBy(x => x.DisplayLevel)
                                                                           .ThenBy(x => x.DisplayOrder)
                                                                           .ToListAsync(cancellationToken);

            methodResult.Result = ExportExcelTemplate(surveyQuestionReportDistricts.ToList(), surveyQuestions, request);
            return methodResult;
        }

        public static Stream ExportExcelTemplate(IList<SurveyQuestionReportDistrictModel>? reportPlacementTestEvents, IList<SurveyQuestion> surveyQuestions, ExportReportSurveyQuestionEventQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(surveyQuestions);
            MemoryStream memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.ReportSurveyQuestionEvent)))
            {
                var excelWorksheet = excelPackage.Workbook.Worksheets[0];
                UpdateTemplateExcel(excelWorksheet, surveyQuestions, request);
                int startRow = 5;
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
                        excelWorksheet.Cells[startRow, 9].Value = item.NumberStudentsCompletedSurvey;
                        excelWorksheet.Cells[startRow, 10].Value = item.PercentageCompletionSurvey + "%";
                        if (item.SurveyQuestionUserReports != null)
                        {
                            var rowReportLevel = 11;
                            foreach (var reportLevel in item.SurveyQuestionUserReports)
                            {
                                excelWorksheet.Cells[startRow, rowReportLevel].Value = reportLevel.TotalCount;
                                excelWorksheet.Cells[startRow, rowReportLevel + 1].Value = NumberHelper.GetPercent(reportLevel.TotalCount, item.NumberStudentsCompletedSurvey) + "%";
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

        private static void UpdateTemplateExcel(ExcelWorksheet excelWorksheet, IList<SurveyQuestion> surveyQuestions, ExportReportSurveyQuestionEventQuery request)
        {
            int currentColumn = 9;
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
