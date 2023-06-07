// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractices;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters;

    public class ExtraPracticeConverter
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;

        public ExtraPracticeConverter(IExtraPracticeRepository extraPracticeRepository, IMapper mapper, VideoConverter videoConverter)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
        }

        public VoidMethodResult AddExtraPracticeExercise(dynamic extraPracticeExercises, IList<CreateExtraPracticeChapterCommand>? exercisePracticeChapters)
        {
            ArgumentNullException.ThrowIfNull(exercisePracticeChapters);
            VoidMethodResult methodResult = new VoidMethodResult();
            foreach (var item in exercisePracticeChapters)
            {
                ExtraPracticeChapter extraPracticeChapter = _mapper.Map<ExtraPracticeChapter>(item);
                if (item == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExtraPractiveChapterErrorCode.ExtraPracticeChapterNull));
                    return methodResult;
                }
                if (item.Exercises != null && item.Exercises.Count > 0)
                {
                    foreach (var exercise in item.Exercises)
                    {
                        if (exercise == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                            return methodResult;
                        }
                        Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                        var method = _videoConverter.ExerciseValuedate(excerciseNew, exercise);
                        if (!method.IsOK)
                        {
                            methodResult.AddError(method.ErrorMessages);
                            return methodResult;
                        }
                        extraPracticeExercises.Add(new ExtraPracticeExercise { Exercise = excerciseNew, ExtraPracticeChapter = extraPracticeChapter });
                    }
                }
            }
            return methodResult;
        }

        public VoidMethodResult AddExerciseToExtraPractice(dynamic extraPracticeExercises, IList<CreateExerciseCommandModel>? exercises)
        {
            ArgumentNullException.ThrowIfNull(exercises);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (exercises == null || exercises.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull));
                return methodResult;
            }
            foreach (var exercise in exercises)
            {
                if (exercise == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                    return methodResult;
                }
                Exercise excerciseNew = _mapper.Map<Exercise>(exercise);
                var method = _videoConverter.ExerciseValuedate(excerciseNew, exercise);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
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

            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            if (request.Type == EnumExtraPracticeType.Book && request.ExtraPracticeChapters != null && request.ExtraPracticeChapters.Count > 0)
            {
                var method = AddExtraPracticeExercise(extraPracticeExercises, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                var method = AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                if (request.Video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNull));
                    return methodResult;
                }
                Video video = _mapper.Map<Video>(request.Video);
                var method = await _videoConverter.CreateTimeCodeToVideo(video, request.Video);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }

            return methodResult;
        }
        public async Task<VoidMethodResult> UpdateExtraPractice(dynamic extraPractice, UpdateExtraPracticeCommandModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            List<ExtraPracticeExercise> extraPracticeExercises = new List<ExtraPracticeExercise>();
            if (request.Type == EnumExtraPracticeType.Book && request.ExtraPracticeChapters != null && request.ExtraPracticeChapters.Count > 0)
            {
                var method = AddExtraPracticeExercise(extraPracticeExercises, request.ExtraPracticeChapters);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.VideoEmbed || request.Type == EnumExtraPracticeType.Exercise)
            {
                var method = AddExerciseToExtraPractice(extraPracticeExercises, request.Exercises);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
            }
            else if (request.Type == EnumExtraPracticeType.InteractiveVideo)
            {
                if (request.Video == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNull));
                    return methodResult;
                }
                Video video = _mapper.Map<Video>(request.Video);
                var method = await _videoConverter.CreateTimeCodeToVideo(video, request.Video);
                if (!method.IsOK)
                {
                    methodResult.AddError(method.ErrorMessages);
                    return methodResult;
                }
                extraPractice.Video = video;
            }
            if (extraPracticeExercises.Count > 0)
            {
                extraPractice.ExtraPracticeExercises = extraPracticeExercises;
            }

            return methodResult;
        }
    }
}
