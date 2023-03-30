// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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
            var lesson = await _lessonRepository.Queryable
                            .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                            .ThenInclude(x => x.Video)
                            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumLessonErrorCode.LessonNotExist),
                    nameof(request.Id), request.Id);
                return methodResult;
            }

            var lessonModel = _mapper.Map<LessonModel>(lesson);
            lessonModel.Video = _mapper.Map<VideoModel>(lesson.LessonVideos.Select(x => x.Video).Where(x => x != null && !x.IsDeleted).FirstOrDefault());
            lessonModel.IsActive = lesson.UnitLessons.Any();

            methodResult.Result = lessonModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
