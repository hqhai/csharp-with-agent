// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            QuestionConverter questionConverter,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            FinishOneHomeWorkPublisher finishOneHomeWorkPublisher,
            AnswerTypeConverter answerTypeConverter,
            IQuestionRepository questionRepository
            )
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _questionConverter = questionConverter;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _finishOneHomeWorkPublisher = finishOneHomeWorkPublisher;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var homeWorkResult = await _homeWorkResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId, cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultDone));
                return methodResult;
            }
            var questionIds = request.Answers.Select(x => x.QuestionId).Distinct().ToList();
            var questions = await _questionRepository.GetIncludeByHomeWorkAsync(questionIds);
            var homeWorkAnswers = new List<HomeWorkAnswer>();
            int correctTotal = default;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount) = questionResult.Result;
                var homeWorkQuestion = questionItem.HomeWorkQuestions.FirstOrDefault();
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                    return methodResult;
                }
                var homeWorkAnswer = await _homeWorkAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == homeWorkResult.Id, cancellationToken);
                correctTotal += correctCount;
                if (homeWorkAnswer == null)
                {
                    homeWorkAnswer = new HomeWorkAnswer();
                    homeWorkAnswer.HomeWorkQuestionId = homeWorkQuestion.Id;
                    homeWorkAnswer.HomeWorkResultId = homeWorkResult.Id;
                    homeWorkResult.HomeWorkAnswers.Add(homeWorkAnswer);
                }
                else
                {
                    homeWorkAnswers.Add(homeWorkAnswer);
                }
                homeWorkAnswer.Answer = answerConfig;
                homeWorkAnswer.CorrectCount = request.IsSubmit ? correctCount : default;
            }

            var homeWork = await _homeWorkRepository.Queryable
                            .Include(x => x!.HomeWorkQuestions)
                            .ThenInclude(x => x.Question)
                            .Where(x => x.Id == homeWorkResult.HomeWorkId).FirstOrDefaultAsync(cancellationToken);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            var listQuestion = homeWork.HomeWorkQuestions.Select(x => x.Question).ToList();
            if (request.Answers.Count == listQuestion.Count && request.IsSubmit)
            {
                homeWorkResult.CorrectCount = correctTotal;
                homeWorkResult.Status = EnumResultStatus.Done;
                homeWorkResult.Percent = NumberHelper.GetPercent(homeWorkResult.CorrectCount, homeWorkResult.CorrectTotal);
                var skillScores = new SkillScores
                {
                    Skill = homeWork.CourseSkill,
                    CorrectCount = homeWorkResult.CorrectCount,
                    TotalCount = listQuestion.Sum(x => x!.CorrectTotal),
                    CountQuestion = request.Answers.Count,
                    TotalQuestion = listQuestion.Count,
                    Percent = homeWorkResult.Percent,
                    Scores = 0
                };
                homeWorkResult.SkillScores = new List<SkillScores> { skillScores };

                await _finishOneHomeWorkPublisher.Publish(homeWorkResult, cancellationToken);
            }
            else
            {
                homeWorkResult.Status = EnumResultStatus.Process;
                homeWorkResult.CorrectCount = correctTotal;
            }

            #endregion Validation

            await _homeWorkAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                if (homeWorkAnswers.Any())
                {
                    _homeWorkAnswerRepository.UpdateList(homeWorkAnswers);
                    await _homeWorkAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
