// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
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
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IMapper _mapper;
        private readonly IQuestionShuffleRepository _questionShuffleRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly ISkillRepository _skillRepository;
        private const int NumberOfQuestion = 1;
        private List<VideoTimeCode> DeleteVideoTimeCodes = new List<VideoTimeCode>();
        private List<Exercise> DeleteExercises = new List<Exercise>();
        private List<Question> DeleteQuestions = new List<Question>();

        public VideoConverter(IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IExerciseRepository exerciseRepository
            , QuestionTypeConverter questionTypeConverter
            , AnswerTypeConverter answerTypeConverter
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , QuestionConverter questionConverter
            , IExerciseQuestionRepository exerciseQuestionRepository
            , ITimeCodeExerciseRepository timeCodeExerciseRepository
            , DateTimeConverter dateTimeConverter
            , IMapper mapper
            , IQuestionShuffleRepository questionShuffleRepository
            , IQuestionExplanationErrorRepository questionExplanationErrorRepository
            , ISkillRepository skillRepository)
        {
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _exerciseRepository = exerciseRepository;
            _questionTypeConverter = questionTypeConverter;
            _answerTypeConverter = answerTypeConverter;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _questionConverter = questionConverter;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _dateTimeConverter = dateTimeConverter;
            _mapper = mapper;
            _questionShuffleRepository = questionShuffleRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _skillRepository = skillRepository;
        }

        #region ADD

        public VoidMethodResult AddQuestionToExercise(Exercise exercise, CreateExerciseCommandModel? exerciseRequest)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            ArgumentNullException.ThrowIfNull(exerciseRequest);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (exerciseRequest.Questions == null || exerciseRequest.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest.Questions));
                return methodResult;
            }

            var listQuestion = exercise.ExerciseQuestions.Select(x => x.Question).Select(x => x ?? new Question()).ToList();
            foreach (var questionRequest in exerciseRequest.Questions)
            {
                if (questionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionRequest));
                    return methodResult;
                }
                Question? question;
                if (questionRequest.Id.HasValue)
                {
                    question = listQuestion.FirstOrDefault(x => x.Id == questionRequest.Id.Value);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), questionRequest.Id);
                        return methodResult;
                    }
                    _mapper.Map(questionRequest, question);
                }
                else
                {
                    question = _mapper.Map<Question>(questionRequest);
                }

                var method = _questionConverter.HandleQuestion(question);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                if (method.Result != null)
                {
                    if (!method.Result.IsValid())
                    {
                        methodResult.AddErrorBadRequest(method.Result.ErrorMessages);
                        return methodResult;
                    }
                    if (method.Result.Id == Guid.Empty)
                    {
                        exercise.ExerciseQuestions.Add(new ExerciseQuestion
                        {
                            Question = question,
                        });
                    }
                }
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> AddTimeCodeToVideo(Video video, IList<CreateVideoTimeCodeCommandModel>? requestVideoTimeCodes)
        {
            ArgumentNullException.ThrowIfNull(requestVideoTimeCodes);
            ArgumentNullException.ThrowIfNull(video);
            VoidMethodResult methodResult = new VoidMethodResult();

            var listTimeCodeType = requestVideoTimeCodes.Select(x => x.TimeCodeType).ToList();
            if (listTimeCodeType.Contains(EnumTimeCodeType.UnitTest) && listTimeCodeType.Contains(EnumTimeCodeType.SkillTest))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.CanNotUnitTestAndSkillTestAtTheSameTime), nameof(requestVideoTimeCodes));
                return methodResult;
            }
            var listTimeCode = video.VideoTimeCodes;
            foreach (var videoTimeCodeRequest in requestVideoTimeCodes)
            {
                if (videoTimeCodeRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeRequest));
                    return methodResult;
                }
                if (videoTimeCodeRequest.Exercises == null || videoTimeCodeRequest.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeRequest.Exercises));
                    return methodResult;
                }

                VideoTimeCode? videoTimeCode;
                if (videoTimeCodeRequest.Id.HasValue)
                {
                    videoTimeCode = listTimeCode.FirstOrDefault(x => x.Id == videoTimeCodeRequest.Id);
                    if (videoTimeCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode), videoTimeCodeRequest.Id);
                        return methodResult;
                    }
                    _mapper.Map(videoTimeCodeRequest, videoTimeCode);
                }
                else
                {
                    videoTimeCode = _mapper.Map<VideoTimeCode>(videoTimeCodeRequest);
                }
                var listExercise = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).Select(x => x ?? new Exercise()).ToList();
                var listQuestion = listExercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();
                foreach (var exerciseRequest in videoTimeCodeRequest.Exercises)
                {
                    if (exerciseRequest == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest));
                        return methodResult;
                    }
                    if (exerciseRequest.SkillId.HasValue)
                    {
                        var skillExists = await _skillRepository.AnyGuidAsync(exerciseRequest.SkillId.Value);
                        if (!skillExists)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest.SkillId), exerciseRequest.SkillId);
                            return methodResult;
                        }
                    }
                    Exercise? exercise;
                    if (exerciseRequest.Id.HasValue)
                    {
                        exercise = listExercise.FirstOrDefault(x => x.Id == exerciseRequest.Id.Value);
                        if (exercise == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise), exerciseRequest.Id);
                            return methodResult;
                        }
                        _mapper.Map(exerciseRequest, exercise);
                    }
                    else
                    {
                        exercise = _mapper.Map<Exercise>(exerciseRequest);
                    }
                    if (!exercise.IsValid())
                    {
                        methodResult.AddErrorBadRequest(exercise.ErrorMessages);
                        return methodResult;
                    }
                    var method = AddQuestionToExercise(exercise, exerciseRequest);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    if (exercise.Id == Guid.Empty)
                    {
                        videoTimeCode.TimeCodeExercises.Add(new TimeCodeExercise
                        {
                            VideoTimeCode = videoTimeCode,
                            Exercise = exercise
                        });
                    }
                }

                var questions = videoTimeCodeRequest.Exercises.SelectMany(x => x.Questions ?? new List<CreateQuestionCommandModel>());
                var questionsToDelete = listQuestion.Where(x => x != null && x.Id != Guid.Empty && !questions.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
                if (questionsToDelete.Any())
                {
                    DeleteQuestions.AddRange(questionsToDelete);
                }

                var exercisesToDelete = listExercise.Where(x => x != null && x.Id != Guid.Empty && !videoTimeCodeRequest.Exercises.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
                if (exercisesToDelete.Any())
                {
                    DeleteExercises.AddRange(exercisesToDelete);
                }

                if (!videoTimeCode.IsValid())
                {
                    methodResult.AddErrorBadRequest(videoTimeCode.ErrorMessages);
                    return methodResult;
                }
                if (videoTimeCode.Id == Guid.Empty)
                {
                    video.VideoTimeCodes.Add(videoTimeCode);
                }
            }
            var videoTimeCodesToDelete = listTimeCode.Where(x => x != null && x.Id != Guid.Empty && !requestVideoTimeCodes.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
            if (videoTimeCodesToDelete.Any())
            {
                DeleteVideoTimeCodes.AddRange(videoTimeCodesToDelete);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            await DeleteDataVideoAsync();
            return methodResult;
        }

        public async Task<VoidMethodResult> CreateTimeCodeToVideo(Video video, CreateVideoCommandModel? request)
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

            var method = await AddTimeCodeToVideo(video, request.VideoTimeCodes);
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

        #endregion ADD

        #region Update

        public VoidMethodResult UpdateQuestionToExercise(Exercise exercise, UpdateExerciseCommandModel? exerciseRequest)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            ArgumentNullException.ThrowIfNull(exerciseRequest);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (exerciseRequest.Questions == null || exerciseRequest.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest.Questions));
                return methodResult;
            }

            var listQuestion = exercise.ExerciseQuestions.Select(x => x.Question).Select(x => x ?? new Question()).ToList();
            foreach (var questionRequest in exerciseRequest.Questions)
            {
                if (questionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionRequest));
                    return methodResult;
                }
                Question? question;
                if (questionRequest.Id.HasValue)
                {
                    question = listQuestion.FirstOrDefault(x => x.Id == questionRequest.Id.Value);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), questionRequest.Id);
                        return methodResult;
                    }
                    _mapper.Map(questionRequest, question);
                }
                else
                {
                    question = _mapper.Map<Question>(questionRequest);
                }

                var method = _questionConverter.HandleQuestion(question);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                if (method.Result != null)
                {
                    if (!method.Result.IsValid())
                    {
                        methodResult.AddErrorBadRequest(method.Result.ErrorMessages);
                        return methodResult;
                    }
                    if (method.Result.Id == Guid.Empty)
                    {
                        exercise.ExerciseQuestions.Add(new ExerciseQuestion
                        {
                            Question = question,
                        });
                    }
                }
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> UpdateTimeCodeToVideo(Video video, IList<UpdateVideoTimeCodeCommandModel>? requestVideoTimeCodes)
        {
            ArgumentNullException.ThrowIfNull(requestVideoTimeCodes);
            ArgumentNullException.ThrowIfNull(video);
            VoidMethodResult methodResult = new VoidMethodResult();

            var listTimeCodeType = requestVideoTimeCodes.Select(x => x.TimeCodeType).ToList();
            if (listTimeCodeType.Contains(EnumTimeCodeType.UnitTest) && listTimeCodeType.Contains(EnumTimeCodeType.SkillTest))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.CanNotUnitTestAndSkillTestAtTheSameTime), nameof(requestVideoTimeCodes));
                return methodResult;
            }
            var listTimeCode = video.VideoTimeCodes;
            foreach (var videoTimeCodeRequest in requestVideoTimeCodes)
            {
                if (videoTimeCodeRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeRequest));
                    return methodResult;
                }
                if (videoTimeCodeRequest.Exercises == null || videoTimeCodeRequest.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeRequest.Exercises));
                    return methodResult;
                }

                VideoTimeCode? videoTimeCode;
                if (videoTimeCodeRequest.Id.HasValue)
                {
                    videoTimeCode = listTimeCode.FirstOrDefault(x => x.Id == videoTimeCodeRequest.Id);
                    if (videoTimeCode == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode), videoTimeCodeRequest.Id);
                        return methodResult;
                    }
                    _mapper.Map(videoTimeCodeRequest, videoTimeCode);
                }
                else
                {
                    videoTimeCode = _mapper.Map<VideoTimeCode>(videoTimeCodeRequest);
                }
                var listExercise = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).Select(x => x ?? new Exercise()).ToList();
                var listQuestion = listExercise.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();
                foreach (var exerciseRequest in videoTimeCodeRequest.Exercises)
                {
                    if (exerciseRequest == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest));
                        return methodResult;
                    }
                    if (exerciseRequest.SkillId.HasValue)
                    {
                        var skillExists = await _skillRepository.AnyGuidAsync(exerciseRequest.SkillId.Value);
                        if (!skillExists)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exerciseRequest.SkillId), exerciseRequest.SkillId);
                            return methodResult;
                        }
                    }
                    Exercise? exercise;
                    if (exerciseRequest.Id.HasValue)
                    {
                        exercise = listExercise.FirstOrDefault(x => x.Id == exerciseRequest.Id.Value);
                        if (exercise == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise), exerciseRequest.Id);
                            return methodResult;
                        }
                        _mapper.Map(exerciseRequest, exercise);
                    }
                    else
                    {
                        exercise = _mapper.Map<Exercise>(exerciseRequest);
                    }
                    if (!exercise.IsValid())
                    {
                        methodResult.AddErrorBadRequest(exercise.ErrorMessages);
                        return methodResult;
                    }
                    var method = UpdateQuestionToExercise(exercise, exerciseRequest);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    if (exercise.Id == Guid.Empty)
                    {
                        videoTimeCode.TimeCodeExercises.Add(new TimeCodeExercise
                        {
                            VideoTimeCode = videoTimeCode,
                            Exercise = exercise
                        });
                    }
                }

                var questions = videoTimeCodeRequest.Exercises.SelectMany(x => x.Questions ?? new List<UpdateQuestionCommandModel>());
                var questionsToDelete = listQuestion.Where(x => x != null && x.Id != Guid.Empty && !questions.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
                if (questionsToDelete.Any())
                {
                    DeleteQuestions.AddRange(questionsToDelete);
                }

                var exercisesToDelete = listExercise.Where(x => x != null && x.Id != Guid.Empty && !videoTimeCodeRequest.Exercises.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
                if (exercisesToDelete.Any())
                {
                    DeleteExercises.AddRange(exercisesToDelete);
                }

                if (!videoTimeCode.IsValid())
                {
                    methodResult.AddErrorBadRequest(videoTimeCode.ErrorMessages);
                    return methodResult;
                }
                if (videoTimeCode.Id == Guid.Empty)
                {
                    video.VideoTimeCodes.Add(videoTimeCode);
                }
            }
            var videoTimeCodesToDelete = listTimeCode.Where(x => x != null && x.Id != Guid.Empty && !requestVideoTimeCodes.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
            if (videoTimeCodesToDelete.Any())
            {
                DeleteVideoTimeCodes.AddRange(videoTimeCodesToDelete);
            }

            if (!methodResult.IsOK)
            {
                return methodResult;
            }
            await DeleteDataVideoAsync();
            return methodResult;
        }

        public async Task<VoidMethodResult> UpdateTimeCodeToVideo(Video video, UpdateVideoCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.VideoTimeCodes == null || request.VideoTimeCodes.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodes));
                return methodResult;
            }

            var isExistName = await _videoRepository.Queryable.AnyAsync(x => x.Name == request.Name && x.Id != request.Id && (x.OriginalId != video.OriginalId && x.Id != video.OriginalId));
            if (isExistName)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNameIsExist), nameof(request.Name), request.Name);
                return methodResult;
            }
            var method = await UpdateTimeCodeToVideo(video, request.VideoTimeCodes);
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

        #endregion Update

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

        public async Task<VoidMethodResult> DeleteDataVideoAsync()
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            foreach (var item in DeleteVideoTimeCodes)
            {
                await _videoTimeCodeRepository.DeleteAsync(item);
            }
            await _videoTimeCodeRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            foreach (var item in DeleteExercises)
            {
                await _exerciseRepository.DeleteAsync(item);
            }
            await _exerciseRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            foreach (var item in DeleteQuestions)
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

        public async Task<(IList<VideoSkillScores> VideoSkillScores, int? TokenFirst, int? TokenLast)>
       GetSkillScoreAndTokens(VideoResult videoResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoResult);

            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable
                                                .Include(x => x.VideoTimeCode)
                                                .Where(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate)
                                                .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.UpdatedDate <= videoResult.UpdatedDate)
                                                .ToListAsync(cancellationToken);

            // Tính token cho Standalone TimeCode
            var tokenConfig = videoTimeCodeResults
                .Where(x => x.VideoTimeCode != null
                            && x.VideoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone
                            && x.VideoResultId == videoResult.Id)
                .GroupBy(x => x.VideoResultId)
                .Select(g => new
                {
                    TokenFirst = g.Where(x => x.TokenFirstTime.HasValue)
                                  .Sum(x => x.TokenFirstTime),
                    TokenLast = g.Where(x => x.TokenLastTime.HasValue)
                                 .Sum(x => x.TokenLastTime),
                })
                .FirstOrDefault();

            // Lấy tất cả SkillScores có dữ liệu
            var rawSkillScores = videoTimeCodeResults
                .Where(x => x.CorrectTotal > 0 && x.SkillScores != null && x.SkillScores.Any())
                .GroupBy(x => x.VideoTimeCode!.TimeCodeType)
                .SelectMany(g =>
                    g.SelectMany(v => v.SkillScores!)
                     .GroupBy(s => new
                     {
                         s.Skill,      // EnumCourseSkill (cũ)
                         s.SkillId,    // Guid? / int? (mới)
                         s.SkillName,  // string? (mới)
                         s.SkillFilePath,
                         TimeCodeType = g.Key
                     })
                     .Select(x => new
                     {
                         Type = x.Key.TimeCodeType,
                         Skill = x.Key.Skill,
                         SkillId = x.Key.SkillId,
                         SkillFilePath = x.Key.SkillFilePath,
                         SkillName = x.Key.SkillName,
                         CorrectCount = x.Sum(y => y.CorrectCount),
                         TotalCount = x.Sum(y => y.TotalCount),
                         CorrectQuestion = x.Sum(y => y.CountQuestion),
                         TotalQuestion = x.Sum(y => y.TotalQuestion),
                         CountQuestion = x.Sum(y => y.CountQuestion),
                         TokenReceived = x.Sum(y => y.TokenReceived)
                     }))
                .ToList();

            // Map sang VideoSkillScores, vẫn giữ cả CourseSkill + SkillId + SkillName
            var videoSkillScores = rawSkillScores
                .GroupBy(x => x.Type)
                .Select(g => new VideoSkillScores
                {
                    Type = g.Key,
                    SkillScores = g
                        .Where(x => x.TotalQuestion != 0)
                        .Select(x => new SkillScores
                        {
                            Skill = x.Skill,        // EnumCourseSkill (dùng hiện tại)
                            SkillId = x.SkillId,      // dùng dần về sau
                            SkillName = x.SkillName,    // dùng dần về sau
                            SkillFilePath = x.SkillFilePath,
                            TotalCount = x.TotalCount,
                            CorrectQuestion = x.CorrectQuestion,
                            CorrectCount = x.CorrectCount,
                            TotalQuestion = x.TotalQuestion,
                            CountQuestion = x.CountQuestion,
                            TokenReceived = x.TokenReceived,
                        })
                        .OrderBy(s => s.Skill)   // có thể thêm ThenBy(s => s.SkillName) nếu muốn
                        .ToList()
                })
                .ToList();

            return (videoSkillScores, tokenConfig?.TokenFirst, tokenConfig?.TokenLast);
        }

        public async Task<(IList<VideoSkillScores>, bool)> GetVideoSkillScores(VideoResult videoResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoResult);
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable
                    .Include(x => x.VideoTimeCode)
                    .Where(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate)
                    .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.UpdatedDate <= videoResult.UpdatedDate)
                    .ToListAsync(cancellationToken);

            var answers = videoTimeCodeResults.Where(x => x.CorrectTotal > 0 && x.SkillScores != null && x.SkillScores.Any())
                            .GroupBy(x => new { x.VideoTimeCode!.TimeCodeType })
                            .SelectMany(g => g.SelectMany(x => x.SkillScores!).GroupBy(x => new { x.Skill, g.Key.TimeCodeType }).Select(x => new
                            {
                                Type = x.Key.TimeCodeType,
                                Skill = x.Key.Skill,
                                CorrectCount = x.Sum(y => y.CorrectCount),
                                TotalAnswer = x.Sum(y => y.CountQuestion),
                                TokenReceived = x.Sum(x => x.TokenReceived)
                            })).ToList();
            var videoTimeCodes = await _videoTimeCodeRepository.ReadQueryable
                                    .Include(x => x.TimeCodeExercises)
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions)
                                    .ThenInclude(x => x.Question)
                                    .Where(x => x.VideoId == videoResult.VideoId)
                                    .ToListAsync(cancellationToken);
            var listGroupVideoTimeCode = videoTimeCodes.GroupBy(x => x.TimeCodeType)
                                        .Select(x =>
                                            x.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise)
                                            .GroupBy(x => x!.CourseSkill)
                                            .Select(y => new
                                            {
                                                Type = x.Key,
                                                Skill = y.Key,
                                                Questions = y.SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question)
                                                            .Where(x => !x!.Ungraded && x.QuestionType != EnumQuestionType.ExercisePreparation)
                                                            .Select(x => new
                                                            {
                                                                TotalCount = x!.CorrectTotal,
                                                                TotalQuestion = NumberOfQuestion
                                                            })
                                            }))
                                        .ToList();

            var listGroupQuestion = listGroupVideoTimeCode.SelectMany(x => x).Where(x => x.Questions.Any()).Select(x => new
            {
                Type = x.Type,
                Skill = x.Skill,
                TotalCount = x.Questions.Sum(x => x.TotalCount),
                TotalQuestion = x.Questions.Sum(x => x.TotalQuestion)
            });

            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var types = Enum.GetValues(typeof(EnumTimeCodeType)).Cast<EnumTimeCodeType>();
            var scoreQuery = from type in types
                             select new VideoSkillScores
                             {
                                 Type = type,
                                 SkillScores = (from skill in skills
                                                join questionTimeCodeQ in listGroupQuestion on skill equals questionTimeCodeQ.Skill into questionTimeCodeQ_jointable
                                                from questionTimeCodeQJ in questionTimeCodeQ_jointable.DefaultIfEmpty()
                                                join answerTimeCodeQ in answers.AsQueryable() on skill equals answerTimeCodeQ.Skill into answerTimeCodeQ_jointable
                                                from answerTimeCodeQJ in answerTimeCodeQ_jointable.DefaultIfEmpty()
                                                where questionTimeCodeQJ != null && questionTimeCodeQJ.Type == type && (!(answerTimeCodeQJ != null) || answerTimeCodeQJ.Type == type)
                                                select new SkillScores
                                                {
                                                    Skill = skill,
                                                    TotalCount = questionTimeCodeQJ.TotalCount,
                                                    CorrectCount = answerTimeCodeQJ != null ? answerTimeCodeQJ.CorrectCount : default,
                                                    TotalQuestion = questionTimeCodeQJ.TotalQuestion,
                                                    CountQuestion = answerTimeCodeQJ != null ? answerTimeCodeQJ.TotalAnswer : default,
                                                    TokenReceived = answerTimeCodeQJ != null ? answerTimeCodeQJ.TokenReceived : default,
                                                }).ToList()
                             };
            return (scoreQuery.ToList(), listGroupQuestion.Sum(x => x.TotalQuestion) != answers.Sum(x => x.TotalAnswer));
        }

        public async Task<int> GetHighestStreak(VideoResult videoResult)
        {
            ArgumentNullException.ThrowIfNull(videoResult);
            var questionIds = await (from baseQ in _videoTimeCodeRepository.Queryable
                                     join te in _timeCodeExerciseRepository.Queryable on baseQ.Id equals te.VideoTimeCodeId
                                     join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                     join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                     join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                     where baseQ.VideoId == videoResult.VideoId && baseQ.TimeCodeType == EnumTimeCodeType.Standalone
                                      && !q.Ungraded && q.QuestionType != EnumQuestionType.ExercisePreparation
                                     select q.Id).ToListAsync();

            var answerQuery = from baseQ in _questionRepository.Queryable.WhereBulkContains(questionIds, x => x.Id)
                              join vtca in _videoTimeCodeAnswerRepository.Queryable.Where(x => x.CreatedDate >= videoResult.CreatedDate && x.VideoResultId == videoResult.Id) on baseQ.Id equals vtca.QuestionId
                              where vtca.VideoResultId == videoResult.Id
                              orderby baseQ.CreatedDate
                              select vtca.IsCorrect == true && vtca.IsFirstSubmit;
            var highestStreaks = await answerQuery.ToListAsync();
            if (highestStreaks == null)
            {
                return default;
            }
            return highestStreaks.GetHighestStreak();
        }

        public async Task<int> GetHighestStreak(VideoTimeCodeResult videoTimeCodeResult)
        {
            var answers = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                                                .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                                                .Where(q => q.Question != null && !q.Question.Ungraded && q.Question.QuestionType != EnumQuestionType.ExercisePreparation)
                                                .OrderBy(x => x.Question!.CreatedDate)
                                                .Select(x => x.IsCorrect == true && x.IsFirstSubmit).ToListAsync();
            return answers.GetHighestStreak();
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

        public async Task<VideoTimeCodeModel> GetVideoTimeCodeAsync(VideoTimeCode? videoTimeCode, VideoTimeCodeResultModel? videoTimeCodeResult, bool isShowSubStatus = false)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            var isTimeCodeProcess = videoTimeCodeResult != null && videoTimeCodeResult.Status == EnumResultStatus.Process;
            var timeCode = GetVideoTimeCode(videoTimeCode, videoTimeCodeResult);
            foreach (var exercise in videoTimeCode.TimeCodeExercises.OrderBy(x => x!.CreatedDate).Select(n => n.Exercise))
            {
                var exerciseModel = await GetExercise(exercise, videoTimeCodeResult?.StudentId ?? default, videoTimeCodeResult?.Status ?? EnumResultStatus.New, isShowSubStatus, videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone);
                timeCode.Exercises.Add(exerciseModel);
            }
            return timeCode;
        }

        public async Task<VideoTimeCodeModel> GetVideoTimeCodeDetailAsync(VideoTimeCode videoTimeCode, VideoTimeCodeResultModel videoTimeCodeResult, bool isShowSubStatus = false)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);
            var listQuestionShuffle = new List<QuestionShuffle>();

            var isTimeCodeProcess = videoTimeCodeResult.Status == EnumResultStatus.Process;
            var timeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode);

            var exercises = await _timeCodeExerciseRepository.Queryable.Where(x => x.VideoTimeCodeId == videoTimeCode.Id)
                                                                       .OrderBy(x => x.CreatedDate)
                                                                       .Select(x => x.Exercise ?? new Exercise())
                                                                       .ToListAsync();

            var exerciseQuestions = await _exerciseQuestionRepository.Queryable.WhereBulkContains(exercises.Select(x => x.Id), x => x.ExerciseId).OrderBy(x => x.CreatedDate).Select(x => new
            {
                ExerciseId = x.ExerciseId,
                Question = x.Question ?? new Question()
            }).ToListAsync();
            var questions = exerciseQuestions.Select(x => x.Question).ToList();
            var questionIds = questions.Select(x => x!.Id).ToList();

            var questionExplanationErrors = await _questionExplanationErrorRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                                               .Where(x => x.Status == EnumProcessedStatus.NotProcessed && x.ObjectResultId == videoTimeCodeResult.VideoResultId)
                                                                                               .ToListAsync();

            var questionShuffles = await _questionShuffleRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                             .Where(x => x.StudentId == videoTimeCodeResult.StudentId)
                                                                             .ToListAsync();

            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                           .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                                                                           .Where(x => !(videoTimeCodeResult.Status == EnumResultStatus.Done && videoTimeCodeResult.UpdatedDate.HasValue) || x.CreatedDate <= videoTimeCodeResult.UpdatedDate)
                                                                           .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                                                                           .ToListAsync();

            timeCode.TotalCount = questionIds.Count;
            timeCode.Ungraded = questions.Any(x => x.Ungraded);
            timeCode.CorrectCount = videoTimeCodeResult.CorrectCount;
            timeCode.CorrectTotal = questions.Sum(x => x.CorrectTotal);
            timeCode.Status = videoTimeCodeResult.Status != EnumResultStatus.Done ? EnumResultStatus.Process : EnumResultStatus.Done;
            timeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode);
            timeCode.CourseSkills = exercises.Select(x => x.CourseSkill).Distinct().ToList();
            foreach (var exercise in exercises)
            {
                var exerciseModel = _mapper.Map<ExerciseModel>(exercise);
                var listQuestion = exerciseQuestions.Where(x => x.ExerciseId == exercise.Id).Select(x => x.Question).ToList();
                foreach (var question in listQuestion)
                {
                    if (question == null)
                    {
                        continue;
                    }

                    var videoTimeCodeAnswer = videoTimeCodeAnswers.FirstOrDefault(x => x.QuestionId == question.Id);
                    var isCheck = videoTimeCodeAnswer?.Status == EnumAnswerStatus.Done;
                    var questionModel = _mapper.Map<QuestionModel>(question);
                    if (!isCheck)
                    {
                        questionModel.Explanations = null;
                        questionModel.Explanation = null;
                    }
                    questionModel.CorrectStatus = GetCorrectStatus(videoTimeCodeAnswer);
                    questionModel.IsReportExplanation = questionExplanationErrors.Any(x => x.QuestionId == question.Id);
                    questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isCheck)).Item1;

                    var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);
                    (questionModel.Config, string? questionShuffleStr) = _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, isCheck, questionShuffle?.ShuffleConfigs);
                    if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                    {
                        if (questionShuffle == null)
                        {
                            questionShuffle = new QuestionShuffle
                            {
                                QuestionId = question.Id,
                                StudentId = videoTimeCodeResult.StudentId,
                                ShuffleConfigStr = questionShuffleStr
                            };
                        }
                        else
                        {
                            questionShuffle.ShuffleConfigStr = questionShuffleStr;
                        }
                        listQuestionShuffle.Add(questionShuffle);
                    }

                    if (videoTimeCodeAnswer != null)
                    {
                        videoTimeCodeAnswer.CorrectCount = isCheck ? videoTimeCodeAnswer.CorrectCount : default;
                        videoTimeCodeAnswer.Answer = _answerTypeConverter.AnswerTypeConverterObject(videoTimeCodeAnswer.Answer, question.QuestionType, isShowSubStatus, videoTimeCodeResult.Status, videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone);
                        questionModel.ResultAnswer = _mapper.Map<AnswerModel>(videoTimeCodeAnswer);
                    }
                    exerciseModel.Questions.Add(questionModel);
                }

                await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
                timeCode.Exercises.Add(exerciseModel);
            }
            return timeCode;
        }

        private async Task<ExerciseModel> GetExercise(Exercise? exercise, Guid studentId, EnumResultStatus status, bool isShowSubStatus, bool isDisableAnswer)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            var listQuestionShuffle = new List<QuestionShuffle>();
            var exerciseModel = _mapper.Map<ExerciseModel>(exercise);
            var questions = exercise.ExerciseQuestions.OrderBy(x => x!.CreatedDate).Where(x => x.Question != null).Select(x => x.Question).ToList();
            var questionShuffles = await _questionShuffleRepository.Queryable.Where(x => questions.Select(x => x!.Id).Contains(x.QuestionId) && x.StudentId == studentId).ToListAsync();

            foreach (var question in questions)
            {
                if (question == null)
                {
                    continue;
                }

                var videoTimeCodeAnswer = question.VideoTimeCodeAnswers.FirstOrDefault();
                var isCheck = videoTimeCodeAnswer?.Status == EnumAnswerStatus.Done;
                var questionModel = _mapper.Map<QuestionModel>(question);
                if (!isCheck)
                {
                    questionModel.Explanations = null;
                    questionModel.Explanation = null;
                }
                questionModel.CorrectStatus = GetCorrectStatus(videoTimeCodeAnswer);
                questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isCheck)).Item1;

                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);
                (questionModel.Config, string? questionShuffleStr) = _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, isCheck, questionShuffle?.ShuffleConfigs);
                if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                {
                    if (questionShuffle == null)
                    {
                        questionShuffle = new QuestionShuffle
                        {
                            QuestionId = question.Id,
                            StudentId = studentId,
                            ShuffleConfigStr = questionShuffleStr
                        };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }
                    listQuestionShuffle.Add(questionShuffle);
                }

                if (videoTimeCodeAnswer != null)
                {
                    videoTimeCodeAnswer.CorrectCount = isCheck ? videoTimeCodeAnswer.CorrectCount : default;
                    videoTimeCodeAnswer.Answer = _answerTypeConverter.AnswerTypeConverterObject(videoTimeCodeAnswer.Answer, question.QuestionType, isShowSubStatus, status, isDisableAnswer);
                    questionModel.ResultAnswer = _mapper.Map<AnswerModel>(videoTimeCodeAnswer);
                }
                exerciseModel.Questions.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            return exerciseModel;
        }

        public VideoTimeCodeModel GetVideoTimeCode(VideoTimeCode? videoTimeCode, VideoTimeCodeResultModel? videoTimeCodeResult)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            var timeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode);
            timeCode.Ungraded = GetUngraded(videoTimeCode);
            timeCode.CorrectCount = GetCorrectCount(videoTimeCode);
            timeCode.CorrectTotal = GetCorrectTotal(videoTimeCode);
            timeCode.Status = GetTimeCodeStatus(videoTimeCode);
            timeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode);
            return timeCode;
        }

        public async Task<IList<VideoTimeCodeModel>> GetTimeCodes(Video? video, VideoResult videoResult, bool isShowExercise = false)
        {
            ArgumentNullException.ThrowIfNull(video);
            ArgumentNullException.ThrowIfNull(videoResult);
            var videoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).ToList();
            var videoTimeCodeModels = new List<VideoTimeCodeModel>();
            var indexProcess = GetIndexProcess(videoTimeCodes, videoResult.CurrentVideoTimeCodeId);
            foreach (var item in videoTimeCodes)
            {
                var indexTimeCode = videoTimeCodes.IndexOf(item);
                var videoTimeCodeResult = item.VideoTimeCodeResults.FirstOrDefault();
                var videoTimeCodeResultModel = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult);
                var videoTimeCode = isShowExercise ? await GetVideoTimeCodeAsync(item, videoTimeCodeResultModel, false) : GetVideoTimeCode(item, videoTimeCodeResultModel);
                videoTimeCode.Status = GetTimeCodeStatus(indexProcess, indexTimeCode, videoTimeCodeResult);
                videoTimeCodeModels.Add(videoTimeCode);
            }
            return videoTimeCodeModels;
        }

        public async Task<IList<VideoTimeCodeModel>> GetTimeCodes(Video? video, VideoResult videoResult)
        {
            ArgumentNullException.ThrowIfNull(video);
            ArgumentNullException.ThrowIfNull(videoResult);
            var videoTimeCodeModels = new List<VideoTimeCodeModel>();

            var videoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).ToList();
            var indexProcess = GetIndexProcess(videoTimeCodes, videoResult.CurrentVideoTimeCodeId);

            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate)
                                                                           .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.UpdatedDate <= videoResult.UpdatedDate)
                                                                           .ToListAsync();

            var queryData = await (from baseQ in _videoTimeCodeRepository.ReadQueryable
                                   join te in _timeCodeExerciseRepository.ReadQueryable on baseQ.Id equals te.VideoTimeCodeId
                                   join eq in _exerciseQuestionRepository.ReadQueryable on te.ExerciseId equals eq.ExerciseId
                                   where baseQ.VideoId == video.Id
                                   select new
                                   {
                                       baseQ.Id,
                                       CourseSkill = te.Exercise.CourseSkill,
                                       Question = eq.Question,
                                   }).ToListAsync();

            foreach (var item in videoTimeCodes)
            {
                var indexTimeCode = videoTimeCodes.IndexOf(item);
                var videoTimeCodeResult = videoTimeCodeResults.FirstOrDefault(x => x.VideoTimeCodeId == item.Id);
                var courseSkills = queryData.Where(x => x.Id == item.Id).Select(x => x.CourseSkill).Distinct().ToList();
                var questions = queryData.Where(x => x.Id == item.Id).Select(x => x.Question).ToList();

                var videoTimeCodeResultModel = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult);
                if (videoTimeCodeResultModel != null)
                {
                    videoTimeCodeResultModel.CurrentVideoTimeCodeId = videoResult.CurrentVideoTimeCodeId;
                }
                var videoTimeCode = _mapper.Map<VideoTimeCodeModel>(item);
                videoTimeCode.TotalCount = questions.Count;
                videoTimeCode.Ungraded = questions.Any(x => x.Ungraded);
                videoTimeCode.CorrectCount = videoTimeCodeResult?.CorrectCount ?? default;
                videoTimeCode.CorrectTotal = questions.Sum(x => x.CorrectTotal);
                videoTimeCode.VideoTimeCodeResult = GetVideoTimeCodeResult(videoTimeCodeResultModel, item);
                videoTimeCode.CourseSkills = courseSkills;
                videoTimeCode.Status = GetTimeCodeStatus(indexProcess, indexTimeCode, videoTimeCodeResult);
                videoTimeCodeModels.Add(videoTimeCode);
            }
            return videoTimeCodeModels;
        }

        private VideoTimeCodeResultModel? GetVideoTimeCodeResult(VideoTimeCodeResultModel? videoTimeCodeResult, VideoTimeCode videoTimeCode)
        {
            if (videoTimeCodeResult != null)
            {
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
            }
            return videoTimeCodeResult;
        }

        private static EnumCorrectStatus? GetCorrectStatus(VideoTimeCodeAnswer? videoTimeCodeAnswer)
        {
            EnumCorrectStatus? status = null;
            if (videoTimeCodeAnswer != null && videoTimeCodeAnswer.IsCorrect.HasValue)
            {
                status = EnumCorrectStatus.Process;
                if (videoTimeCodeAnswer.Status == EnumAnswerStatus.Done)
                {
                    status = videoTimeCodeAnswer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail;
                }
            }

            return status;
        }

        private static EnumResultStatus GetTimeCodeStatus(VideoTimeCode? videoTimeCode)
        {
            var status = EnumResultStatus.Process;
            if (videoTimeCode != null && videoTimeCode.VideoTimeCodeAnswers.Any() && videoTimeCode.VideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Done))
            {
                status = EnumResultStatus.Done;
            }
            return status;
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

        private static int? GetIndexProcess(IList<VideoTimeCode>? videoTimeCodes, Guid? currentVideoTimeCodeId)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCodes);
            var videoTimeCode = videoTimeCodes.FirstOrDefault(x => x.Id == currentVideoTimeCodeId);
            return videoTimeCode != null ? videoTimeCodes.IndexOf(videoTimeCode) : null;
        }

        private async Task<(List<Question>?, IList<VideoTimeCodeAnswer>?)> GetUnansweredQuestionIds(VideoTimeCodeResult videoTimeCodeResult)
        {
            var questions = await (from baseQ in _videoTimeCodeRepository.Queryable
                                   join te in _timeCodeExerciseRepository.Queryable on baseQ.Id equals te.VideoTimeCodeId
                                   join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                   join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                   join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                   where baseQ.Id == videoTimeCodeResult.VideoTimeCodeId
                                   select q).ToListAsync();
            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.Include(x => x.Question)
                                                                           .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                                                                           .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id).ToListAsync();

            var questionCompleteIds = videoTimeCodeAnswers.Select(x => x.QuestionId).ToList();
            var unansweredQuestionIds = questions.Select(x => x!.Id).Except(questionCompleteIds).ToList();
            return (questions.Where(x => unansweredQuestionIds.Contains(x.Id)).ToList(), videoTimeCodeAnswers.Where(x => x.Status != EnumAnswerStatus.Done).ToList());
        }

        public async Task<long> UpdateVideoAnswers(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, bool isDone = false, bool isSubmit = true)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCode);
            var (questions, updateVideoTimeCodeAnswers) = await GetUnansweredQuestionIds(videoTimeCodeResult);
            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            if (questions != null && questions.Any())
            {
                var exerciseQuestions = await _exerciseQuestionRepository.Queryable.WhereBulkContains(questions.Select(x => x.Id), x => x.QuestionId).ToListAsync();

                videoTimeCodeAnswers = questions.Select(question =>
                {
                    Guid exerciseId = exerciseQuestions.FirstOrDefault(x => x.QuestionId == question.Id)?.ExerciseId ?? default;
                    return new VideoTimeCodeAnswer
                    {
                        Answer = _answerTypeConverter.GetConfigEmpty(question.QuestionType),
                        QuestionId = question.Id,
                        VideoResultId = videoTimeCodeResult.VideoResultId,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoTimeCodeId = videoTimeCodeResult.VideoTimeCodeId,
                        ExerciseId = exerciseId,
                        Status = isDone ? EnumAnswerStatus.Done : EnumAnswerStatus.Process,
                        IsCorrect = null
                    };
                }).ToList();

                await _videoTimeCodeAnswerRepository.BulkMergeAsync(videoTimeCodeAnswers, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.VideoResultId, c.VideoTimeCodeResultId, c.VideoTimeCodeId, c.QuestionId, c.IsDeleted };
                });
            }
            if (updateVideoTimeCodeAnswers != null && updateVideoTimeCodeAnswers.Any())
            {
                isDone = !(questions != null && questions.Any()) && (isDone || updateVideoTimeCodeAnswers.All(x => x.Status == EnumAnswerStatus.Done));
                updateVideoTimeCodeAnswers.ForEach(x =>
                {
                    var status = GetAnswerStatus(videoTimeCode.TimeCodeType, isSubmit, x.CorrectCount, x.Question!.CorrectTotal);
                    x.Status = isDone ? EnumAnswerStatus.Done : status;
                    x.IsCorrect = x.IsCorrect.HasValue ? x.CorrectCount == x.Question!.CorrectTotal : null;
                });
                await _videoTimeCodeAnswerRepository.BulkUpdateList(updateVideoTimeCodeAnswers, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeResultId, entity.QuestionId };
                });
            }
            return updateVideoTimeCodeAnswers?.Where(x => x.Question != null && !x.Question.Ungraded && x.Question.QuestionType != EnumQuestionType.ExercisePreparation)?.Where(x => x.Status == EnumAnswerStatus.Done).Sum(x => x.CorrectCount) ?? default;
        }

        private static EnumAnswerStatus GetAnswerStatus(EnumTimeCodeType? timeCodeType, bool isSubmit, int correctCount, int correctTotal)
        {
            if (timeCodeType == EnumTimeCodeType.Standalone)
            {
                return correctCount == correctTotal && isSubmit ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
            }
            return EnumAnswerStatus.Done;
        }
    }
}
