// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartLessonCommand : StartLessonCommandModel, IRequest<MethodResult<LessonResultModel>>
    {
    }

    public class StartLessonCommandHandler : IRequestHandler<StartLessonCommand, MethodResult<LessonResultModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public StartLessonCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , ILessonRepository lessonRepository
            , ILessonResultRepository lessonResultRepository
            , IHomeWorkRepository homeWorkRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonResultModel> methodResult = new MethodResult<LessonResultModel>();

            #region Validation

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist), nameof(request.CourseId), request.CourseId);
                return methodResult;
            }
            else if (course.Status == EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsNewStateCantStartLesson), nameof(course.Status), course.Status);
                return methodResult;
            }
            var unit = await _unitRepository.GetByIdAsync(request.UnitId);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitNotExist), nameof(request.UnitId), request.UnitId);
                return methodResult;
            }
            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonVideos).FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            #endregion Validation

            var studentId = student?.Content?.Result?.Id;
            var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                                                .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == request.LessonId))
                                                .ToListAsync(cancellationToken);

            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonId == request.LessonId && x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
            if (lessonResult == null)
            {
                lessonResult = new LessonResult
                {
                    StudentId = studentId ?? default,
                    CourseId = course.Id,
                    UnitId = unit.Id,
                    LessonId = request.LessonId,
                    VideoResult = new VideoResult
                    {
                        VideoId = lesson.LessonVideos.FirstOrDefault()!.VideoId,
                        Status = EnumResultStatus.Process,
                        StudentId = studentId ?? default,
                    },
                    HomeWorkResults = homeWorks.Select(x => new HomeWorkResult
                    {
                        HomeWorkId = x.Id,
                        Status = EnumResultStatus.Unfinished,
                        StudentId = studentId ?? default,
                    }).ToList()
                };
                _lessonResultRepository.Add(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<LessonResultModel>(lessonResult);
            return methodResult;
        }
    }
}
