// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd.V1i1
{
    using System.Linq;
    using System.Threading;
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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerV1i1CommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
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

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var method = await Validate(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (questions, homeWorkResult) = method.Result;
            var methodSave = await SaveAnswer(homeWorkResult, questions, request, cancellationToken);
            if (!methodSave.IsOK)
            {
                methodResult.AddErrorBadRequest(methodSave.ErrorMessages);
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status201Created;
            methodResult = await UpdateHomeWorkResult(homeWorkResult, request.IsSubmit, cancellationToken);
            return methodResult;
        }

        private async Task<MethodResult<bool>> SaveAnswer(HomeWorkResult? homeWorkResult, IList<Question>? questions, CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<bool>();
            var isTryAgain = homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;
            var createHomeWorkAnswers = new List<HomeWorkAnswer>();
            var updateHomeWorkAnswers = new List<HomeWorkAnswer>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, default, isTryAgain, request.IsSubmit);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                var homeWorkQuestion = questionItem.HomeWorkQuestions.FirstOrDefault();
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                    return methodResult;
                }
                var homeWorkAnswer = await _homeWorkAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == homeWorkResult.Id, cancellationToken);
                if (homeWorkAnswer == null)
                {
                    homeWorkAnswer = new HomeWorkAnswer
                    {
                        HomeWorkQuestionId = homeWorkQuestion.Id,
                        HomeWorkResultId = homeWorkResult.Id
                    };
                    createHomeWorkAnswers.Add(homeWorkAnswer);
                }
                else
                {
                    updateHomeWorkAnswers.Add(homeWorkAnswer);
                }
                homeWorkAnswer.Status = GetStatusAnswer(request.IsSubmit, homeWorkResult);
                homeWorkAnswer.Answer = answerConfig;
                homeWorkAnswer.CorrectCount = correctCount;
                homeWorkAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
            }
            if (createHomeWorkAnswers.Any())
            {
                await _homeWorkAnswerRepository.AddList(createHomeWorkAnswers);
                await _homeWorkAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (updateHomeWorkAnswers.Any())
            {
                _homeWorkAnswerRepository.UpdateList(updateHomeWorkAnswers);
                await _homeWorkAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            return methodResult;
        }

        private static EnumAnswerStatus GetStatusAnswer(bool isSubmit, HomeWorkResult homeWorkResult)
        {
            return isSubmit && homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
        }

        private async Task<MethodResult<bool>> UpdateHomeWorkResult(HomeWorkResult? homeWorkResult, bool isSubmit, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            var methodResult = new MethodResult<bool>();
            var homeWorkQuestionCount = await _homeWorkResultRepository.Queryable.Where(x => x.Id == homeWorkResult.Id).Select(x => new
            {
                CourseSkill = x.HomeWork!.CourseSkill,
                CorrectCount = x.HomeWorkAnswers.Sum(x => x.CorrectCount),
                CorrectTotal = x.HomeWork.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                TotalQuestion = x.HomeWork.HomeWorkQuestions.Count,
                TotalAnswer = x.HomeWorkAnswers.Count,
            }).FirstOrDefaultAsync(cancellationToken);

            if (homeWorkQuestionCount != null)
            {
                if (isSubmit)
                {
                    if (homeWorkQuestionCount.TotalAnswer != homeWorkQuestionCount.TotalQuestion)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.QuestionNotCompleted));
                        return methodResult;
                    }
                    if (homeWorkQuestionCount.CorrectCount == homeWorkQuestionCount.CorrectTotal)
                    {
                        homeWorkResult = await GetHomeWorkResult(homeWorkResult, homeWorkQuestionCount, cancellationToken);
                    }
                    else
                    {
                        homeWorkResult.SubmissionCount = EnumSubmissionCount.SecondSubmit;
                    }
                }
                else
                {
                    homeWorkResult.Status = EnumResultStatus.Process;
                }
                _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<HomeWorkResult> GetHomeWorkResult(HomeWorkResult homeWorkResult, dynamic homeWorkQuestionCount, CancellationToken cancellationToken)
        {
            homeWorkResult.CorrectCount = homeWorkQuestionCount.CorrectCount;
            homeWorkResult.CorrectTotal = homeWorkQuestionCount.CorrectTotal;
            homeWorkResult.Status = EnumResultStatus.Done;
            var skillScores = new SkillScores
            {
                Skill = homeWorkQuestionCount.CourseSkill,
                CorrectCount = homeWorkQuestionCount.CorrectCount,
                TotalCount = homeWorkQuestionCount.CorrectTotal,
                CountQuestion = homeWorkQuestionCount.TotalAnswer,
                TotalQuestion = homeWorkQuestionCount.TotalQuestion,
            };
            homeWorkResult.SkillScores = new List<SkillScores> { skillScores };
            await _finishOneHomeWorkPublisher.Publish(homeWorkResult, cancellationToken);
            return homeWorkResult;
        }

        public async Task<MethodResult<(IList<Question>, HomeWorkResult)>> Validate(CreateHomeWorkAnswerCommand request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<(IList<Question>, HomeWorkResult)>();
            if (request.Answers == null || !request.Answers.Any())
            {
                if (request.IsSubmit)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                }
                else
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                }
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
            var listQuestion = request.Answers.Select(x => x.QuestionId).GroupBy(x => x).Select(x => new
            {
                QuestionId = x.Key,
                TotalQuestion = x.Count()
            }).ToList();

            if (listQuestion.Any(x => x.TotalQuestion > 1))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.QuestionNotCompleted), nameof(listQuestion));
                return methodResult;
            }

            var questionIds = listQuestion.Select(x => x.QuestionId).ToList();
            var homeWork = await _homeWorkRepository.GetByIdAsync(homeWorkResult.HomeWorkId);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            var questions = await _questionRepository.GetIncludeByHomeWorkAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            methodResult.Result = (questions, homeWorkResult);
            return methodResult;
        }
    }
}
