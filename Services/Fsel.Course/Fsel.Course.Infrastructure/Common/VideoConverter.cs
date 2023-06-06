// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.Videos;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
    using Microsoft.EntityFrameworkCore;

    public class VideoConverter
    {
        private readonly IVideoRepository _videoRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;

        public VideoConverter(IVideoRepository videoRepository
            , QuestionTypeConverter questionTypeConverter
            , IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
        }

        public VoidMethodResult ExerciseValuedate(dynamic newExercise, CreateExerciseCommandModel? exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);
            VoidMethodResult methodResult = new VoidMethodResult();

            if (exercise.Questions == null || exercise.Questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull));
                return methodResult;
            }
            foreach (var question in exercise.Questions)
            {
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull));
                    return methodResult;
                }
                var newQuestion = _mapper.Map<Question>(question);
                newExercise.ExerciseQuestions.Add(new ExerciseQuestion
                {
                    Exercise = newExercise,
                    Question = newQuestion
                });
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question!.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
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
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodeNull));
                    return methodResult;
                }
                var newTimeCode = _mapper.Map<VideoTimeCode>(timeCode);
                if (timeCode.Exercises == null || timeCode.Exercises.Count == 0)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExercisesNull));
                    return methodResult;
                }
                foreach (var exercise in timeCode.Exercises)
                {
                    if (exercise == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumExerciseErrorCode.ExerciseNull));
                        return methodResult;
                    }
                    var newExercise = _mapper.Map<Exercise>(exercise);
                    var method = ExerciseValuedate(newExercise, exercise);
                    if (!method.IsOK)
                    {
                        methodResult.AddError(method.ErrorMessages);
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
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodesNull));
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
                methodResult.AddError(method.ErrorMessages);
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
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodesNull));
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
                methodResult.AddError(method.ErrorMessages);
                return methodResult;
            }
            if (!video.IsValid())
            {
                methodResult.AddErrorBadRequest(video.ErrorMessages);
                return methodResult;
            }
            return methodResult;
        }
    }
}
