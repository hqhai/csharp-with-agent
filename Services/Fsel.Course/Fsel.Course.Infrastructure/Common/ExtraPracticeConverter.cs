// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters;
    using Microsoft.EntityFrameworkCore;

    public class ExtraPracticeConverter
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly ISkillRepository _skillRepository;

        public ExtraPracticeConverter(IExtraPracticeRepository extraPracticeRepository, IMapper mapper
            , VideoConverter videoConverter
            , IMockTestRepository mockTestRepository
            , IVideoRepository videoRepository
            , IExtraPracticeExerciseRepository extraPracticeExerciseRepository
            , IExerciseRepository exerciseRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , IQuestionRepository questionRepository
            , QuestionTypeConverter questionTypeConverter
            , ISkillRepository skillRepository
            )
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _mockTestRepository = mockTestRepository;
            _videoRepository = videoRepository;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _questionRepository = questionRepository;
            _questionTypeConverter = questionTypeConverter;
            _skillRepository = skillRepository;
        }

        public VoidMethodResult AddExtraPracticeChapterExercise(dynamic extraPracticeChapters, IList<CreateExtraPracticeChapterCommandModel>? exercisePracticeChapters)
        {
            ArgumentNullException.ThrowIfNull(exercisePracticeChapters);
            VoidMethodResult methodResult = new VoidMethodResult();
            foreach (var item in exercisePracticeChapters)
            {
                ExtraPracticeChapter extraPracticeChapter = _mapper.Map<ExtraPracticeChapter>(item);
                if (item == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPracticeChapter));
                    return methodResult;
                }
                if (item.Exercises != null && item.Exercises.Count > 0)
                {
                    foreach (var exercise in item.Exercises)
                    {
                        if (exercise == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise));
                            return methodResult;
                        }
                        Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                        var method = _videoConverter.AddQuestionToExercise(excerciseNew, exercise);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                        extraPracticeChapter.ExtraPracticeExercises.Add(new ExtraPracticeExercise { Exercise = excerciseNew });
                    }
                    extraPracticeChapters.Add(extraPracticeChapter);
                }
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> AddExerciseToExtraPractice(dynamic extraPracticeExercises, IList<CreateExerciseCommandModel>? exercises)
        {
            ArgumentNullException.ThrowIfNull(exercises);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (exercises == null || exercises.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercises));
                return methodResult;
            }
            foreach (var exercise in exercises)
            {
                if (exercise == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise));
                    return methodResult;
                }
                if (exercise.SkillId.HasValue)
                {
                    var skillExists = await _skillRepository.AnyGuidAsync(exercise.SkillId.Value);
                    if (!skillExists)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(exercise.SkillId), exercise.SkillId);
                        return methodResult;
                    }
                }

                Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                var method = _videoConverter.AddQuestionToExercise(excerciseNew, exercise);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                extraPracticeExercises.Add(new ExtraPracticeExercise { Exercise = excerciseNew });
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> CreateExtraPractice(dynamic extraPractice, CreateExtraPracticeCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (await _extraPracticeRepository.Queryable.AnyAsync(x => x.Code == request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            List<ExtraPracticeChapter> extraPracticeChapters = new List<ExtraPracticeChapter>();
            if (request.Type == EnumExtraPracticeType.Book)
            {
                if (request.ExtraPracticeChapters == null || request.ExtraPracticeChapters.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ExtraPracticeChapters));
                    return methodResult;
                }
                var method = AddExtraPracticeChapterExercise(extraPracticeChapters, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                if (request.Exercises == null || request.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Exercises));
                    return methodResult;
                }
                var method = await AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                if (request.Video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Video));
                    return methodResult;
                }
                Video video = _mapper.Map<Video>(request.Video);
                var method = await _videoConverter.CreateTimeCodeToVideo(video, request.Video);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            else if (request.Type == EnumExtraPracticeType.MockTest)
            {
                if (request.MockTestId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.MockTestId));
                    return methodResult;
                }
                var isCheck = await _mockTestRepository.Queryable.AnyAsync(x => x.Id == request.MockTestId) && !(await _extraPracticeRepository.Queryable.AnyAsync(x => x.MockTestId == request.MockTestId));
                if (isCheck)
                {
                    extraPractice.MockTestId = request.MockTestId;
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isCheck));
                    return methodResult;
                }
            }
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }
            else if (extraPracticeChapters.Count > 0)
            {
                extraPractice.ExtraPracticeChapters = extraPracticeChapters;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> UpdateExtraPractice(dynamic extraPractice, UpdateExtraPracticeCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (await _extraPracticeRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.Code == request.Code))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Code), request.Code);
                return methodResult;
            }
            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            List<ExtraPracticeChapter> extraPracticeChapters = new List<ExtraPracticeChapter>();
            if (request.Type == EnumExtraPracticeType.Book && request.ExtraPracticeChapters != null && request.ExtraPracticeChapters.Count > 0)
            {
                var method = AddExtraPracticeChapterExercise(extraPracticeChapters, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                var method = await AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                var video = await _videoRepository.GetIncludeByIdAsync(request.VideoId ?? Guid.Empty);
                if (request.Video == null || request.VideoId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Video));
                    return methodResult;
                }
                if (video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                    return methodResult;
                }
                request.Video.Id = video.Id;
                _mapper.Map(request.Video, video);
                var methodUpdate = await _videoConverter.UpdateTimeCodeToVideo(video, request.Video);
                if (!methodUpdate.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodUpdate.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            else if (request.Type == EnumExtraPracticeType.MockTest)
            {
                if (request.MockTestId == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.MockTestId));
                    return methodResult;
                }
                if (await _mockTestRepository.Queryable.AnyAsync(x => x.Id == request.MockTestId))
                {
                    extraPractice.MockTestId = request.MockTestId;
                }
            }
            _mapper.Map(request, extraPractice);
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises.Clear();
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }
            else if (extraPracticeChapters.Count > 0)
            {
                extraPractice.ExtraPracticeChapters = extraPracticeChapters;
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> DeleteExtraPractice(ExtraPractice? extraPractice, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            VoidMethodResult methodResult = new VoidMethodResult();
            IList<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            IList<Exercise> exercises = new List<Exercise>();
            IList<Question> questions = new List<Question>();
            if (extraPractice.ExtraPracticeExercises != null && extraPractice.ExtraPracticeExercises.Count > 0)
            {
                extraPracticeExercises = extraPractice.ExtraPracticeExercises.ToList();
                extraPractice.ExtraPracticeExercises.Clear();
            }
            else if (extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).ToList().Count > 0)
            {
                extraPracticeExercises = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).ToList();
                extraPractice.ExtraPracticeChapters.Clear();
            }
            else if (extraPractice.Video != null)
            {
                exercises = extraPractice.Video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise ?? new Exercise()).ToList();
                questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();
            }
            if (extraPracticeExercises != null && extraPracticeExercises.Count > 0)
            {
                exercises = extraPracticeExercises.Select(x => x.Exercise ?? new Exercise()).ToList();
                questions = exercises.SelectMany(x => x.ExerciseQuestions).Select(x => x.Question ?? new Question()).ToList();

                foreach (var item in extraPracticeExercises)
                {
                    await _extraPracticeExerciseRepository.DeleteAsync(item);
                }
                await _extraPracticeExerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            if (exercises != null)
            {
                foreach (var item in exercises)
                {
                    await _exerciseRepository.DeleteAsync(item);
                }
                await _exerciseRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var item in questions)
                {
                    await _questionRepository.DeleteAsync(item);
                }
                await _questionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return methodResult;
        }
    }
}
