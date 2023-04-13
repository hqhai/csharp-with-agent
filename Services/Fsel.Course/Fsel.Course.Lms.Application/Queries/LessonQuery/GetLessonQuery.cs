// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonModel>>
    {
        public Guid LessonId { get; set; }
        public Guid UnitId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetLessonQueryHandler(IMapper mapper,
            AuthContext authContext,
            IUserService userService,
            ILessonRepository lessonRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
            _userService = userService;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<LessonModel> methodResult = new MethodResult<LessonModel>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var lesson = await _lessonRepository.Queryable
                            .Include(x => x.UnitLessons.Where(y => !y.IsDeleted))
                            .Include(x => x.LessonHomeWorks.Where(y => !y.IsDeleted)).ThenInclude(x => x.HomeWork)
                            .Include(x => x.LessonInstructions.Where(y => !y.IsDeleted))
                            .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                            .ThenInclude(x => x.Video)
                            .Include(x => x.LessonResults.Where(y => !y.IsDeleted && y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId))
                            .Select(x => new LessonModel
                            {
                                Id = x.Id,
                                CreatedFullName = x.CreatedFullName,
                                CreatedDate = x.CreatedDate,
                                InstructionContent = x.InstructionContent,
                                Name = x.Name,
                                CourseLevel = x.CourseLevel,
                                Video = _mapper.Map<VideoModel?>(x.LessonVideos.FirstOrDefault()!.Video),
                                LessonResult = _mapper.Map<LessonResultModel>(x.LessonResults.FirstOrDefault()),
                                DisplayOrder = x.UnitLessons.Where(x => x.UnitId == request.UnitId).Select(x => x.DisplayOrder).FirstOrDefault(),
                                IsActive = x.UnitLessons.Any()
                            })
                            .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);

            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonIdNotExist), nameof(request.LessonId), request.LessonId);
                return methodResult;
            }

            methodResult.Result = lesson;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
