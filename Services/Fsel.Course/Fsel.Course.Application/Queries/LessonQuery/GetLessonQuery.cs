// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.LessonQuery
{
    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;

        public GetLessonQueryHandler(IMapper mapper, ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            ArgumentNullException.ThrowIfNull(request);
            var lesson = await _lessonRepository.GetIncludeByIdAsync(request.Id);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            var video = lesson.LessonVideos.Select(x => x.Video).FirstOrDefault();

            var lessonModel = _mapper.Map<LessonModel>(lesson);
            lessonModel.Video = _mapper.Map<VideoModel>(video);
            lessonModel.VideoId = video?.Id;
            lessonModel.HomeWorks = _mapper.Map<IList<HomeWorkModel>>(lesson.LessonHomeWorks.Select(x => x.HomeWork));
            lessonModel.ClassForum = _mapper.Map<ClassForumModel>(lesson.ClassForum);
            lessonModel.IsActive = lesson.UnitLessons.Any();
            methodResult.Result = lessonModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
