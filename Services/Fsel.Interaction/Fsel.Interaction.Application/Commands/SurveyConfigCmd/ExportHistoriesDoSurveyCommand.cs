using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Interaction.Application.Services.UserServices;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using OfficeOpenXml;
using static OfficeOpenXml.ExcelErrorValue;

namespace Fsel.Interaction.Application.Commands.SurveyConfigCmd
{
    public class ExportHistoriesDoSurveyCommand : IRequest<MethodResult<Stream>>
    {
        public Guid SurveyConfigId { get; set; }
    }

    public class ExportHistoriesDoSurveyCommandHandler : IRequestHandler<ExportHistoriesDoSurveyCommand, MethodResult<Stream>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly IUserService _userService;

        public ExportHistoriesDoSurveyCommandHandler(ISurveyConfigRepository surveyConfigRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserService userService)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userService = userService;
        }

        public async Task<MethodResult<Stream>> Handle(ExportHistoriesDoSurveyCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

            var customerSurveys = await (from cs in _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId)
                                         join csg in _customerSurveyGroupRepository.Queryable on cs.CustomerSurveyGroupId equals csg.Id
                                         where csg.Status == EnumSurveyGroupStatus.Done
                                         select new
                                         {
                                             CustomerSurvey = cs,
                                             CustomerSurveyGroup = csg
                                         }).ToListAsync(cancellationToken);

            var users = customerSurveys.GroupBy(p => p.CustomerSurvey.UserId);
            var userIds = users.Select(p => p.Key).ToList();
            var studentResults = await _userService.GetStudentByUserIdsAsync(userIds);
            var students = studentResults.Content?.Result;

            if (students == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            using var memoryStream = new MemoryStream();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            using var excelPackage = new ExcelPackage(new FileInfo(ResourceSettings.RevenueReport));

            var excelWorksheet = excelPackage.Workbook.Worksheets[0];
            int startRow = 1;
            int startColumn = 5;

            surveyConfig.SurveyQuestions = surveyConfig.SurveyQuestions.OrderBy(p => p.DisplayLevel).ThenBy(p => p.DisplayOrder).ToList();

            foreach (var item in surveyConfig.SurveyQuestions)
            {
                excelWorksheet.Cells[0, startColumn].Value = item.Question;
            }

            var dataBag = new ConcurrentBag<(int row, List<string> values)>();

            var tasks = students.Select(async (item, index) =>
            {
                if (item.Human != null)
                {
                    var answers = customerSurveys
                        .Where(p => p.CustomerSurvey.UserId == item.Human.UserId)
                        .ToList();

                    List<string> rowValues = new List<string>()
                    {
                        answers.LastOrDefault()?.CustomerSurvey.CreatedDate
                            .ConvertTimeFromUtc(EnumCountryKey.Vietnam)
                            .ToString("yyyy-MM-dd HH:mm", cultureInfo) ?? string.Empty,

                        item.Human.FullName ?? string.Empty,
                        item.Human.Email ?? string.Empty,
                        item.Human.FullName ?? string.Empty,
                        item.Human.User?.UserName ?? string.Empty
                    };

                    foreach (var question in surveyConfig.SurveyQuestions)
                    {
                        var answer = answers.FirstOrDefault(x => x.CustomerSurvey.SurveyQuestionId == question.Id)?.CustomerSurvey.Answer ?? string.Empty;

                        try
                        {
                            using var document = JsonDocument.Parse(answer.ToString() ?? string.Empty);
                            var content = document.RootElement[0].GetProperty("content").GetString() ?? string.Empty;
                            rowValues.Add(content);
                        }
                        catch
                        {
                            rowValues.Add(string.Empty);
                        }
                    }

                    lock (dataBag)
                    {
                        dataBag.Add((startRow + index, rowValues));
                    }
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            var sortedData = dataBag.OrderBy(x => x.row);

            foreach (var (row, values) in sortedData)
            {
                for (int col = 1; col <= values.Count; col++)
                {
                    excelWorksheet.Cells[row, col].Value = values[col - 1];
                }
            }

            return methodResult;
        }
    }
}
