// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetVideoQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;

        public GetVideoQueryHandler(IVideoRepository videoRepository
            )
        {
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();
            ArgumentNullException.ThrowIfNull(request);
            var query = from i in _videoRepository.Queryable
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                .ThenInclude(x => x.Exercise)
                                .ThenInclude(x => x.ExerciseQuestions.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x.Question)
                                .Where(x => x.Id == request.Id)
                        select new VideoModel
                        {
                            Id = i.Id,
                            Name = i.Name,
                            VideoFilePath = i.VideoFilePath,
                            IsActive = i.LessonVideos.Any(),
                            TeacherId = i.TeacherId,
                            CourseLevel = i.CourseLevel,
                            VideoTimeCodes = i.VideoTimeCodes.Select(x => new VideoTimeCodeModel
                            {
                                Id = x.Id,
                                DisplayTime = x.DisplayTime,
                                ExecutionTime = x.ExecutionTime,
                                TimeCodeType = x.TimeCodeType,
                                VideoId = x.VideoId,
                                Exercises = x.TimeCodeExercises.Select(n => n.Exercise).Select(n => new ExerciseModel
                                {
                                    Id = n.Id,
                                    MediaPost = n.MediaPost,
                                    CourseSkill = n.CourseSkill,
                                    Questions = n.ExerciseQuestions.Select(m => m.Question).Select(m => new QuestionModel()
                                    {
                                        Id = m.Id,
                                        QuestionType = m.QuestionType,
                                        CorrectTotal = m.CorrectTotal,
                                        IsSave = m.IsSave,
                                        Config = m.Config
                                    }).ToList()
                                }).ToList(),
                            }).ToList(),
                        };

            var video = query.FirstOrDefault();
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotExist),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = video;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
