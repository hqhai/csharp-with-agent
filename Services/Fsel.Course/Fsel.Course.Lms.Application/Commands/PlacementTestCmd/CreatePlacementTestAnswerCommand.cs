// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System;
    using System.Globalization;
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
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly CreateOrderPublisher _createOrderPublisher;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly AuthContext _authContext;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IMediator _mediator;

        public CreatePlacementTestAnswerCommandHandler(IPlacementTestAnswerRepository placementTestAnswerRepository
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , CreateOrderPublisher createOrderPublisher
            , IQuestionRepository questionRepository
            , IMapper mapper
            , ICourseRepository courseRepository
            , AuthContext authContext
            , AnswerTypeConverter answerTypeConverter
            , IMediator mediator)
        {
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _createOrderPublisher = createOrderPublisher;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _authContext = authContext;
            _answerTypeConverter = answerTypeConverter;
            _mediator = mediator;
        }

        public async Task<MethodResult<IList<PlacementTestResultModel>>> Handle(CreatePlacementTestAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<PlacementTestResultModel>> methodResult = new MethodResult<IList<PlacementTestResultModel>>();

            #region Validation

            if (request.Skills == null || request.Skills.Count == 0 || request.Skills.Any(x => x.Answers == null || x.Answers.Count == 0))
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
            var studentId = student?.Id;
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                           .OrderByDescending(x => x.CreatedDate)
                                                                           .FirstOrDefaultAsync(cancellationToken);
            int age = DateTimeHelper.GetYearOld(student?.Human?.Birthday);
            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, age);
                if (isLock)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestLock), nameof(levelNext));
                    return methodResult;
                }
            }

            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (placementTestResults.Count >= 3)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestResultMaxThree), nameof(placementTestResults));
                return methodResult;
            }
            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Level == request.Level && x.Status == EnumResultStatus.Process, cancellationToken);
            if (placementTestResult == null)
            {
                placementTestResult = new PlacementTestResult
                {
                    Status = EnumResultStatus.Process,
                    Level = request.Level,
                    StudentId = studentId ?? default
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
                        if (question == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                            return methodResult;
                        }
                        else if (question.Config == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                            return methodResult;
                        }
                        else if (question.SectionQuestions == null || question.SectionQuestions.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question.SectionQuestions));
                            return methodResult;
                        }
                        var sectionQuestionId = question.SectionQuestions.FirstOrDefault()!.Id;
                        var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == placementTestResult.Id && x.SectionQuestionId == sectionQuestionId, cancellationToken);

                        if (placementTestAnswer == null)
                        {
                            var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(answer.Answer, question.Config, question.QuestionType);
                            if (answerConfig == null && !string.IsNullOrEmpty(answer.Answer?.ToString()))
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answer.Answer), answer.Answer);
                                return methodResult;
                            }
                            count += correctCount;
                            countQuestion++;
                            placementTestAnswer = new PlacementTestAnswer
                            {
                                CorrectCount = correctCount,
                                Answer = answerConfig,
                                SectionQuestionId = sectionQuestionId
                            };
                            placementTestAnswers.Add(placementTestAnswer);
                        }
                    }
                    var percent = questions.Sum(x => x.CorrectTotal) > 0 ? (double)count / questions.Sum(x => x.CorrectTotal) * 100 : default;
                    var skillScore = new SkillScores
                    {
                        Skill = item.Skill,
                        CountQuestion = countQuestion,
                        Scores = percent.GetIeltsScore(item.Skill),
                        TotalQuestion = questions.Count,
                        TotalCount = questions.Sum(x => x.CorrectTotal),
                        CorrectCount = count,
                        Percent = percent
                    };
                    if (placementTestResult.Level == EnumPlacementTestLevel.IELTS)
                    {
                        skillScore.Scores = skillScore.CorrectCount.GetIeltsScore(skillScore.Skill);
                    }
                    skillScores.Add(skillScore);
                }
            }

            #endregion Validation

            placementTestResult.CorrectCount = Convert.ToInt32(skillScores.Sum(x => x.CorrectCount));
            placementTestResult.CorrectTotal = Convert.ToInt32(skillScores.Sum(x => x.TotalCount));
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores;
            placementTestResult.Percent = placementTestResult.CorrectTotal > 0 ? ((double)placementTestResult.CorrectCount / placementTestResult.CorrectTotal * 100) : default;
            placementTestResult.PlacementTestAnswers = placementTestAnswers;

            var overallScore = NumberHelper.RoundNumberDouble(skillScores.Select(x => x.Scores).Average());
            var (currentLevel, isLockPT) = request.Level.GetLevelInScore(placementTestResult.Level == EnumPlacementTestLevel.IELTS ? overallScore : placementTestResult.Percent, age);

            if (currentLevel.HasValue)
            {
                var updateStudent = new NotificationRemindModel
                {
                    Id = _authContext.CurrentUserId,
                    Level = currentLevel.Value
                };
                var isCheckResult = await _userService.UpdateStudentByLevelAsync(updateStudent);
                if (!isCheckResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                    return methodResult;
                }
            }
            if (isLockPT)
            {
                #region Pilot

                if (currentLevel.HasValue)
                {
                    Random random = new Random();
                    var courses = await _courseRepository.Queryable.Where(x => x.CourseLevel == currentLevel.Value && x.Status != EnumCourseStatus.New).ToListAsync(cancellationToken);
                    var course = courses.OrderBy(x => random.Next(courses.Count)).FirstOrDefault();
                    if (course == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                        return methodResult;
                    }
                    CreateOrderQueueModel createOrderQueueModel = new CreateOrderQueueModel
                    {
                        Address = "Viet Nam",
                        Country = "Viet Nam",
                        CourseId = course.Id,
                        CourseLevel = currentLevel.Value,
                        FullName = student?.Human?.FullName,
                        PaymentMethod = EnumPaymentMethodStatus.Card,
                        CodeCourse = course.Code,
                        UserId = _authContext.CurrentUserId
                    };
                    await _createOrderPublisher.Publish(createOrderQueueModel, cancellationToken);
                }

                #endregion Pilot
            }
            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                placementTestResult = _placementTestResultRepository.Add(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                placementTestResults.Add(placementTestResult);
                placementTestResults = placementTestResults.OrderBy(x => x.CreatedDate).ToList();

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<PlacementTestResultModel>>(placementTestResults);
                return methodResult;
            });
            if (isLockPT)
            {
                var param = new SendStudentPTTemplateModel
                {
                    StudentName = student?.Human?.FullName,
                    CourseLevel = placementTestResult.Level,
                    Percents = string.Join(Environment.NewLine, placementTestResults.Select((x, index) => $"- Module {index + 1}: {x.Percent} %")),
                };
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
                var sendResult = new MethodResult<bool>();
                if (!string.IsNullOrEmpty(student!.Human?.Email))
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = student!.Human?.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendStudentPTOnline }, cancellationToken).ConfigureAwait(false);
                }
            }
            return methodResult;
        }
    }
}
