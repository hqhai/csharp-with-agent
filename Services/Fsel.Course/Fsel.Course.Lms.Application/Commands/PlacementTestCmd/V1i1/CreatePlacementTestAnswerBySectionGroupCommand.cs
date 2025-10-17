// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Commands.StudentCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreatePlacementTestAnswerBySectionGroupCommand : CreatePlacementTestAnswerBySectionGroupCommandModel, IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class CreatePlacementTestAnswerBySectionGroupCommandHandler : IRequestHandler<CreatePlacementTestAnswerBySectionGroupCommand, MethodResult<PlacementTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionConverter _questionConverter;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly IMapper _mapper;
        private readonly ICourseRepository _courseRepository;
        private readonly AppSetting _appSetting;
        private readonly ILogger<CreatePlacementTestAnswerBySectionGroupCommand> _logger;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly DisconnectSocketCalculateTimePublisher _disconnectSocketCalculateTimePublisher;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly SavePlacementTestAnswersPublisher _savePlacementTestAnswersPublisher;
        private readonly ISystemService _systemService;

        public CreatePlacementTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository,
            AuthContext authContext,
            QuestionConverter questionConverter,
            SectionGroupConverter sectionGroupConverter,
            IUserService userService,
            IMediator mediator,
            IPlacementTestResultRepository placementTestResultRepository,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            IPlacementTestRepository placementTestRepository,
            IMapper mapper,
            ICourseRepository courseRepository,
            AppSetting appSetting,
            ILogger<CreatePlacementTestAnswerBySectionGroupCommand> logger,
            QuestBoardPublisher questBoardPublisher,
            DisconnectSocketCalculateTimePublisher disconnectSocketCalculateTimePublisher,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            SavePlacementTestAnswersPublisher savePlacementTestAnswersPublisher, ISystemService systemService
            )
        {
            _questionRepository = questionRepository;
            _authContext = authContext;
            _questionConverter = questionConverter;
            _sectionGroupConverter = sectionGroupConverter;
            _userService = userService;
            _mediator = mediator;
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _placementTestRepository = placementTestRepository;
            _mapper = mapper;
            _courseRepository = courseRepository;
            _appSetting = appSetting;
            _logger = logger;
            _questBoardPublisher = questBoardPublisher;
            _disconnectSocketCalculateTimePublisher = disconnectSocketCalculateTimePublisher;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _savePlacementTestAnswersPublisher = savePlacementTestAnswersPublisher;
            _systemService = systemService;
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(CreatePlacementTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PlacementTestResultModel>();

            _logger.LoggerRequest(request);

            StudentModel? student;
            if (request.StudentId.HasValue)
            {
                var studentResult = await _userService.GetUserByStudentId(request.StudentId.Value);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            else
            {
                var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            #region Validate

            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.PlacementTestResultId);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                return methodResult;
            }
            else if (placementTestResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestDone), nameof(placementTestResult.Status));
                return methodResult;
            }
            var placementTest = await _placementTestRepository.GetByIdAsync(placementTestResult.PlacementTestId ?? default);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTest));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == placementTestResult.Id && x.CreatedDate >= placementTestResult.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (sectionGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroupResult));
                return methodResult;
            }
            else if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.SectionGroupResultDone), nameof(sectionGroupResult.Status));
                return methodResult;
            }

            #endregion Validate

            if (request.IsSubmit)
            {
                await _disconnectSocketCalculateTimePublisher.Publish(new SetTimeModuleModel
                {
                    Type = nameof(PlacementTest),
                    ObjectId = sectionGroupResult.Id
                }, cancellationToken);
            }
            if (request.Answers != null && request.Answers.Any())
            {
                var answerResult = await CreateAnswerAsync(request, sectionGroupResult);
                if (!answerResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                    return methodResult;
                }
            }
            await _sectionGroupConverter.UpdateSectionGroupToIsSubmit(sectionGroup, sectionGroupResult, request.IsSubmit);

            var isLockPT = await UpdatePlacementTestResultAsync(placementTestResult, placementTest, student, cancellationToken);
            methodResult.Result = await GetPlacementTestResult(placementTestResult, isLockPT);
            return methodResult;
        }

        private async Task<MethodResult<IList<PlacementTestAnswer>>> CreateAnswerAsync(CreatePlacementTestAnswerBySectionGroupCommand request, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<IList<PlacementTestAnswer>>();
            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeSectionByIdAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            var sectionGroupIds = questions.SelectMany(x => x.SectionQuestions).Select(x => x.Section?.SectionGroupId).ToList();
            if (!sectionGroupIds.Any(x => x == request.SectionGroupId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.SectionGroupId));
                return methodResult;
            }

            var anwserResult = await CreateAnswer(request, questions, sectionGroupResult);
            if (!anwserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anwserResult.ErrorMessages);
                return methodResult;
            }
            var (createPlacementTestAnswers, updatePlacementTestAnswers) = anwserResult.Result;

            try
            {
                if (createPlacementTestAnswers != null && createPlacementTestAnswers.Any())
                {
                    await _placementTestAnswerRepository.BulkMergeAsync(createPlacementTestAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.SectionGroupResultId, entity.SectionQuestionId, entity.PlacementTestResultId, entity.IsDeleted };
                    });
                }
                if (updatePlacementTestAnswers != null && updatePlacementTestAnswers.Any())
                {
                    await _placementTestAnswerRepository.BulkUpdateList(updatePlacementTestAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.PlacementTestResultId, entity.SectionQuestionId, entity.SectionGroupResultId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate PlacementTestAnswer : {ex.Message}");
            }
            return methodResult;
        }

        private async Task<MethodResult<(IList<PlacementTestAnswer>, IList<PlacementTestAnswer>)>> CreateAnswer(CreatePlacementTestAnswerBySectionGroupCommand request, IList<Question>? questions, SectionGroupResult sectionGroupResult)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<(IList<PlacementTestAnswer>, IList<PlacementTestAnswer>)>();
            var createPlacementTestAnswers = new List<PlacementTestAnswer>();
            var updatePlacementTestAnswers = new List<PlacementTestAnswer>();
            if (questions != null && questions.Any())
            {
                var placementTestAnswers = await _placementTestAnswerRepository.Queryable.Where(x => x.SectionGroupResultId == sectionGroupResult.Id)
                                                                               .Where(x => x.CreatedDate >= sectionGroupResult.CreatedDate)
                                                                               .ToListAsync();
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleAnswerTest(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var placementTestAnswer = placementTestAnswers.FirstOrDefault(x => x.SectionQuestionId == sectionQuestionId);
                    if (placementTestAnswer == null)
                    {
                        placementTestAnswer = new PlacementTestAnswer
                        {
                            PlacementTestResultId = request.PlacementTestResultId,
                            SectionGroupResultId = sectionGroupResult.Id,
                            SectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default,
                        };
                        placementTestAnswer = GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem);
                        if (!placementTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(placementTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        createPlacementTestAnswers.Add(placementTestAnswer);
                    }
                    else
                    {
                        placementTestAnswer = GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem);
                        if (!placementTestAnswer.IsValid())
                        {
                            methodResult.AddErrorBadRequest(placementTestAnswer.ErrorMessages);
                            return methodResult;
                        }
                        updatePlacementTestAnswers.Add(placementTestAnswer);
                    }
                }
            }

            methodResult.Result = (createPlacementTestAnswers, updatePlacementTestAnswers);
            return methodResult;
        }

        private static PlacementTestAnswer GetPlacementTestAnswer(PlacementTestAnswer placementTestAnswer, object? answer, short correctCount, bool isAnswered, Question questionItem)
        {
            placementTestAnswer.Answer = answer;
            placementTestAnswer.CorrectCount = correctCount;
            placementTestAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
            placementTestAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
            return placementTestAnswer;
        }

        private async Task UpdatePlacementGroupResultDoneAsync(PlacementTestResult placementTestResult, EnumCourseLevel? level)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == placementTestResult.StudentId);
            if (placementTestGroupResult == null)
            {
                return;
            }
            placementTestGroupResult.CompletionDate = DateTime.UtcNow;
            placementTestGroupResult.CompletionLevel = placementTestResult.Level;
            placementTestGroupResult.SuggetLevel = level;
            placementTestGroupResult.CurrentLevel = SendMailHelper.GetPreviousEnumValue(level ?? default);
            placementTestGroupResult.Status = EnumResultStatus.Done;
            placementTestGroupResult.Percent = placementTestResult.Percent;

            await _placementTestGroupResultRepository.BulkUpdateList(new List<PlacementTestGroupResult> { placementTestGroupResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId };
            });
        }

        private async Task<PlacementTestResultModel> GetPlacementTestResult(PlacementTestResult placementTestResult, bool isLockPT)
        {
            var moduleNumber = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == placementTestResult.StudentId).CountAsync();
            var placementTestResultDto = _mapper.Map<PlacementTestResultModel>(placementTestResult);
            placementTestResultDto.IsLock = isLockPT;
            placementTestResultDto.ModuleNumber = ++moduleNumber;
            return placementTestResultDto;
        }

        private async Task<bool> UpdatePlacementTestResultAsync(PlacementTestResult placementTestResult, PlacementTest placementTest, StudentModel? student, CancellationToken cancellationToken)
        {
            if (student != null)
            {
                var numberOfDone = 4;
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.PlacementTestResultId == placementTestResult.Id && s.CreatedDate >= placementTestResult.CreatedDate).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
                if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
                    placementTestResult = GetPlacementTestResult(sectionGroupResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).ToList(), placementTestResult);
                    var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == placementTestResult.StudentId)
                                                                          .OrderBy(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);

                    var (currentLevel, isLockPT) = placementTest.Level.GetLevelInScore(placementTestResult.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                    if (currentLevel.HasValue)
                    {
                        await _userService.UpdateStudentByLevelAsync(new UpdateStudentByLevelModel
                        {
                            Id = student.Human?.UserId ?? _authContext.CurrentUserId,
                            CourseLevel = currentLevel.Value,
                            BaseCourseLevel = currentLevel.Value
                        }).ConfigureAwait(false);
                    }

                    await _placementTestResultRepository.BulkUpdateList(new List<PlacementTestResult> { placementTestResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.PlacementTestId };
                    });

                    if (isLockPT)
                    {
                        await UpdatePlacementGroupResultDoneAsync(placementTestResult, currentLevel);
                        await DoQuestBoard(student.Id, cancellationToken).ConfigureAwait(false);
                        await SendStudentPlacementTest(currentLevel ?? default, student, age, cancellationToken).ConfigureAwait(false);
                        await DoUserReferral(student.Human?.UserId ?? _authContext.CurrentUserId, cancellationToken).ConfigureAwait(false);
                    }
                    return isLockPT;
                }
            }
            return default;
        }

        private async Task DoUserReferral(Guid receiverId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new AddFeatureMissionCommand()
            {
                ReceiverId = receiverId,
                FeatureUserReferral = EnumFeatureUserReferral.PT
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.BeginnerQuests,
                Category = EnumQuestBoardCategory.CompleteTheFirstTest,
                Value = 1
            }, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendStudentPlacementTest(EnumCourseLevel courseLevel, StudentModel student, int age, CancellationToken cancellationToken)
        {
            var currentLevel = SendMailHelper.GetPreviousEnumValue(courseLevel);

            var courseSuggestResults = await _systemService.CourseSuggestConfigQuery(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>()
                {
                    new GenericFilterModel()
                    {
                        Property = "FromAge",
                        Operator = EnumFilterOperator.LessThanOrEqual,
                        Value = age
                    },
                     new GenericFilterModel()
                    {
                        Property = "ToAge",
                        Operator = EnumFilterOperator.GreaterThanOrEqual,
                        Value = age
                    },
                     new GenericFilterModel()
                    {
                        Property = "PlacementTestLevel",
                        Operator = EnumFilterOperator.Equal,
                        Value = courseLevel
                    }
                     ,
                     new GenericFilterModel()
                    {
                        Property = "Type",
                        Operator = EnumFilterOperator.Equal,
                        Value = "Balanced"
                    }
                }
            });

            var courseSuggests = courseSuggestResults.Content?.Result;
            var suggestLevels = courseSuggests?.FirstOrDefault()?.CourseLevels;

            string currentCourseHtml = string.Empty;

            if (courseLevel == EnumCourseLevel.A1)
            {
                currentCourseHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(null), cancellationToken);
            }
            else
            {
                currentCourseHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(currentLevel), cancellationToken);
            }

            var courseInfoHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.CourseInfo, cancellationToken);

            var teachersHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeachersInFo, cancellationToken);
            var pathTeachersBios = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeacherBios);
            var listTeachersBios = ConvertHelper.DeserializeFromFilePath<IList<CourseTeacherModel>>(pathTeachersBios);

            string coursesInfo = string.Empty;

            for (int i = 0; i < suggestLevels?.Count; i++)
            {
                var teachers = listTeachersBios?.Where(p => p.TeacherLevels != null && p.TeacherLevels.Any(x => x == suggestLevels[i])).ToList();
                var teacherInfo = string.Empty;

                for (int j = 0; j < teachers?.Count; j++)
                {
                    if (j == 0)
                    {
                        var teacherInfoHtml = string.Format(CultureInfo.InvariantCulture, teachersHtml, null, suggestLevels[i], teachers[j].AvatarPath, teachers[j].FullName, teachers[j].Nationality, teachers[j].Certifications, teachers[j].Experience);
                        teacherInfo += teacherInfoHtml;
                    }
                    else
                    {
                        var teacherInfoHtml = string.Format(CultureInfo.InvariantCulture, teachersHtml, SendMailSetting.Display, suggestLevels[i], teachers[j].AvatarPath, teachers[j].FullName, teachers[j].Nationality, teachers[j].Certifications, teachers[j].Experience);
                        teacherInfo += teacherInfoHtml;
                    }
                }
                var courseType = EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i]);

                var courseInfo = string.Format(CultureInfo.InvariantCulture, courseInfoHtml, i + 1, suggestLevels[i], suggestLevels[i].GetDescription(), EnumCourseLevelHelper.GetCourseTitle(suggestLevels[i]), EnumCourseLevelHelper.GetLevelPhoto(suggestLevels[i]), SendMailHelper.GetCourseTitle(courseType), SendMailHelper.GetInfoCourse(EnumCourseLevelHelper.GetEnumCourseType(suggestLevels[i])), teacherInfo);

                coursesInfo += courseInfo;
            }

            var param = new SendStudentPTTemplateModel
            {
                FullName = student.Human?.FullName,
                CurrentCourse = currentCourseHtml,
                CourseInfos = coursesInfo,
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl
            };

            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(student.Human?.Email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = student.Human.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.StudentCompletePT, IsCCEmail = true }, cancellationToken).ConfigureAwait(false);
            }
        }

        private static PlacementTestResult GetPlacementTestResult(IList<SkillScores>? skillScores, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            placementTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            placementTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores.OrderBy(x => x.Skill).ToList();
            return placementTestResult;
        }
    }
}
