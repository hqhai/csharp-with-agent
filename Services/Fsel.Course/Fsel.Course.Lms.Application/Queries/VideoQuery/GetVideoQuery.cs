// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoId { get; set; }
    }

    public class GetVideoStandaloneQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;

        public GetVideoStandaloneQueryHandler(IVideoRepository videoRepository,
            QuestionTypeConverter questionTypeConverter,
            IMapper mapper)
        {
            _videoRepository = videoRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var video = await _videoRepository.Queryable
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => (x ?? new()).ExerciseQuestions.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.Question)
                                .ThenInclude(x => (x ?? new()).VideoTimeCodeAnswer)
                                .Where(x => x.Id == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotExist), nameof(request.VideoId), request?.VideoId);
                return methodResult;
            }

            var videoModel = new VideoModel
            {
                Id = video.Id,
                Name = video.Name,
                VideoFilePath = video.VideoFilePath,
                IsActive = video.LessonVideos.Any(),
                TeacherId = video.TeacherId,
                CourseLevel = video.CourseLevel,
                VideoTimeCodes = video.VideoTimeCodes.Select(x => new VideoTimeCodeModel
                {
                    Id = x.Id,
                    DisplayTime = x.DisplayTime,
                    ExecutionTime = x.ExecutionTime,
                    TimeCodeType = x.TimeCodeType,
                    VideoId = x.VideoId,
                    Exercises = x.TimeCodeExercises.Where(n => n.Exercise != null).Select(n => n.Exercise ?? new()).Select(n => new ExerciseModel
                    {
                        Id = n.Id,
                        MediaPost = n.MediaPost,
                        CourseSkill = n.CourseSkill,
                        Questions = n.ExerciseQuestions.Where(m => m.Question != null).Select(m => m.Question ?? new()).Select(m => new QuestionModel()
                        {
                            Id = m.Id,
                            QuestionType = m.QuestionType,
                            CorrectTotal = m.CorrectTotal,
                            Explanation = m.Explanation,
                            Config = _questionTypeConverter.QuestionTypeConverterObject(m.QuestionType, m.Config),
                            VideoTimeCodeAnswer = _mapper.Map<VideoTimeCodeAnswerModel>(m.VideoTimeCodeAnswer)
                        }).ToList()
                    }).ToList(),
                }).ToList(),
            };

            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
