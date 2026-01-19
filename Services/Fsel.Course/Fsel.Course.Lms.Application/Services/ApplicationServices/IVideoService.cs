// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public interface IVideoService
    {
        Task<VideoModel?> GetVideoModelAsync(VideoResult videoResult, CancellationToken cancellationToken = default);

        Task<(IList<SkillScores>?, IList<SkillScores>, bool)> GetSkillScoresAsync(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, bool isTimeUp, CancellationToken cancellationToken = default);

        Task<VoidMethodResult> CreateAnswers(CreateVideoTimeCodeAnswerV1i1CommandModel request, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken = default);

        Task<long> UpdateVideoAnswers(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, bool isDoneTimeCode = false, bool isSubmit = true, CancellationToken cancellationToken = default);
    }

    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ITimeCodeQuestionCachingService _timeCodeQuestionCachingService;
        private readonly IMapper _mapper;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly QuestionConverter _questionConverter;

        public VideoService(
            IVideoRepository videoRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ITimeCodeQuestionCachingService timeCodeQuestionCachingService,
            IMapper mapper,
            DateTimeConverter dateTimeConverter,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            QuestionConverter questionConverter)
        {
            _videoRepository = videoRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _timeCodeQuestionCachingService = timeCodeQuestionCachingService;
            _mapper = mapper;
            _dateTimeConverter = dateTimeConverter;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _questionConverter = questionConverter;
        }

        #region Build VideoModel

        public async Task<VideoModel?> GetVideoModelAsync(VideoResult videoResult, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(videoResult);

            var video = await GetVideoAsync(videoResult.VideoId);
            if (video == null)
            {
                return null;
            }

            var videoModel = _mapper.Map<VideoModel>(video);
            videoModel.VideoTimeCodes = await GetVideoTimeCodesAsync(video, videoResult);
            return videoModel;
        }

        public async Task<IList<VideoTimeCodeModel>> GetVideoTimeCodesAsync(Video? video, VideoResult videoResult)
        {
            ArgumentNullException.ThrowIfNull(video);
            ArgumentNullException.ThrowIfNull(videoResult);
            // 1. Chuẩn bị danh sách timecode + index đang xử lý
            var videoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).ToList();
            var indexProcess = GetIndexProcess(videoTimeCodes, videoResult.CurrentVideoTimeCodeId);

            // 2. Load data từ DB 1 lần, convert sang lookup/dict
            var timeCodeResultDict = await LoadVideoTimeCodeResultDictAsync(videoResult);
            var timeCodeQuestionLookup = await LoadTimeCodeQuestionLookupAsync(video.Id);

            // 3. Build từng VideoTimeCodeModel
            var models = BuildVideoTimeCodeModels(
                videoTimeCodes,
                indexProcess,
                videoResult.CurrentVideoTimeCodeId,
                timeCodeResultDict,
                timeCodeQuestionLookup);
            return models;
        }

        private IList<VideoTimeCodeModel> BuildVideoTimeCodeModels(
        IList<VideoTimeCode> videoTimeCodes,
        int? indexProcess,
        Guid? currentVideoTimeCodeId,
        IDictionary<Guid, VideoTimeCodeResult?> timeCodeResultDict,
        ILookup<Guid, TimeCodeQuestionModel> timeCodeQuestionLookup)
        {
            var result = new List<VideoTimeCodeModel>(videoTimeCodes.Count);

            for (int index = 0; index < videoTimeCodes.Count; index++)
            {
                var timeCode = videoTimeCodes[index];
                var model = BuildSingleVideoTimeCodeModel(
                    timeCode,
                    index,
                    indexProcess,
                    currentVideoTimeCodeId,
                    timeCodeResultDict,
                    timeCodeQuestionLookup);

                result.Add(model);
            }

            return result;
        }

        private VideoTimeCodeModel BuildSingleVideoTimeCodeModel(
        VideoTimeCode item,
        int indexTimeCode,
        int? indexProcess,
        Guid? currentVideoTimeCodeId,
        IDictionary<Guid, VideoTimeCodeResult?> timeCodeResultDict,
        ILookup<Guid, TimeCodeQuestionModel> timeCodeQuestionLookup)
        {
            // Lấy data theo Id
            var questionsGroup = timeCodeQuestionLookup[item.Id];
            var questions = questionsGroup.Select(x => x.Question).ToList();
            var courseSkills = questionsGroup
                .Select(x => x.CourseSkill)
                .Distinct()
                .ToList();
            var skillViews = questionsGroup
                .Where(x => x.Skill != null)
                .Select(x => x.Skill!)
                .Distinct()
                .ToList();

            // Lấy result nếu có
            timeCodeResultDict.TryGetValue(item.Id, out var videoTimeCodeResult);
            var videoTimeCodeResultModel = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult);

            if (videoTimeCodeResultModel != null)
            {
                videoTimeCodeResultModel.CurrentVideoTimeCodeId = currentVideoTimeCodeId;
            }

            // Map ra model cuối
            var videoTimeCode = _mapper.Map<VideoTimeCodeModel>(item);
            videoTimeCode.TotalCount = questions.Count;
            videoTimeCode.Ungraded = questions.Any(x => x.Ungraded);
            videoTimeCode.CorrectCount = videoTimeCodeResult?.CorrectCount ?? default;
            videoTimeCode.CorrectTotal = questions.Sum(x => x.CorrectTotal);
            videoTimeCode.CourseSkills = courseSkills;
            videoTimeCode.Skills = skillViews;
            videoTimeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResultModel, item);
            videoTimeCode.Status = GetTimeCodeStatus(indexProcess, indexTimeCode, videoTimeCodeResult);
            return videoTimeCode;
        }

        private async Task<Dictionary<Guid, VideoTimeCodeResult?>> LoadVideoTimeCodeResultDictAsync(VideoResult videoResult)
        {
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate)
                                                                                     .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.UpdatedDate <= videoResult.UpdatedDate)
                                                                                     .ToListAsync();
            return videoTimeCodeResults.ToDictionary(x => x.VideoTimeCodeId, x => (VideoTimeCodeResult?)x);
        }

        private async Task<ILookup<Guid, TimeCodeQuestionModel>> LoadTimeCodeQuestionLookupAsync(Guid videoId)
        {
            var queryData = await GetTimeCodeQuestionsAsync(videoId);
            return queryData.ToLookup(x => x.Id); // Id = VideoTimeCodeId
        }

        private static int? GetIndexProcess(IList<VideoTimeCode>? videoTimeCodes, Guid? currentVideoTimeCodeId)
        {
            var videoTimeCode = videoTimeCodes?.FirstOrDefault(x => x.Id == currentVideoTimeCodeId);
            return videoTimeCode != null ? videoTimeCodes?.IndexOf(videoTimeCode) : null;
        }

        private VideoTimeCodeResultModel? GetVideoTimeCodeResult(VideoTimeCodeResultModel? videoTimeCodeResult, VideoTimeCode videoTimeCode)
        {
            if (videoTimeCodeResult == null)
            {
                return videoTimeCodeResult;
            }
            double remainingTime;
            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                remainingTime = videoTimeCodeResult.WorkingTime;
            }
            else
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    remainingTime = videoTimeCodeResult.WorkingTime;
                }
                else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    remainingTime = videoTimeCodeResult.RetryWorkingTime;
                }
                else
                {
                    remainingTime = videoTimeCodeResult.RetryWorkingTime == default ? videoTimeCodeResult.WorkingTime : videoTimeCodeResult.RetryWorkingTime;
                }
            }
            videoTimeCodeResult.RemainingTime = _dateTimeConverter.SetRemainingTime(videoTimeCode.ExecutionTime, remainingTime);
            return videoTimeCodeResult;
        }

        private static EnumResultStatus GetTimeCodeStatus(int? indexProcess, int indexTimeCode, VideoTimeCodeResult? videoTimeCodeResult)
        {
            var status = EnumResultStatus.Unfinished;
            if (indexProcess == indexTimeCode)
            {
                status = EnumResultStatus.Process;
                if (videoTimeCodeResult != null && videoTimeCodeResult.Status == EnumResultStatus.Done)
                {
                    status = EnumResultStatus.Done;
                }
            }
            else if (indexProcess > indexTimeCode)
            {
                status = EnumResultStatus.Done;
            }
            return status;
        }

        private async Task<IList<TimeCodeQuestionModel>> GetTimeCodeQuestionsAsync(Guid id)
        {
            return await _timeCodeQuestionCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var query = from baseQ in _videoTimeCodeRepository.ReadQueryable
                            join te in _timeCodeExerciseRepository.ReadQueryable on baseQ.Id equals te.VideoTimeCodeId
                            join eq in _exerciseQuestionRepository.ReadQueryable on te.ExerciseId equals eq.ExerciseId
                            where baseQ.VideoId == id
                            select new TimeCodeQuestionModel
                            {
                                Id = baseQ.Id,
                                CourseSkill = te.Exercise!.CourseSkill,
                                Question = eq.Question,
                                ExerciseId = te.ExerciseId,
                                Skill = _mapper.Map<SkillViewModel>(te.Exercise!.Skill)
                            };
                return await query.ToListAsync(_);
            });
        }

        private async Task<IList<TimeCodeQuestionModel>> GetDetailtTimeCodeQuestionsAsync(Guid videoTimeCodeId)
        {
            return await _timeCodeQuestionCachingService.GetOrSetAsync(videoTimeCodeId.ToString(), async (ctx, _) =>
            {
                var query = from baseQ in _videoTimeCodeRepository.ReadQueryable
                            join te in _timeCodeExerciseRepository.ReadQueryable on baseQ.Id equals te.VideoTimeCodeId
                            join eq in _exerciseQuestionRepository.ReadQueryable on te.ExerciseId equals eq.ExerciseId
                            where baseQ.Id == videoTimeCodeId
                            select new TimeCodeQuestionModel
                            {
                                Id = baseQ.Id,
                                CourseSkill = te.Exercise!.CourseSkill,
                                Question = eq.Question,
                                ExerciseId = te.ExerciseId,
                                Skill = _mapper.Map<SkillViewModel>(te.Exercise!.Skill)
                            };
                return await query.ToListAsync(_);
            });
        }

        private async Task<Video?> GetVideoAsync(Guid id)
        {
            var video = await _videoRepository.ReadQueryable
                                                  .Where(x => x.Id == id)
                                                  .Include(v => v.VideoTimeCodes)
                                                  .Include(v => v.VideoSubFilePaths)
                                                  .FirstOrDefaultAsync();
            return video;
        }

        #endregion Build VideoModel

        #region Create Answers

        public async Task<VoidMethodResult> CreateAnswers(CreateVideoTimeCodeAnswerV1i1CommandModel request, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.Answers == null || !request.Answers.Any())
            {
                return methodResult;
            }

            var videoTimeCode = videoTimeCodeResult.VideoTimeCode;
            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable
                                                                           .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                                                                           .ToListAsync(cancellationToken);

            var timeCodeQuestions = await GetDetailtTimeCodeQuestionsAsync(request.VideoTimeCodeId);
            var requestQuestionIds = request.Answers.Select(x => x.QuestionId).ToList();

            var validQuestionIds = timeCodeQuestions.Where(x => x.Question != null)
                                                    .Select(x => x.Question!.Id)
                                                    .ToHashSet();

            // Tìm các QuestionId không thuộc TimeCode
            var invalidQuestionIds = requestQuestionIds.Where(q => !validQuestionIds.Contains(q)).ToList();
            if (invalidQuestionIds.Any())
            {
                // Báo lỗi rõ ràng
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat),
                    $"Các câu hỏi sau không thuộc TimeCode: {string.Join(", ", invalidQuestionIds)}"
                );
                return methodResult;
            }

            var createVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            foreach (var item in request.Answers)
            {
                var timeCodeQuestion = timeCodeQuestions.FirstOrDefault(x => x.Question != null && x.Question.Id == item.QuestionId);
                if (timeCodeQuestion == null)
                {
                    continue;
                }

                var videoTimeCodeAnswer = videoTimeCodeAnswers.FirstOrDefault(x => x.QuestionId == item.QuestionId);
                var isProcess = videoTimeCodeResult.Status == EnumResultStatus.Process;
                var questionResult = _questionConverter.HandleQuestionAnswer(timeCodeQuestion.Question, item.Answer, request.IsSubmit, videoTimeCodeAnswer?.Answer, isProcess, false);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                var isFirstSubmit = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.New;
                if (videoTimeCodeAnswer == null)
                {
                    videoTimeCodeAnswer = new VideoTimeCodeAnswer
                    {
                        VideoTimeCodeId = videoTimeCode.Id,
                        ExerciseId = timeCodeQuestion.ExerciseId,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoTimeCodeResult.VideoResultId,
                    };
                    createVideoTimeCodeAnswers.Add(videoTimeCodeAnswer);
                }
                else if (videoTimeCodeAnswer.Status != EnumAnswerStatus.Done)
                {
                    updateVideoTimeCodeAnswers.Add(videoTimeCodeAnswer);
                }
                videoTimeCodeAnswer = GetVideoTimeCodeAnswer(videoTimeCodeAnswer, questionItem, correctCount, answerConfig ?? item.Answer, isFirstSubmit, isAnswered);
                if (!videoTimeCodeAnswer.IsValid())
                {
                    methodResult.AddErrorBadRequest(videoTimeCodeAnswer.ErrorMessages);
                    return methodResult;
                }
            }
            try
            {
                if (createVideoTimeCodeAnswers.Any())
                {
                    await _videoTimeCodeAnswerRepository.BulkMergeAsync(createVideoTimeCodeAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeResultId, entity.QuestionId, entity.IsDeleted };
                    });
                }
                else if (updateVideoTimeCodeAnswers.Any())
                {
                    await _videoTimeCodeAnswerRepository.BulkUpdateList(updateVideoTimeCodeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeResultId, entity.QuestionId };
                    });
                }
            }
            catch
            {
            }
            return methodResult;
        }

        private static VideoTimeCodeAnswer GetVideoTimeCodeAnswer(VideoTimeCodeAnswer answer, Question question, short correctCount, object? answerConfig, bool isFirstSubmit, bool isAnswered)
        {
            answer.Answer = answerConfig;
            answer.CorrectCount = correctCount;
            answer.Status = EnumAnswerStatus.Process;
            answer.IsCorrect = isAnswered ? correctCount == question.CorrectTotal : null;
            answer.IsFirstSubmit = isFirstSubmit;
            return answer;
        }

        #endregion Create Answers

        #region SkillScores

        public async Task<(IList<SkillScores>?, IList<SkillScores>, bool)> GetSkillScoresAsync(
        VideoTimeCodeResult videoTimeCodeResult,
        VideoTimeCode videoTimeCode,
        bool isTimeUp,
        CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);

            // Lấy tất cả answer của TimeCodeResult này
            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.ReadQueryable
                .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                .ToListAsync(cancellationToken);

            // Lấy toàn bộ câu hỏi thuộc TimeCode này
            var timeCodeQuestions = await GetDetailtTimeCodeQuestionsAsync(videoTimeCodeResult.VideoTimeCodeId);
            if (timeCodeQuestions == null || !timeCodeQuestions.Any())
            {
                return (new List<SkillScores>(), new List<SkillScores>(), false);
            }

            // Lookup QuestionId -> list answer (cùng VideoTimeCodeResult)
            var answersByQuestionId = videoTimeCodeAnswers
                .GroupBy(a => a.QuestionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Còn câu nào chưa Done không?

            var hasNotDone = false;
            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                if (videoTimeCodeAnswers.Count == timeCodeQuestions.Count)
                {
                    hasNotDone = videoTimeCodeResult.Status != EnumResultStatus.New || videoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Done);
                }
                else if (isTimeUp)
                {
                    hasNotDone = videoTimeCodeResult.Status != EnumResultStatus.New;
                }
            }
            else if (videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest || videoTimeCode.TimeCodeType == EnumTimeCodeType.SkillTest)
            {
                hasNotDone = true;
            }

            // Group theo CourseSkill (lấy từ timeCodeQuestions)
            var groupedBySkill = timeCodeQuestions
                .Where(x => x.Question != null)
                .GroupBy(x => new { x.CourseSkill, x.Skill.Id });     // nếu property tên khác thì sửa lại chỗ này

            var ungradedScores = new List<SkillScores>();
            var gradedScores = new List<SkillScores>();

            foreach (var skillGroup in groupedBySkill)
            {
                var courseSkill = skillGroup.Key.CourseSkill;
                var skill = skillGroup.First().Skill;

                // Câu hỏi chưa chấm (Ungraded)
                var ungradedQuestions = skillGroup
                    .Where(x => x.Question != null && x.Question!.Ungraded)
                    .Select(x => x.Question!)
                    .ToList();

                // Câu hỏi đã chấm (graded)
                var gradedQuestions = skillGroup
                    .Where(x => x.Question != null && !x.Question!.Ungraded)
                    .Select(x => x.Question!)
                    .ToList();

                var ungraded = BuildSkillScores(ungradedQuestions, answersByQuestionId, courseSkill, skill);
                if (ungraded != null)
                {
                    ungradedScores.Add(ungraded);
                }

                var graded = BuildSkillScores(gradedQuestions, answersByQuestionId, courseSkill, skill);
                if (graded != null)
                {
                    gradedScores.Add(graded);
                }
            }

            return (ungradedScores, gradedScores, hasNotDone);
        }

        private static SkillScores? BuildSkillScores(
        IList<Question> questions,
        IDictionary<Guid, List<VideoTimeCodeAnswer>> answersByQuestionId,
        EnumCourseSkill courseSkill,
        SkillViewModel? skill)
        {
            if (questions == null || questions.Count == 0)
            {
                return null;
            }

            // Lấy tất cả answer tương ứng các câu hỏi này
            var allAnswers = questions
                .Where(q => answersByQuestionId.ContainsKey(q.Id))
                .SelectMany(q => answersByQuestionId[q.Id])
                .ToList();

            var correctCount = allAnswers.Sum(a => a.CorrectCount);
            var tokenReceived = allAnswers.Sum(a => a.TokenReceived);
            var totalCount = questions.Sum(q => q.CorrectTotal);
            var totalQuestion = questions.Count;

            return new SkillScores
            {
                CorrectCount = correctCount,
                CorrectQuestion = allAnswers.Count(a => a.IsCorrect == true),
                CountQuestion = allAnswers.Count,
                Skill = courseSkill,
                TotalCount = totalCount,
                SkillId = skill?.Id,
                SkillName = skill?.Name,
                SkillFilePath = skill?.FilePath,
                TotalQuestion = totalQuestion,
                Scores = correctCount.GetIeltsScore(courseSkill),
                TokenReceived = tokenReceived
            };
        }

        #endregion SkillScores

        #region Update Video Answers

        public async Task<long> UpdateVideoAnswers(
        VideoTimeCode videoTimeCode,
        VideoTimeCodeResult videoTimeCodeResult,
        bool isDoneTimeCode = false,
        bool isSubmit = true,
        CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);

            // Load tất cả answer của TimeCodeResult này
            var answersToUpdate = await _videoTimeCodeAnswerRepository.Queryable
                .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                .Include(x => x.Question)
                .Where(x => x.Status != EnumAnswerStatus.Done)
                .ToListAsync(cancellationToken);

            if (!answersToUpdate.Any())
            {
                return default;
            }

            if (answersToUpdate.Any())
            {
                foreach (var answer in answersToUpdate)
                {
                    if (answer.Question == null)
                    {
                        continue;
                    }

                    // Tính status theo rule hiện tại
                    var status = GetAnswerStatus(videoTimeCode.TimeCodeType,
                        isSubmit,
                        answer.CorrectCount,
                        answer.Question.CorrectTotal);

                    // Nếu timeCode được coi là "done" ở tầng trên -> ép Done
                    if (isDoneTimeCode)
                    {
                        status = EnumAnswerStatus.Done;
                    }

                    answer.Status = status;
                    // Giữ nguyên ý cũ: chỉ set IsCorrect nếu trước đó đã có giá trị
                    answer.IsCorrect = answer.IsCorrect.HasValue ? answer.CorrectCount == answer.Question.CorrectTotal : null;
                }

                await _videoTimeCodeAnswerRepository.BulkUpdateList(answersToUpdate, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.IsCorrect,
                        entity.Status
                    };
                });
            }

            var gradedDoneAnswers = answersToUpdate
                .Where(x => x.Question != null && !x.Question.Ungraded && x.Question.QuestionType != EnumQuestionType.ExercisePreparation)
                .Where(x => x.Status == EnumAnswerStatus.Done);

            return gradedDoneAnswers.Sum(x => x.CorrectCount);
        }

        private static EnumAnswerStatus GetAnswerStatus(EnumTimeCodeType? timeCodeType, bool isSubmit, int correctCount, int correctTotal)
        {
            if (timeCodeType == EnumTimeCodeType.Standalone)
            {
                return correctCount == correctTotal && isSubmit ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
            }
            return EnumAnswerStatus.Done;
        }

        #endregion Update Video Answers
    }
}
