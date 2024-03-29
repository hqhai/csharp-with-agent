// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1
{
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.SenderCmd;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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

        public CreatePlacementTestAnswerBySectionGroupCommandHandler(IQuestionRepository questionRepository
            , AuthContext authContext
            , QuestionConverter questionConverter
            , SectionGroupConverter sectionGroupConverter
            , IUserService userService
            , IMediator mediator
            , IPlacementTestResultRepository placementTestResultRepository
            , IPlacementTestAnswerRepository placementTestAnswerRepository
            , ISectionGroupResultRepository sectionGroupResultRepository
            , ISectionGroupRepository sectionGroupRepository
            , IPlacementTestRepository placementTestRepository
            , IMapper mapper,
ICourseRepository courseRepository,
AppSetting appSetting)
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
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(CreatePlacementTestAnswerBySectionGroupCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PlacementTestResultModel>();
            StudentModel? student;
            if (request.StudentId.HasValue)
            {
                var studentResults = await _userService.GetStudentsByStudentIdsAsync(new List<Guid> { request.StudentId.Value });
                if (!studentResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                    return methodResult;
                }
                student = studentResults.Content?.Result?.FirstOrDefault();
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
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == student.Id && x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == placementTestResult.Id).FirstOrDefaultAsync(cancellationToken);
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

            if (request.Answers != null && request.Answers.Any())
            {
                var answerResult = await CreateAnswerAsync(request, sectionGroupResult);
                if (!answerResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(answerResult.ErrorMessages);
                    return methodResult;
                }
            }
            await _placementTestAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                sectionGroupResult = await _sectionGroupConverter.UpdateSectionGroupToIsSubmit(sectionGroup, sectionGroupResult, request.IsSubmit);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            var isLockPT = await UpdatePlacementTestResultAsync(placementTestResult, placementTest, student, cancellationToken);
            methodResult.Result = await GetPlacementTestResult(placementTestResult, isLockPT);
            return methodResult;
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
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(s => s.PlacementTestResultId == placementTestResult.Id).ToListAsync(cancellationToken);
                if (sectionGroupResults != null && sectionGroupResults.Count == numberOfDone && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.Human?.Birthday);
                    placementTestResult = GetPlacementTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), placementTestResult);

                    var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == placementTestResult.StudentId)
                                                                          .OrderBy(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);

                    var (currentLevel, isLockPT) = placementTest.Level.GetLevelInScore(placementTestResult.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                    if (currentLevel.HasValue)
                    {
                        await _userService.UpdateStudentByLevelAsync(new UpdateStudentByLevelModel
                        {
                            Id = student.Human?.UserId ?? _authContext.CurrentUserId,
                            Level = currentLevel.Value
                        }).ConfigureAwait(false);
                    }
                    _placementTestResultRepository.Update(placementTestResult);
                    await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                    if (isLockPT)
                    {
                        await SendStudentPlacementTest(currentLevel ?? default, student, placementTestResult, cancellationToken);
                    }
                    return isLockPT;
                }
            }
            return default;
        }

        private async Task SendStudentPlacementTest(EnumCourseLevel courseLevel, StudentModel student, PlacementTestResult placementTestResult, CancellationToken cancellationToken)
        {
            var skillScoresHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.Skill, cancellationToken);

            var courseInfoHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, EnumCourseLevelHelper.GetCourseInfo(courseLevel), cancellationToken);

            var teachersHtml = await SendMailHelper.GetTemplateFromPath(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeachersInFo, cancellationToken);

            var pathTeachersBios = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SendMailSetting.TeacherBios);
            var listTeachersBios = ConvertHelper.DeserializeFromFilePath<IList<CourseTeacherModel>>(pathTeachersBios);

            var teachers = listTeachersBios?.Where(p => p.TeacherLevels != null && p.TeacherLevels.Any(x => x == courseLevel)).ToList();
            var teacherInfo = string.Empty;

            for (int i = 0; i < teachers?.Count; i++)
            {
                var teacher = string.Empty;
                if (i > 0)
                {
                    teacher = string.Format(CultureInfo.InvariantCulture, teachersHtml, null, teachers[i].AvatarPath, teachers[i].FullName, teachers[i].Nationality, teachers[i].Deggree, teachers[i].Experience, teachers[i].Strength);
                }
                else
                {
                    teacher = string.Format(CultureInfo.InvariantCulture, teachersHtml, SendMailSetting.Display, teachers[i].AvatarPath, teachers[i].FullName, teachers[i].Nationality, teachers[i].Deggree, teachers[i].Experience, teachers[i].Strength);
                }
                teacherInfo += teacher;
            }

            var skillsScore = string.Empty;
            if (placementTestResult.SkillScores != null)
            {
                foreach (var item in placementTestResult.SkillScores)
                {
                    var (color, skillName, icon) = SendMailHelper.ConvertEnum(item.Skill);
                    var html = string.Format(CultureInfo.InvariantCulture, skillScoresHtml, icon, skillName, item.Percent, item.Percent < 100 ? "100px 0px 0px 100px" : "100px 100px 100px 100px", color, 100 - item.Percent, item.Percent);
                    skillsScore += html;
                }
            }

            var param = new SendStudentPTTemplateModel
            {
                FullName = student.Human?.FullName,
                CourseLevel = courseLevel.GetDescription(),
                SkillScores = skillsScore,
                CourseInfo = courseInfoHtml,
                CourseTitle = EnumCourseLevelHelper.GetCourseTitle(courseLevel),
                ContinueLearn = _appSetting.ResourceContent?.LmsWebsiteUrl,
                TeachersInfo = teacherInfo
            };

            param.CourseTitleDisplay = string.IsNullOrEmpty(param.CourseTitle) ? SendMailSetting.Display : null;

            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendPTResultSubject);
            var sendResult = new MethodResult<bool>();
            if (!string.IsNullOrEmpty(student.Human?.Email))
            {
                sendResult = await _mediator.Send(new SenderCommand { Email = student.Human?.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.StudentCompletePT }, cancellationToken).ConfigureAwait(false);
            }
        }

        private static PlacementTestResult GetPlacementTestResult(IList<SkillScores>? skillScores, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            placementTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            placementTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores;
            return placementTestResult;
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
            var anwserResult = await CreateAnswer(request, questions, sectionGroupResult);
            if (!anwserResult.IsOK)
            {
                methodResult.AddErrorBadRequest(anwserResult.ErrorMessages);
                return methodResult;
            }
            var (createPlacementTestAnswers, updatePlacementTestAnswers) = anwserResult.Result;
            if (createPlacementTestAnswers != null && createPlacementTestAnswers.Any())
            {
                await _placementTestAnswerRepository.AddList(createPlacementTestAnswers);
                await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            if (updatePlacementTestAnswers != null && updatePlacementTestAnswers.Any())
            {
                _placementTestAnswerRepository.UpdateList(updatePlacementTestAnswers);
                await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
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
                foreach (var item in request.Answers)
                {
                    var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                    var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit);
                    if (!questionResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                        return methodResult;
                    }
                    var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                    var sectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default;
                    var placementTestAnswer = await _placementTestAnswerRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestResultId == request.PlacementTestResultId && x.SectionQuestionId == sectionQuestionId);
                    if (placementTestAnswer == null)
                    {
                        placementTestAnswer = new PlacementTestAnswer
                        {
                            PlacementTestResultId = request.PlacementTestResultId,
                            SectionGroupResultId = sectionGroupResult.Id,
                            SectionQuestionId = questionItem.SectionQuestions.FirstOrDefault()?.Id ?? default,
                        };
                        createPlacementTestAnswers.Add(GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem));
                    }
                    else
                    {
                        updatePlacementTestAnswers.Add(GetPlacementTestAnswer(placementTestAnswer, answerConfig, correctCount, isAnswered, questionItem));
                    }
                }
            }

            methodResult.Result = (createPlacementTestAnswers, updatePlacementTestAnswers);
            return methodResult;
        }

        private static PlacementTestAnswer GetPlacementTestAnswer(PlacementTestAnswer placementTestAnswer, object? answer, int correctCount, bool isAnswered, Question questionItem)
        {
            placementTestAnswer.Answer = answer;
            placementTestAnswer.CorrectCount = correctCount;
            placementTestAnswer.IsCorrect = isAnswered ? correctCount == questionItem.CorrectTotal : null;
            placementTestAnswer.Status = questionItem.CorrectTotal == correctCount ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
            return placementTestAnswer;
        }
    }
}
