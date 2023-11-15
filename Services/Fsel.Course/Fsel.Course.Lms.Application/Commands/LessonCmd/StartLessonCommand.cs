// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Lessons;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
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
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly AuthContext _authContext;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;

        public StartLessonCommandHandler(ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService
            , IMapper mapper
            , IUnitResultRepository unitResultRepository
            , AuthContext authContext
            , ILessonRepository lessonRepository
            , ILessonResultRepository lessonResultRepository
            , ICourseResultRepository courseResultRepository
            , IHomeWorkRepository homeWorkRepository)
        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
            _mapper = mapper;
            _unitResultRepository = unitResultRepository;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _courseResultRepository = courseResultRepository;
            _homeWorkRepository = homeWorkRepository;
        }

        public async Task<MethodResult<LessonResultModel>> Handle(StartLessonCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonResultModel> methodResult = new MethodResult<LessonResultModel>();

            #region Validation

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

            #endregion Validation

            var method = await Validate(request, studentId, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var lesson = method.Result;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult != null && lessonResult.Status == EnumResultStatus.New)
            {
                lessonResult = await UpdateLessonResult(lesson, lessonResult, cancellationToken);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<LessonResultModel>(lessonResult);
            return methodResult;
        }

        private async Task<LessonResult> UpdateLessonResult(Lesson? lesson, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lesson);
            lessonResult.VideoResult = new VideoResult
            {
                VideoId = lesson.LessonVideos.FirstOrDefault()?.VideoId ?? default,
                Status = EnumResultStatus.Process,
                StudentId = lessonResult.StudentId,
            };
            var homeWorks = await _homeWorkRepository.Queryable.Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                                             .Include(x => x.HomeWorkQuestions)
                                             .ThenInclude(x => x.Question)
                                             .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                             .ToListAsync(cancellationToken);
            lessonResult.HomeWorkResults = homeWorks.Select(x => new HomeWorkResult
            {
                HomeWorkId = x.Id,
                Status = EnumResultStatus.Unfinished,
                StudentId = lessonResult.StudentId,
                CorrectTotal = x.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal)
            }).ToList();

            lessonResult.Status = EnumResultStatus.Process;
            lessonResult = _lessonResultRepository.Update(lessonResult);
            await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return lessonResult;
        }

        private async Task UpdateUnitStatusNew(Domain.Entities.Unit unit, Guid? studentId, CancellationToken cancellationToken)
        {
            var unitResult = unit.UnitResults.FirstOrDefault(x => x.UnitId == unit.Id && x.StudentId == studentId);
            if (unitResult != null && unitResult.Status == EnumResultStatus.New)
            {
                unitResult.Status = EnumResultStatus.Process;
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<MethodResult<Lesson>> Validate(StartLessonCommand request, Guid? studentId, CancellationToken cancellationToken)
        {
            MethodResult<Lesson> methodResult = new MethodResult<Lesson>();
            var course = await _courseRepository.Queryable.Include(x => x.CourseResults).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.Status == EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseIsNewStateCantStartLesson), nameof(course.Status), course.Status);
                return methodResult;
            }

            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults).Include(x => x.UnitResults).FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }

            var lessonResultNew = unit.LessonResults.FirstOrDefault(x => x.LessonId == request.LessonId && x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId);
            if (lessonResultNew == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResultNew));
                return methodResult;
            }

            if (lessonResultNew.Status != EnumResultStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultNotNew));
                return methodResult;
            }

            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonVideos).Include(x => x.LessonHomeWorks).FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson));
                return methodResult;
            }
            var videoId = lesson.LessonVideos.FirstOrDefault()?.VideoId;
            if (videoId == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoId));
                return methodResult;
            }
            if (!lesson.LessonHomeWorks.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lesson.LessonHomeWorks));
                return methodResult;
            }
            await UpdateUnitStatusNew(unit, studentId, cancellationToken).ConfigureAwait(false);
            methodResult.Result = lesson;
            return methodResult;
        }
    }
}
