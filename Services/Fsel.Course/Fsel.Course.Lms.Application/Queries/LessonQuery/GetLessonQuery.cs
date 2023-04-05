// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid LessonId { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly AuthContext _authContext;

        public GetLessonQueryHandler(IMapper mapper, ILessonRepository lessonRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            ArgumentNullException.ThrowIfNull(request);
            var lesson = await _lessonRepository.Queryable
                            .Include(x => x.UnitLessons.Where(y => !y.IsDeleted))
                            .Include(x => x.LessonInstructions.Where(y => !y.IsDeleted))
                            .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                            .ThenInclude(x => x.Video)
                            .Include(x => x.LessonResults)
                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist), nameof(request.LessonId), request.LessonId);
                return methodResult;
            }
            lesson.LessonResults = lesson.LessonResults.Where(y => y.LessonId == request.LessonId && y.CourseId == request.CourseId && y.UnitId == request.UnitId && y.StudentId == _authContext.CurrentUserId).ToList();

            var lessonModel = _mapper.Map<LessonModel>(lesson);
            lessonModel.Video = _mapper.Map<VideoModel>(lesson.LessonVideos.Select(x => x.Video).Where(x => x != null && !x.IsDeleted).FirstOrDefault());
            lessonModel.IsActive = lesson.UnitLessons.Any();

            methodResult.Result = lessonModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
