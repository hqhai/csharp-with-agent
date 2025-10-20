// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.QuestionQuery
{
    using System.Linq.Dynamic.Core;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Bases;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid ObjectResultId { get; set; }

        public IList<string> QuestionIds { get; set; } = new List<string>();

        [JsonIgnore]
        public IList<Guid> ListQuestionIds
        { get { return QuestionIds.ToList<Guid>(); } }
    }

    public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdsQuery, MethodResult<IList<QuestionModel>>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeAnswerRepository _examPracticeAnswerRepository;

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository,
            IMapper mapper,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeAnswerRepository examPracticeAnswerRepository)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeAnswerRepository = examPracticeAnswerRepository;
        }

        public async Task<MethodResult<IList<QuestionModel>>> Handle(GetQuestionByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestionModel>>();
            if (request.ListQuestionIds == null || !request.ListQuestionIds.Any())
            {
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<IList<QuestionModel>?> GetQuestionAsync(GetQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.ListQuestionIds);
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(request.ObjectResultId);
            if (examPracticeResult == null)
            {
                return default;
            }
            var questions = await _questionRepository.Queryable.AsNoTracking()
                                        .WhereBulkContains(request.ListQuestionIds, x => x.Id)
                                        .OrderBy(x => x.DisplayOrder)
                                        .ThenBy(x => x.CreatedDate)
                                        .ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return await GetQuestionsAsync(questions, examPracticeResult);
        }

        private async Task<IList<QuestionModel>> GetQuestionsAsync(IList<Question>? questions, ExamPracticeResult examPracticeResult)
        {
            var listQuestion = new List<QuestionModel>();
            if (questions == null || !questions.Any())
            {
                return new List<QuestionModel>();
            }
            var questionIds = questions.Select(x => x.Id).ToList();

            var examPracticeAnswers = await GetAnswersAsync(examPracticeResult.Id, questionIds);
            var answerByQuestionId = BuildAnswerIndex(examPracticeAnswers);
            var isShowAnswer = examPracticeResult.Status == EnumResultStatus.Done;

            return questions.Select(question =>
            {
                answerByQuestionId.TryGetValue(question.Id, out var examPracticeAnswer);

                var questionModel = _mapper.Map<QuestionModel>(question);
                questionModel.Config = QuestionTypeHelper.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1;
                questionModel.CorrectStatus = GetCorrectStatus(_mapper.Map<BaseAnswer>(examPracticeAnswer));
                if (examPracticeAnswer != null)
                {
                    var answerDto = _mapper.Map<AnswerModel>(examPracticeAnswer);
                    answerDto.Answer = AnswerTypeHelper.AnswerTypeConverterObject(answerDto.Answer, question.QuestionType, false, examPracticeResult.Status, isShowAnswer);
                    if (!isShowAnswer)
                    {
                        answerDto.Status = EnumAnswerStatus.Process;
                        answerDto.CorrectCount = default;
                        answerDto.IsCorrect = null;
                    }
                    questionModel.ResultAnswer = answerDto;
                }
                return questionModel;
            }).ToList();
        }

        private async Task<List<ExamPracticeAnswer>> GetAnswersAsync(Guid examPracticeResultId, IList<Guid> questionIds)
        {
            // Nếu WhereBulkContains không hỗ trợ Guid? -> dùng Contains với .Value (đã filter HasValue)
            return await _examPracticeAnswerRepository.Queryable
                .AsNoTracking()
                .Where(a => a.ExamPracticeResultId == examPracticeResultId && a.QuestionId.HasValue)
                .WhereBulkContains(questionIds, a => a.QuestionId) // hoặc: .Where(a => questionIds.Contains(a.QuestionId!.Value))
                .ToListAsync();
        }

        private static Dictionary<Guid, ExamPracticeAnswer?> BuildAnswerIndex(IEnumerable<ExamPracticeAnswer> answers)
        {
            // Nếu có CreatedDate/UpdatedDate: lấy bản mới nhất; nếu không, lấy First()
            return answers
                .GroupBy(a => a.QuestionId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.CreatedDate).FirstOrDefault() // nếu không có CreatedDate, đổi thành g.FirstOrDefault()
                );
        }

        private static EnumCorrectStatus? GetCorrectStatus(BaseAnswer? answer)
        {
            EnumCorrectStatus? status = null;
            if (answer != null)
            {
                status = EnumCorrectStatus.Process;
                if (answer.Status == EnumAnswerStatus.Done)
                {
                    status = answer.IsCorrect.HasValue && answer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                }
            }
            return status;
        }
    }
}
