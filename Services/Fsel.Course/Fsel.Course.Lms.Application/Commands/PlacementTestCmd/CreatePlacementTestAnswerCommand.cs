// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreatePlacementTestAnswerCommand : CreatePlacementTestAnswerCommandModel, IRequest<MethodResult<IList<PlacementTestResultModel>>>
    {
    }

    public class CreatePlacementTestAnswerCommandHandler : IRequestHandler<CreatePlacementTestAnswerCommand, MethodResult<IList<PlacementTestResultModel>>>
    {
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly QuestionConverter _questionConverter;
        private readonly IMediator _mediator;

        public CreatePlacementTestAnswerCommandHandler(IPlacementTestAnswerRepository placementTestAnswerRepository
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IQuestionRepository questionRepository
            , IMapper mapper
            , AuthContext authContext
            , QuestionConverter questionConverter
            , IMediator mediator)
        {
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _authContext = authContext;
            _questionConverter = questionConverter;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<PlacementTestResultModel>>> Handle(CreatePlacementTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<PlacementTestResultModel>> methodResult = new MethodResult<IList<PlacementTestResultModel>>();

            #region Validation

            if (request.Skills == null || request.Skills.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Skills));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            if (!student.CourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            if (request.Level != EnumPlacementTestLevel.IELTS)
            {
                request.Level = student.CourseLevel.Value.GetPlacementTestLevelByCourseLevel();
            }
            var studentId = student.Id;
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                           .OrderByDescending(x => x.CreatedDate)
                                                                           .FirstOrDefaultAsync(cancellationToken);

            var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId)
                                                                      .OrderBy(x => x.CreatedDate)
                                                                      .FirstOrDefaultAsync(cancellationToken);

            int age = DateTimeHelper.GetYearOld(student?.User?.Birthday);
            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                if (isLock)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestLock), nameof(isLock));
                    return methodResult;
                }
                if (levelNext != student.BaseCourseLevel)
                {
                    methodResult.AddErrorBadRequest(nameof(levelNext));
                    return methodResult;
                }
            }

            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (placementTestResults.Count >= 3)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestResultMaxThree), nameof(placementTestResults));
                return methodResult;
            }
            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Level == request.Level && x.Status == EnumResultStatus.Process && x.StudentId == studentId, cancellationToken);
            if (placementTestResult == null)
            {
                placementTestResult = new PlacementTestResult
                {
                    Status = EnumResultStatus.Process,
                    Level = request.Level,
                    StudentId = studentId
                };
            }
            else if (placementTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestResultErrorCode.PlacementTestResultDone));
                return methodResult;
            }

            var placementTestAnswers = new List<PlacementTestAnswer>();
            var skillScores = new List<SkillScores>();
            foreach (var item in request.Skills)
            {
                if (item.Answers != null && item.Answers.Count > 0)
                {
                    var questionIds = item.Answers.Select(x => x.QuestionId).ToList();
                    var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
                    if (questions == null || questions.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                        return methodResult;
                    }
                    int count = 0;
                    int countQuestion = 0;
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
                        var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == placementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                        if (placementTestAnswer == null)
                        {
                            count += correctCount;
                            countQuestion++;
                            placementTestAnswer = new PlacementTestAnswer
                            {
                                CorrectCount = correctCount,
                                Answer = answerConfig,
                                SectionQuestionId = sectionQuestionId,
                                IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null
                            };
                            placementTestAnswers.Add(placementTestAnswer);
                        }
                    }
                    var skillScore = new SkillScores
                    {
                        Skill = item.Skill,
                        CountQuestion = countQuestion,
                        TotalQuestion = questions.Count,
                        TotalCount = questions.Sum(x => x.CorrectTotal),
                        CorrectCount = count,
                    };
                    if (placementTestResult.Level == EnumPlacementTestLevel.IELTS)
                    {
                        skillScore.Scores = count.GetIeltsScorePT(skillScore.Skill);
                    }
                    skillScores.Add(skillScore);
                }
            }

            #endregion Validation

            placementTestResult.CountQuestion = request.CountQuestion;
            placementTestResult.TotalQuestion = request.TotalQuestion;
            placementTestResult.CorrectCount = Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
            placementTestResult.CorrectTotal = Convert.ToInt32(skillScores.Sum(x => x.TotalCount));
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores;
            placementTestResult.PlacementTestAnswers = placementTestAnswers;

            var overallScore = NumberHelper.RoundNumberDouble(skillScores.Select(x => x.Scores).Average());
            var (currentLevel, isLockPT) = request.Level.GetLevelInScore(placementTestResult.Level == EnumPlacementTestLevel.IELTS ? overallScore : placementTestResult.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));

            if (currentLevel.HasValue)
            {
                var updateStudent = new UpdateStudentByLevelModel
                {
                    Id = _authContext.CurrentUserId,
                    CourseLevel = currentLevel.Value,
                    BaseCourseLevel = currentLevel.Value
                };
                var isCheckResult = await _userService.UpdateStudentByLevelAsync(updateStudent);
                if (!isCheckResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                    return methodResult;
                }
            }
            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                await _placementTestResultRepository.BulkMergeAsync(new List<PlacementTestResult> { placementTestResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.PlacementTestId, c.StudentId, c.IsDeleted };
                });
                placementTestResults.Add(placementTestResult);
                placementTestResults = placementTestResults.OrderBy(x => x.CreatedDate).ToList();

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<PlacementTestResultModel>>(placementTestResults);
                return methodResult;
            });
            if (isLockPT)
            {
                #region
                //var param = new SendStudentPTTemplateModel
                //{
                //    StudentName = student?.User?.FullName,
                //    CourseLevel = placementTestResult.Level,
                //    Percents = string.Join(Environment.NewLine, placementTestResults.Select((x, index) => $"- Module {index + 1}: {Math.Round(x.Percent, MidpointRounding.AwayFromZero)} %")),
                //};
                //var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
                //var sendResult = new MethodResult<bool>();
                //if (!string.IsNullOrEmpty(student?.User?.Email))
                //{
                //    sendResult = await _mediator.Send(new SenderCommand { Email = student?.User?.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendStudentPTOnline }, cancellationToken).ConfigureAwait(false);
                //}
                #endregion
            }
            return methodResult;
        }
    }
}
