// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.Videos;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class VideoConverter
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IMapper _mapper;

        public VideoConverter(IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IExerciseRepository exerciseRepository
            , QuestionTypeConverter questionTypeConverter
            , IVideoResultRepository videoResultRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IExerciseQuestionRepository exerciseQuestionRepository
            , ITimeCodeExerciseRepository timeCodeExerciseRepository
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _exerciseRepository = exerciseRepository;
            _questionTypeConverter = questionTypeConverter;
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _mapper = mapper;
        }

        public VoidMethodResult AddQuestionToExercise(dynamic newExercise, CreateExerciseCommandModel? exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (exercise.Questions == null || exercise.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise.Questions));
                return methodResult;
            }
            foreach (var question in exercise.Questions)
            {
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                    return methodResult;
                }
                var newQuestion = _mapper.Map<Question>(question);
                newExercise.ExerciseQuestions.Add(new ExerciseQuestion
                {
                    Exercise = newExercise,
                    Question = newQuestion
                });
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question!.Config, question.QuestionType, isShowCorrectTotal: true, false);
                if (config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                    return methodResult;
                }
                newQuestion.CorrectTotal = correctTotal;
                if (!newQuestion.IsValid())
                {
                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                    return methodResult;
                }
            }
            if (!newExercise.IsValid())
            {
                methodResult.AddErrorBadRequest(newExercise.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }

        public VoidMethodResult AddTimeCodeToVideo(dynamic video, IList<CreateVideoTimeCodeCommandModel>? videoTimeCodes)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCodes);
            VoidMethodResult methodResult = new VoidMethodResult();

            var listTimeCodeType = videoTimeCodes.Select(x => x.TimeCodeType).ToList();
            if (listTimeCodeType.Contains(EnumTimeCodeType.UnitTest) && listTimeCodeType.Contains(EnumTimeCodeType.SkillTest))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.CanNotUnitTestAndSkillTestAtTheSameTime), nameof(videoTimeCodes));
                return methodResult;
            }
            video.VideoTimeCodes.Clear();
            foreach (var timeCode in videoTimeCodes)
            {
                if (timeCode == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(timeCode));
                    return methodResult;
                }
                var newTimeCode = _mapper.Map<VideoTimeCode>(timeCode);
                if (timeCode.Exercises == null || timeCode.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(timeCode.Exercises));
                    return methodResult;
                }
                foreach (var exercise in timeCode.Exercises)
                {
                    if (exercise == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise));
                        return methodResult;
                    }
                    var newExercise = _mapper.Map<Exercise>(exercise);
                    var method = AddQuestionToExercise(newExercise, exercise);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    newTimeCode.TimeCodeExercises.Add(new TimeCodeExercise
                    {
                        Exercise = newExercise
                    });
                }
                video.VideoTimeCodes.Add(newTimeCode);
                if (!newTimeCode.IsValid())
                {
                    methodResult.AddErrorBadRequest(newTimeCode.ErrorMessages);
                    return methodResult;
                }
            }
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            else if (!methodResult.IsOK)
            {
                return methodResult;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> CreateTimeCodeToVideo(dynamic video, CreateVideoCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (request.VideoTimeCodes == null || request.VideoTimeCodes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodes));
                return methodResult;
            }

            var isExistName = await _videoRepository.Queryable.AnyAsync(x => x.Name == request.Name);
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNameIsExist), nameof(request.Name), request.Name);
                return methodResult;
            }
            var method = AddTimeCodeToVideo(video, request.VideoTimeCodes);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> UpdateTimeCodeToVideo(dynamic video, UpdateVideoCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.VideoTimeCodes == null || request.VideoTimeCodes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodes));
                return methodResult;
            }
            var isVideoUsed = await _videoRepository.IsVideoUsed(request.Id);
            if (isVideoUsed)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoUsed), nameof(request.Id), request.Id);
                return methodResult;
            }

            var isExistName = await _videoRepository.Queryable.AnyAsync(x => x.Name == request.Name && x.Id != request.Id);
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNameIsExist), nameof(request.Name), request.Name);
                return methodResult;
            }
            var method = AddTimeCodeToVideo(video, request.VideoTimeCodes);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> DeleteExerciseToVideo(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);
            VoidMethodResult methodResult = new VoidMethodResult();

            List<Exercise> exercises = video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise!).ToList();
            List<Question> questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question!).ToList();
            foreach (var item in exercises)
            {
                await _exerciseRepository.DeleteAsync(item);
            }
            await _exerciseRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            foreach (var item in questions)
            {
                await _questionRepository.DeleteAsync(item);
            }
            await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return methodResult;
        }

        public async Task<VoidMethodResult> GetVideoResultDone(VideoResult videoResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoResult);
            VoidMethodResult methodResult = new VoidMethodResult();
            var (listSkillScore, isEnoughQuestion) = await GetVideoSkillScores(videoResult, cancellationToken);
            if (isEnoughQuestion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.NotEnoughQuestions));
                return methodResult;
            }
            var skillScores = listSkillScore.Where(x => x.Type == EnumTimeCodeType.Standalone && x.SkillScores?.Count > 0).SelectMany(x => x.SkillScores!).ToList();
            videoResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            videoResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            videoResult.Status = EnumResultStatus.Done;
            videoResult.VideoSkillScores = listSkillScore;
            return methodResult;
        }

        public async Task<(IList<VideoSkillScores>, bool)> GetVideoSkillScores(VideoResult videoResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoResult);
            var answerQuery = from baseQ in _videoResultRepository.Queryable
                              join vtcr in _videoTimeCodeResultRepository.Queryable on baseQ.Id equals vtcr.VideoResultId
                              join vtca in _videoTimeCodeAnswerRepository.Queryable on vtcr.Id equals vtca.VideoTimeCodeResultId
                              join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              where baseQ.Id == videoResult.Id
                              group new { vt, vtca } by new { vt.TimeCodeType, e.CourseSkill } into g
                              select new
                              {
                                  Type = g.Key.TimeCodeType,
                                  Skill = g.Key.CourseSkill,
                                  CorrectCount = g.Sum(x => x.vtca.CorrectCount),
                                  TotalAnswer = g.Select(x => x.vtca).Count()
                              };

            var questionQuery = from baseQ in _videoResultRepository.Queryable
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                where baseQ.Id == videoResult.Id && q.QuestionType != EnumQuestionType.ExercisePreparation
                                group new { vt, q } by new { vt.TimeCodeType, e.CourseSkill } into g
                                select new
                                {
                                    Type = g.Key.TimeCodeType,
                                    Skill = g.Key.CourseSkill,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal),
                                    TotalQuestion = g.Select(x => x.q).Count()
                                };
            var questions = await questionQuery.ToListAsync(cancellationToken);
            var answers = await answerQuery.ToListAsync(cancellationToken);
            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var types = Enum.GetValues(typeof(EnumTimeCodeType)).Cast<EnumTimeCodeType>();
            var scoreQuery = from type in types
                             select new VideoSkillScores
                             {
                                 Type = type,
                                 SkillScores = (from skill in skills
                                                join questionTimeCodeQ in questions on skill equals questionTimeCodeQ.Skill into questionTimeCodeQ_jointable
                                                from questionTimeCodeQJ in questionTimeCodeQ_jointable.DefaultIfEmpty()
                                                join answerTimeCodeQ in answerQuery on skill equals answerTimeCodeQ.Skill into answerTimeCodeQ_jointable
                                                from answerTimeCodeQJ in answerTimeCodeQ_jointable.DefaultIfEmpty()
                                                where questionTimeCodeQJ != null && questionTimeCodeQJ.Type == type && (!(answerTimeCodeQJ != null) || answerTimeCodeQJ.Type == type)
                                                select new SkillScores
                                                {
                                                    Skill = skill,
                                                    TotalCount = questionTimeCodeQJ.TotalCount,
                                                    CorrectCount = answerTimeCodeQJ != null ? answerTimeCodeQJ.CorrectCount : default,
                                                    TotalQuestion = questionTimeCodeQJ.TotalQuestion,
                                                    CountQuestion = answerTimeCodeQJ != null ? answerTimeCodeQJ.TotalAnswer : default,
                                                }).ToList()
                             };
            return (scoreQuery.ToList(), questions.Sum(x => x.TotalQuestion) != answers.Sum(x => x.TotalAnswer));
        }

        private static bool GetUngraded(VideoTimeCode? videoTimeCode)
        {
            return videoTimeCode?.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).FirstOrDefault()?.Ungraded ?? default;
        }

        private static int GetCorrectCount(VideoTimeCode? videoTimeCode)
        {
            return (videoTimeCode != null && videoTimeCode.VideoTimeCodeAnswers.Any()) ? videoTimeCode.VideoTimeCodeAnswers.Sum(x => x.CorrectCount) : default;
        }

        private static int GetCorrectTotal(VideoTimeCode? videoTimeCode)
        {
            return videoTimeCode?.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal) ?? default;
        }

        public static int GetTotalQuestion(VideoTimeCode? videoTimeCode)
        {
            return videoTimeCode?.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null)
                                                  .Select(x => x.Exercise)
                                                  .SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null))
                                                  .Select(m => m.Question)
                                                  .Count() ?? default;
        }

        public VideoTimeCodeModel GetVideoTimeCode(VideoTimeCode? videoTimeCode, VideoTimeCodeResultModel? videoTimeCodeResult)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            var timeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode);
            timeCode.TotalCount = GetTotalQuestion(videoTimeCode);
            timeCode.Ungraded = GetUngraded(videoTimeCode);
            timeCode.CorrectCount = GetCorrectCount(videoTimeCode);
            timeCode.CorrectTotal = GetCorrectTotal(videoTimeCode);
            timeCode.Status = GetTimeCodeStatus(videoTimeCode);
            timeCode.VideoTimeCodeResult = videoTimeCodeResult;
            timeCode.Exercises = videoTimeCode.TimeCodeExercises.OrderBy(x => x!.CreatedDate).Select(n => n.Exercise).Select(n => GetExercise(n)).ToList();
            return timeCode;
        }

        public IList<VideoTimeCodeModel> GetTimeCodes(Video? video, Guid videoResultId, bool isShowTotalCount = false)
        {
            ArgumentNullException.ThrowIfNull(video);
            var videoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).ToList();
            var videoTimeCodeModels = new List<VideoTimeCodeModel>();
            var indexProcess = GetIndexProcess(videoTimeCodes, videoResultId);
            foreach (var item in videoTimeCodes)
            {
                var indexTimeCode = videoTimeCodes.IndexOf(item);
                var videoTimeCode = GetVideoTimeCode(item, default);
                videoTimeCode.TotalCount = isShowTotalCount ? GetTotalQuestion(item) : default;
                videoTimeCode.Status = GetTimeCodeStatus(indexProcess, indexTimeCode);
                videoTimeCodeModels.Add(videoTimeCode);
            }
            return videoTimeCodeModels;
        }

        private ExerciseModel GetExercise(Exercise? n)
        {
            ArgumentNullException.ThrowIfNull(n);
            var exerciseModel = _mapper.Map<ExerciseModel>(n);
            exerciseModel.Questions = n.ExerciseQuestions.OrderBy(x => x!.CreatedDate).Select(m => m.Question).Select(m => GetQuestion(m)).ToList();
            return exerciseModel;
        }

        private QuestionModel GetQuestion(Question? question)
        {
            ArgumentNullException.ThrowIfNull(question);
            var isCheck = question.VideoTimeCodeAnswers.FirstOrDefault()?.Status == EnumAnswerStatus.Done;
            var questionModel = _mapper.Map<QuestionModel>(question);
            questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isCheck)).Item1;
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(question.VideoTimeCodeAnswers.FirstOrDefault());
            return questionModel;
        }

        private static EnumResultStatus GetTimeCodeStatus(VideoTimeCode? videoTimeCode)
        {
            var status = EnumResultStatus.Process;
            if (videoTimeCode != null)
            {
                if (videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Done))
                {
                    status = EnumResultStatus.Done;
                }
            }
            return status;
        }

        private static EnumResultStatus GetTimeCodeStatus(int? indexProcess, int indexTimeCode)
        {
            var status = EnumResultStatus.Unfinished;
            if (indexProcess < indexTimeCode)
            {
                return status;
            }
            else if (indexProcess == indexTimeCode)
            {
                status = EnumResultStatus.Process;
            }
            else if (indexProcess > indexTimeCode || indexProcess == null)
            {
                status = EnumResultStatus.Done;
            }
            return status;
        }

        private static int? GetIndexProcess(IList<VideoTimeCode>? videoTimeCodes, Guid videoResultId)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCodes);
            var timeCode = videoTimeCodes.Where(x => !x.VideoTimeCodeAnswers.Any() || x.VideoTimeCodeAnswers.Any(x => x.VideoResultId == videoResultId && x.Status != EnumAnswerStatus.Done)).FirstOrDefault();
            if (timeCode == null)
            {
                return null;
            }
            return videoTimeCodes.IndexOf(timeCode);
        }

        private async Task<IList<Guid>?> GetUnansweredQuestionIds(Guid videoTimeCodeId, VideoTimeCodeResult videoTimeCodeResult)
        {
            var videoTimeCode = await _videoTimeCodeRepository.Queryable
                                    .Include(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id))
                                    .FirstOrDefaultAsync(x => x.Id == videoTimeCodeId);
            if (videoTimeCode == null)
            {
                return default;
            }
            var questions = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).ToList();
            var questionCompleteIds = questions.SelectMany(x => x!.VideoTimeCodeAnswers).Select(x => x.QuestionId).ToList();
            return questions.Select(x => x!.Id).Except(questionCompleteIds).ToList();
        }

        public async Task UpdateVideoAnswers(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            var questionIds = await GetUnansweredQuestionIds(videoTimeCode.Id, videoTimeCodeResult);
            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            if (questionIds != null && questionIds.Any())
            {
                videoTimeCodeAnswers = questionIds.Select(x => new VideoTimeCodeAnswer
                {
                    Answer = null,
                    QuestionId = x,
                    VideoResultId = videoTimeCodeResult.VideoResultId,
                    VideoTimeCodeResultId = videoTimeCodeResult.Id,
                    VideoTimeCodeId = videoTimeCode.Id
                }).ToList();

                await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
        }
    }
}
