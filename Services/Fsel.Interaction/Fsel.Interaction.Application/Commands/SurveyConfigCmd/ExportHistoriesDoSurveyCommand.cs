using System.Collections.Concurrent;
using System.Globalization;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Interaction.Application.Services.UserServices;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Interaction.Domain.Models.EntityModels;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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
        private readonly IMapper _mapper;

        public ExportHistoriesDoSurveyCommandHandler(ISurveyConfigRepository surveyConfigRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserService userService, IMapper mapper)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userService = userService;
            _mapper = mapper;
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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;

            var templateFile = new FileInfo(ResourceSettings.HistoriesSurveyReport);
            var fileBytes = File.ReadAllBytes(templateFile.FullName);
            using var templateStream = new MemoryStream(fileBytes);

            var memoryStream = new MemoryStream();
            using var excelPackage = new ExcelPackage(templateStream);

            var excelWorksheet = excelPackage.Workbook.Worksheets[0];
            int startRow = 1;
            int startColumn = 6;

            surveyConfig.SurveyQuestions = surveyConfig.SurveyQuestions
                .OrderBy(p => p.DisplayLevel)
                .ThenBy(p => p.DisplayOrder)
                .ToList();

            foreach (var item in surveyConfig.SurveyQuestions)
            {
                excelWorksheet.Cells[1, startColumn++].Value = item.Question;
            }

            var dataBag = new ConcurrentBag<(int row, List<string> values)>();

            var tasks = userIds.Select(async (item, index) =>
            {
                var student = students.FirstOrDefault(x => x.Human != null && x.Human.UserId == item);

                var answers = customerSurveys
                    .Where(p => p.CustomerSurvey.UserId == item)
                    .ToList();

                List<string> rowValues = new List<string>()
                    {
                        answers.LastOrDefault()?.CustomerSurvey.CreatedDate
                            .ConvertTimeFromUtc(EnumCountryKey.Vietnam)
                            .ToString("yyyy-MM-dd HH:mm", cultureInfo) ?? string.Empty,

                        student != null ? (student.Human?.FullName ?? string.Empty) : "Not Found",
                        student != null ? (student.Human?.Email ?? string.Empty) : string.Empty,
                        student != null ? (student.Human?.User?.PhoneNumber ?? string.Empty) : string.Empty,
                        student != null ? (student.Human?.User?.UserName ?? string.Empty) : string.Empty,
                    };

                foreach (var question in surveyConfig.SurveyQuestions)
                {
                    var answerStr = answers
                        .FirstOrDefault(x => x.CustomerSurvey.SurveyQuestionId == question.Id)
                        ?.CustomerSurvey.Answer ?? string.Empty;

                    try
                    {
                        var answer = ConvertHelper.Deserialize<List<AnswerSurveyModel>>(answerStr);
                        rowValues.Add(JoinAnswersWithDot(answer));
                    }
                    catch
                    {
                        rowValues.Add(string.Empty);
                    }
                }

                dataBag.Add((startRow + index + 1, rowValues));
            }).ToArray();

            await Task.WhenAll(tasks);

            // Ghi dữ liệu
            var sortedData = dataBag.OrderBy(x => x.row);
            foreach (var (row, values) in sortedData)
            {
                for (int col = 1; col <= values.Count; col++)
                {
                    excelWorksheet.Cells[row, col].Value = values[col - 1];
                }
            }

            // Save vào memoryStream
            excelPackage.SaveAs(memoryStream);
            memoryStream.Position = 0;

            methodResult.Result = memoryStream;
            return methodResult;
        }

        public static string JoinAnswersWithDot(IList<AnswerSurveyModel>? answers)
        {
            if (answers == null || answers.Count == 0)
                return string.Empty;

            return string.Join(", ", answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Content))
                .Select(a => $"{a.Id}. {a.Content}"));
        }
    }
}
