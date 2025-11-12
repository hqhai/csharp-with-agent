// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestionQuery
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPtQuestionByIdsQuery : IRequest<MethodResult<IList<QuestionModel>>>
    {
        public Guid StudentId { get; set; }
        public IList<Guid> QuestionIds { get; set; } = new List<Guid>();
    }

    public class GetPtQuestionByIdsQueryHandler : IRequestHandler<GetPtQuestionByIdsQuery, MethodResult<IList<QuestionModel>>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IQuestionShuffleRepository _questionShuffleRepository;

        public GetPtQuestionByIdsQueryHandler(IQuestionRepository questionRepository,
            AnswerTypeConverter answerTypeConverter,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper,
            IQuestionShuffleRepository questionShuffleRepository)
        {
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _questionShuffleRepository = questionShuffleRepository;
        }

        public async Task<MethodResult<IList<QuestionModel>>> Handle(GetPtQuestionByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<QuestionModel>>();
            if (!request.QuestionIds.Any())
            {
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetQuestionAsync(request);
            return methodResult;
        }

        private async Task<IList<QuestionModel>?> GetQuestionAsync(GetPtQuestionByIdsQuery request)
        {
            ArgumentNullException.ThrowIfNull(request.QuestionIds);

            var questions = await _questionRepository.ReadQueryable.WhereBulkContains(request.QuestionIds, x => x.Id).ToListAsync();

            if (!questions.Any())
            {
                return null;
            }

            var listQuestion = new List<QuestionModel>();

            var listQuestionShuffle = new List<QuestionShuffle>();
            var questionShuffles = await _questionShuffleRepository.Queryable
                .Where(x => questions.Select(x => x!.Id).Contains(x.QuestionId) && x.StudentId == request.StudentId)
                .ToListAsync();
            foreach (var question in questions)
            {
                var questionModel = _mapper.Map<QuestionModel>(question);

                questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: false).Item1;

                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);
                (questionModel.Config, string? questionShuffleStr) =
                    _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, questionShuffle?.ShuffleConfigs);

                if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                {
                    if (questionShuffle == null)
                    {
                        questionShuffle = new QuestionShuffle { QuestionId = question.Id, ShuffleConfigStr = questionShuffleStr };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }

                    listQuestionShuffle.Add(questionShuffle);
                }


                listQuestion.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            return listQuestion;
        }
    }
}
