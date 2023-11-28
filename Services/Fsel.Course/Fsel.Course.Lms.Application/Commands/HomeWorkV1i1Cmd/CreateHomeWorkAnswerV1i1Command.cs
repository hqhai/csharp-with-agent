// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkV1i1Cmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerV1i1Command : CreateHomeWorkAnswerV1i1CommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerV1i1CommandHandler : IRequestHandler<CreateHomeWorkAnswerV1i1Command, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkAnswerV1i1CommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            QuestionConverter questionConverter,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            FinishOneHomeWorkPublisher finishOneHomeWorkPublisher,
            IQuestionRepository questionRepository
            )
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _questionConverter = questionConverter;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _finishOneHomeWorkPublisher = finishOneHomeWorkPublisher;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerV1i1Command request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            if (request.Answers == null || !request.Answers.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            #region Validation

            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (listQuestion, questions, homeWorkResult, homeWork) = method.Result;
            var homeWorkAnswers = new List<HomeWorkAnswer>();
            int correctTotal = default;
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
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
                homeWorkAnswer.CorrectCount = correctCount;
                homeWorkAnswer.IsCorrect = request.IsSubmit ? correctCount == questionItem.CorrectTotal : null;
            }

            if (request.IsSubmit)
            {
                homeWorkResult.CorrectCount = correctTotal;
                homeWorkResult.Status = EnumResultStatus.Done;
                var skillScores = new SkillScores
                {
                    Skill = homeWork.CourseSkill,
                    CorrectCount = homeWorkResult.CorrectCount,
                    TotalCount = listQuestion.Sum(x => x!.CorrectTotal),
                    CountQuestion = request.Answers.Count,
                    TotalQuestion = listQuestion.Count,
                };
                homeWorkResult.SkillScores = new List<SkillScores> { skillScores };

                await _finishOneHomeWorkPublisher.Publish(homeWorkResult, cancellationToken);
            }
            else
            {
                homeWorkResult.Status = EnumResultStatus.Process;
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

        public async Task<MethodResult<(IList<Question>, IList<Question>, HomeWorkResult, HomeWork)>> Validate(CreateHomeWorkAnswerV1i1Command request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<(IList<Question>, IList<Question>, HomeWorkResult, HomeWork)>();
            if (request.Answers == null || !request.Answers.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var homeWorkResult = await _homeWorkResultRepository.GetByIdAsync(request.HomeWorkResultId);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            var questionIds = request.Answers.Select(x => x.QuestionId).Distinct().ToList();
            if (questionIds == null || !questionIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionIds));
                return methodResult;
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
            var listQuestion = homeWork.HomeWorkQuestions.Select(x => x.Question!).ToList();
            if (request.Answers.Count != listQuestion.Count && request.IsSubmit)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.NotAnsweredEnough), nameof(request.Answers));
                return methodResult;
            }
            var questions = await _questionRepository.GetIncludeByHomeWorkAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            methodResult.Result = (listQuestion, questions, homeWorkResult, homeWork);
            return methodResult;
        }
    }
}
