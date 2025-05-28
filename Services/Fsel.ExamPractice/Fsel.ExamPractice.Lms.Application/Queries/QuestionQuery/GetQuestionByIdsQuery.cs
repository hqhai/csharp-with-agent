// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.QuestionQuery
{
    using System.Text.Json.Serialization;
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

        public GetQuestionByIdQueryHandler(IQuestionRepository questionRepository,
            IMapper mapper,
            IExamPracticeResultRepository examPracticeResultRepository)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
            _examPracticeResultRepository = examPracticeResultRepository;
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
            var questions = await _questionRepository.Queryable
                                        .Include(x => x.ExamPracticeAnswers.Where(x => examPracticeResult != null && x.ExamPracticeResultId == examPracticeResult.Id))
                                        .WhereBulkContains(request.ListQuestionIds, x => x.Id)
                                        .OrderBy(x => x.CreatedDate)
                                        .ToListAsync();
            if (questions == null || !questions.Any())
            {
                return default;
            }
            return GetQuestionsAsync(questions, examPracticeResult);
        }

        private IList<QuestionModel> GetQuestionsAsync(IList<Question>? questions, ExamPracticeResult examPracticeResult)
        {
            var listQuestion = new List<QuestionModel>();
            if (questions == null || !questions.Any())
            {
                return new List<QuestionModel>();
            }
            return questions.Select(question =>
            {
                bool isShowAnswer = examPracticeResult.Status == EnumResultStatus.Done;
                var answer = question.ExamPracticeAnswers.FirstOrDefault();
                var questionModel = _mapper.Map<QuestionModel>(question);

                questionModel.Config = QuestionTypeHelper.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !isShowAnswer).Item1;
                questionModel.CorrectStatus = GetCorrectStatus(_mapper.Map<BaseAnswer>(answer));
                if (answer != null)
                {
                    var answerDto = _mapper.Map<AnswerModel>(answer);
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
