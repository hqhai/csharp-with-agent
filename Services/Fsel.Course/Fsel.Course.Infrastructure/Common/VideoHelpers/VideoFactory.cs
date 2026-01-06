// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.VideoHelpers
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.Videos;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodes;
    using Fsel.Shared.Enums;

    public class VideoFactory
    {
        private readonly UpdateVideoCommandModel _request;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;

        protected VideoFactory(UpdateVideoCommandModel request,
            IMapper mapper,
            QuestionConverter questionConverter)
        {
            _request = request;
            _mapper = mapper;
            _questionConverter = questionConverter;
        }

        public Video Build(int version = 0, Guid? originalId = null)
        {
            var video = _mapper.Map<Video>(_request);
            video.VersionStatus = EnumVersionStatus.LastVersion;
            video.Version = version;
            video.VersionType = EnumVersion.V2;
            video.OriginalId = originalId ?? video.Id;
            AddTimeCodes(video, _request.VideoTimeCodes);
            return video;
        }

        public void AddTimeCodes(Video video, IList<UpdateVideoTimeCodeCommandModel>? timeCodeRequests)
        {
            if (timeCodeRequests == null || timeCodeRequests.Count == 0)
            {
                return;
            }

            foreach (var timeCodeRequest in timeCodeRequests)
            {
                var videoTimeCode = _mapper.Map<VideoTimeCode>(timeCodeRequest);
                videoTimeCode.Id = Guid.Empty;

                BuildExercises(videoTimeCode, timeCodeRequest.Exercises);
                video.VideoTimeCodes.Add(videoTimeCode);
            }
        }

        private void BuildExercises(VideoTimeCode timeCode, IList<UpdateExerciseCommandModel>? exerciseRequests)
        {
            if (exerciseRequests == null || exerciseRequests.Count == 0)
            {
                return;
            }
            foreach (var exerciseRequest in exerciseRequests)
            {
                var exercise = _mapper.Map<Exercise>(exerciseRequest);
                exercise.Id = Guid.Empty;

                BuildQuestions(exercise, exerciseRequest);

                timeCode.TimeCodeExercises.Add(new TimeCodeExercise
                {
                    VideoTimeCode = timeCode,
                    Exercise = exercise
                });
            }
        }

        private void BuildQuestions(Exercise exercise, UpdateExerciseCommandModel request)
        {
            if (request.Questions == null || request.Questions.Count == 0)
            {
                return;
            }
            foreach (var questionRequest in request.Questions)
            {
                var question = _mapper.Map<Question>(questionRequest);
                question.Id = Guid.Empty;

                var questionResult = _questionConverter.HandleQuestion(question);

                exercise.ExerciseQuestions.Add(new ExerciseQuestion
                {
                    Question = questionResult.Result
                });
            }
        }

        public VoidMethodResult ValidateQuestions(UpdateVideoCommandModel request)
        {
            var methodResult = new VoidMethodResult();
            var questions = request?.VideoTimeCodes?.SelectMany(x => x.Exercises ?? new List<UpdateExerciseCommandModel>())
                                   .SelectMany(x => x.Questions ?? new List<UpdateQuestionCommandModel>())
                                   .ToList();
            if (questions == null || !questions.Any())
            {
                return methodResult;
            }

            foreach (var question in questions)
            {
                var questionResult = _questionConverter.HandleQuestion(_mapper.Map<Question>(question));
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
            }
            return methodResult;
        }

        public static VideoFactory Create(UpdateVideoCommandModel request, IMapper mapper, QuestionConverter questionConverter)
        {
            return new VideoFactory(request, mapper, questionConverter);
        }
    }
}
