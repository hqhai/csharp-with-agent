// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestionQuery
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public class GetPtQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid TestResultId { get; set; }
        public Guid StudentId { get; set; }
        public IList<QuestionAnswer> QuestionAnswerIds { get; set; } = new List<QuestionAnswer>();
    }

    public class QuestionAnswer
    {
        public Guid QuestionId { get; set; }
        public Guid AnswerId { get; set; }
    }

    public class GetPtQuestionByIdsQueryHandler : IRequestHandler<GetPtQuestionByIdsQuery, MethodResult<IList<QuestionModel>>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IQuestionShuffleRepository _questionShuffleRepository;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public GetPtQuestionByIdsQueryHandler(IQuestionRepository questionRepository,
            AnswerTypeConverter answerTypeConverter,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            IQuestionShuffleRepository questionShuffleRepository,
            IRepository<TestAnswer> testAnswerRepository,
            ITestResultRepository testResultRepository)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _questionShuffleRepository = questionShuffleRepository;
            _testAnswerRepository = testAnswerRepository;
            _testResultRepository = testResultRepository;
        }

        public async Task<MethodResult<IList<QuestionModel>>> Handle(GetPtQuestionByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestionModel>>();
            if (!request.QuestionAnswerIds.Any())
            {
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<IList<QuestionModel>?> GetQuestionAsync(GetPtQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.QuestionAnswerIds);
            var testResult = await _testResultRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.TestResultId);
            if (testResult == null)
            {
                return null;
            }

            var questions = await _questionRepository.ReadQueryable.WhereBulkContains(request.QuestionAnswerIds.Select(x => x.QuestionId), x => x.Id).ToListAsync();

            if (!questions.Any())
            {
                return null;
            }

            var testAnswers = await _testAnswerRepository.ReadQueryable.WhereBulkContains(request.QuestionAnswerIds.Select(x => x.AnswerId), x => x.Id).ToListAsync();

            var listQuestion = new List<QuestionModel>();

            var listQuestionShuffle = new List<QuestionShuffle>();
            var questionShuffles = await _questionShuffleRepository.Queryable
                .Where(x => questions.Select(y => y!.Id).Contains(x.QuestionId) && x.StudentId == request.StudentId)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
            foreach (var question in questions)
            {
                var questionModel = _mapper.Map<QuestionModel>(question);
                var isResultDone = testResult.Status == EnumResultStatus.Done;

                questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: false).Item1;

                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);
                (questionModel.Config, string? questionShuffleStr) =
                    _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, isResultDone, questionShuffle?.ShuffleConfigs);

                if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                {
                    if (questionShuffle == null)
                    {
                        questionShuffle = new QuestionShuffle
                        {
                            QuestionId = question.Id,
                            ShuffleConfigStr = questionShuffleStr,
                            StudentId = testResult.StudentId,
                        };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }

                    listQuestionShuffle.Add(questionShuffle);
                }

                var answer = testAnswers.FirstOrDefault(x => x.QuestionId == question.Id);
                if (answer != null)
                {
                    var answerDto = _mapper.Map<AnswerModel>(answer);
                    if (testResult.Status != EnumResultStatus.Done)
                    {
                        answerDto.IsCorrect = null;
                        answerDto.Status = EnumAnswerStatus.Process;
                    }
                    answerDto.Answer = _answerTypeConverter.AnswerTypeConverterObject(answerDto.Answer, question.QuestionType, false, testResult.Status, isResultDone);

                    questionModel.CorrectStatus = GetCorrectStatus(_mapper.Map<BaseAnswer>(answer), testResult.Status);
                    questionModel.ResultAnswer = answerDto;
                }

                listQuestion.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            return listQuestion;
        }

        private static EnumCorrectStatus? GetCorrectStatus(BaseAnswer? answer, EnumResultStatus resultStatus)
        {
            EnumCorrectStatus? status = null;
            if (answer != null)
            {
                status = EnumCorrectStatus.Process;
                if (answer.Status == EnumAnswerStatus.Done && resultStatus == EnumResultStatus.Done)
                {
                    status = answer.IsCorrect.HasValue && answer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                }
            }

            return status;
        }
    }
}
