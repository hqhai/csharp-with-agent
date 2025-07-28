// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.FinalTestCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.FinalTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateFinalTestAnswerCommand : CreateFinalTestAnswerCommandModel, IRequest<MethodResult<FinalTestResultModel>>
    {
    }

    public class CreateFinalTestAnswerCommandHandler : IRequestHandler<CreateFinalTestAnswerCommand, MethodResult<FinalTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IFinalTestAnswerRepository _finalTestAnswerRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public CreateFinalTestAnswerCommandHandler(
            IQuestionRepository questionRepository
            , IFinalTestAnswerRepository finalTestAnswerRepository
            , IFinalTestResultRepository finalTestResultRepository
            , IFinalTestRepository finalTestRepository
            , IMapper mapper
            , AuthContext authContext
            , IUserService userService
            , ICourseRepository courseRepository,
              QuestBoardPublisher questBoardPublisher,
              QuestionConverter questionConverter)
        {
            _questionRepository = questionRepository;
            _finalTestAnswerRepository = finalTestAnswerRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _finalTestRepository = finalTestRepository;
            _mapper = mapper;
            _authContext = authContext;
            _userService = userService;
            _courseRepository = courseRepository;
            _questBoardPublisher = questBoardPublisher;
            _questionConverter = questionConverter;
        }

        public async Task<MethodResult<FinalTestResultModel>> Handle(CreateFinalTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FinalTestResultModel> methodResult = new MethodResult<FinalTestResultModel>();

            #region Validation

            if (request.FinalTestAnswers == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var finalTest = await _finalTestRepository.GetByIdAsync(request.FinalTestId);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTest));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var finalTestResult = await _finalTestResultRepository.Queryable
                    .FirstOrDefaultAsync(x => x.FinalTestId == request.FinalTestId && x.StudentId == studentId && x.CourseId == request.CourseId, cancellationToken);

            if (finalTestResult == null)
            {
                finalTestResult = new FinalTestResult
                {
                    FinalTestId = request.FinalTestId,
                    StudentId = studentId ?? default,
                    CourseId = request.CourseId,
                    Status = EnumResultStatus.Process
                };

                await _finalTestResultRepository.BulkMergeAsync(new List<FinalTestResult> { finalTestResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.FinalTestId, c.IsDeleted };
                });
            }
            else if (finalTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            var skillScores = new List<SkillScores>();
            foreach (var item in request.FinalTestAnswers)
            {
                if (item.Answers == null || item.Answers.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(item.Answers));
                    return methodResult;
                }
                var questionIds = item.Answers.Select(x => x.QuestionId).ToList();
                var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                if (questions == null || questions.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                    return methodResult;
                }
                int count = 0;
                foreach (var answer in item.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == answer.QuestionId);
                    var questionResult = _questionConverter.HandleQuestionAnswer(question, answer.Answer, true);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()!.Id;
                    var finalAnswer = await _finalTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.FinalTestResultId == finalTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                    if (finalAnswer == null)
                    {
                        finalAnswer = new FinalTestAnswer
                        {
                            CorrectCount = correctCount,
                            Answer = answerConfig ?? answer.Answer,
                            SectionQuestionId = sectionQuestionId,
                            IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null
                        };
                        count += correctCount;
                        finalTestResult.FinalTestAnswers.Add(finalAnswer);
                    }
                }
                var totalCount = questions.Sum(x => x.CorrectTotal);
                skillScores.Add(new SkillScores
                {
                    Skill = item.Skill,
                    TotalCount = totalCount,
                    CorrectCount = count,
                    CountQuestion = item.Answers.Count,
                    TotalQuestion = questions.Count,
                }
                );
            }

            #endregion Validation

            await _finalTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                finalTestResult.CorrectCount = Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
                finalTestResult.CorrectTotal = Convert.ToInt32(skillScores.Sum(x => x.TotalCount));
                finalTestResult.Status = EnumResultStatus.Done;
                finalTestResult.SkillScores = skillScores;
                finalTestResult.Percent = NumberHelper.GetPercent(finalTestResult.CorrectCount, finalTestResult.CorrectTotal);
                var courseId = finalTestResult.CourseId;

                // làm nhiệm vụ
                // await DoQuestBoard(courseId, cancellationToken);
                await _finalTestResultRepository.BulkUpdateList(new List<FinalTestResult> { finalTestResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.FinalTestId };
                });
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FinalTestResultModel>(finalTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
