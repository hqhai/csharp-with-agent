// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<VideoTimeCodeModel>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;

        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;

        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private const float Achieved_Point = 1; // Những nhiệm vụ làm 1 lần thì achieved point sẽ là 1
        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoResultRepository videoResultRepository
            , VideoConverter videoConverter
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IQuestionRepository questionRepository
            , AnswerTypeConverter answerTypeConverter,
              QuestBoardPublisher questBoardPublisher,
              IUserService userService,
              AuthContext authContext)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _videoConverter = videoConverter;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _questionRepository = questionRepository;
            _answerTypeConverter = answerTypeConverter;
            _questBoardPublisher = questBoardPublisher;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                return methodResult;
            }

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeResults).FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var (questions, videoTimeCode) = await GetQuestionsAndVideoTimeCodeAsyns(request.Answers.Select(x => x.QuestionId).ToList());
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            var videoTimeCodeId = videoTimeCode.Id;
            var videoTimeCodeResult = await GetVideoTimeCodeResultAsync(videoResult, videoTimeCodeId);
            videoResult.CurrentVideoTimeCodeId = videoTimeCodeId;

            #region Chặn Time Code Chưa Done

            //var videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id)).Where(x => x.Id == videoResult.CurrentVideoTimeCodeId).FirstOrDefaultAsync(cancellationToken);
            //if (videoTimeCode != null && videoTimeCode.Id != videoTimeCodeQuestion?.Id && videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Process))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodePreviousNotDone));
            //    return methodResult;
            //}

            #endregion Chặn Time Code Chưa Done

            #endregion Validation

            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var skillScores = new List<SkillScores>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null || question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                    return methodResult;
                }
                var exercise = question.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id ?? default;
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoTimeCodeResult.Id, question.Id, exerciseId);
                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                if (!string.IsNullOrEmpty(item.Answer?.ToString()) && answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                    return methodResult;
                }
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig ?? item.Answer,
                        VideoTimeCodeId = videoTimeCodeId,
                        ExerciseId = exerciseId,
                        QuestionId = question.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = question.Ungraded ? default : correctCount,
                        Status = GetAnswerStatus(videoTimeCode.TimeCodeType, correctCount, question.CorrectTotal)
                    };

                    videoTimeCodeAnswers.Add(answer);
                }
                else
                {
                    answer.Answer = answerConfig ?? item.Answer;
                    answer.Status = EnumAnswerStatus.Done;
                    answer.CorrectCount = question.Ungraded ? default : correctCount;
                    updateVideoTimeCodeAnswers.Add(answer);
                }

                skillScores.Add(new SkillScores
                {
                    Skill = exercise?.CourseSkill ?? default,
                    CorrectCount = correctCount,
                    TotalCount = question.CorrectTotal,
                    CountQuestion = 1,
                    TotalQuestion = 1,
                });
            }
            skillScores = GetSkillScores(skillScores);
            videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            videoTimeCodeResult.Percent = skillScores.Sum(x => x.CorrectCount).GetPercent(skillScores.Sum(x => x.TotalCount));
            videoTimeCodeResult.SkillScores = skillScores;
            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCode?.TimeCodeType == EnumTimeCodeType.UnitTest)
                {
                    var courseId = videoResult.LessonResult?.CourseId;
                    if (courseId != null)
                    {
                        await DoQuestBoard((Guid)courseId, cancellationToken);
                    }
                }

                if (videoTimeCodeAnswers.Any())
                {
                    if (videoTimeCode?.TimeCodeType != EnumTimeCodeType.Standalone || skillScores.Sum(x => x.TotalCount) == videoTimeCodeAnswers.Sum(x => x.CorrectCount))
                    {
                        videoTimeCodeResult.Status = EnumResultStatus.Done;
                    }

                    await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (updateVideoTimeCodeAnswers.Any())
                {
                    videoTimeCodeResult.Status = EnumResultStatus.Done;
                    _videoTimeCodeAnswerRepository.UpdateList(updateVideoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            videoTimeCode = await _videoTimeCodeRepository.Queryable.Include(x => x.TimeCodeExercises)
                .ThenInclude(x => x.Exercise)
                .ThenInclude(x => x!.ExerciseQuestions)
                .ThenInclude(x => x.Question)
                .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                .FirstOrDefaultAsync(x => x.Id == videoResult.CurrentVideoTimeCodeId, cancellationToken);
            var videoTimeCodeModel = _videoConverter.GetVideoTimeCode(videoTimeCode);
            methodResult.Result = videoTimeCodeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static List<SkillScores> GetSkillScores(IList<SkillScores> skillScores)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            return skillScores.GroupBy(x => x.Skill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CorrectCount = x.Sum(x => x.CorrectCount),
                TotalCount = x.Sum(x => x.TotalCount),
                CountQuestion = x.Sum(x => x.CountQuestion),
                TotalQuestion = x.Sum(x => x.TotalQuestion),
                Percent = x.Sum(x => x.CorrectCount).GetPercent(x.Sum(x => x.TotalCount))
            }).ToList();
        }

        private async Task<VideoTimeCodeResult> GetVideoTimeCodeResultAsync(VideoResult videoResult, Guid videoTimeCodeId)
        {
            var videoTimeCodeResult = videoResult.VideoTimeCodeResults.Where(x => x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResult.Id).FirstOrDefault();
            if (videoTimeCodeResult == null)
            {
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = videoResult.Id,
                    VideoTimeCodeId = videoTimeCodeId,
                    StudentId = videoResult.StudentId
                };
                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return videoTimeCodeResult;
        }

        private static EnumAnswerStatus GetAnswerStatus(EnumTimeCodeType? timeCodeType, int correctCount, int correctTotal)
        {
            if (timeCodeType == EnumTimeCodeType.Standalone)
            {
                if (correctCount == correctTotal)
                {
                    return EnumAnswerStatus.Done;
                }
                return EnumAnswerStatus.Process;
            }
            return EnumAnswerStatus.Done;
        }

        private async Task<(IList<Question>?, VideoTimeCode?)> GetQuestionsAndVideoTimeCodeAsyns(IList<Guid> questionIds)
        {
            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
            var exercise = questions?.SelectMany(x => x.ExerciseQuestions).Select(x => x.Exercise).FirstOrDefault();
            var videoTimeCode = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCode).FirstOrDefault();
            return (questions, videoTimeCode);
        }

        private async Task DoQuestBoard(Guid courseId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.FinishOneHomeworkMiniProject };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            //Chỉ bài finaltest đầu tiên hoàn thành của khóa mới được tính là hoàn thành nhiệm vụ
            bool checkFirstTimeDoneUnit = _videoTimeCodeResultRepository.Queryable.Any(v => v.Status == EnumResultStatus.Done);

            if (!checkFirstTimeDoneUnit)
            {
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = Achieved_Point,
                    CourseId = courseId
                }, cancellationToken);
            }
        }
    }
}
