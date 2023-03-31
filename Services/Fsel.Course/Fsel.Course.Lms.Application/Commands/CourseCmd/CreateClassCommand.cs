// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Classes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;

        public CreateClassCommandHandler(ICourseRepository courseRepository, IUserService userService
            , IMapper mapper
            , ITrainingService trainingService
            , AuthContext authContext)
        {
            _courseRepository = courseRepository;
            _userService = userService;
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var classs = await _trainingService.CreateClassByCheckId(new Services.TrainingServices.Models.CreateClassStudentModel
            {
                Code = request.Code,
                UserId = _authContext.CurrentUserId
            });
            var classId = classs?.Content?.Result?.Id;

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var course = await _courseRepository.Queryable.Include(x => x.CourseResult)
                                                       .Include(e => e.CourseClassStudents.Where(y => y.IsDeleted == false))
                                                       .Include(x => x.CourseUnitMockTests.Where(y => y.IsDeleted == false))
                                                       .ThenInclude(x => x.Unit)
                                                       .ThenInclude(x => (x ?? new()).UnitLessons.Where(y => y.IsDeleted == false))
                                                       .ThenInclude(x => x.Lesson)
                                                       .ThenInclude(x => (x ?? new()).LessonVideos)
                                                       .AsNoTracking()
                                                       .Where(x => x.Id == request.CourseId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist));
                return methodResult;
            }

            if (course.CourseClassStudents.Count == 0)
            {
                course.CourseClassStudents.Add(new CourseClassStudent
                {
                    CourseId = request.CourseId,
                    ClassId = classId ?? default,
                    StudentId = studentId ?? default
                });
            }

            var courseResult = new CourseResult
            {
                StudentId = _authContext.CurrentUserId
            };

            var units = course.CourseUnitMockTests
                        .Where(x => x.UnitId != null)
                        .Select(x => x.Unit)
                        .ToList();

            var unitResults = units
                        .Select(x => new UnitResult
                        {
                            StudentId = _authContext.CurrentUserId,
                            UnitId = (x ?? new()).Id,
                        }).ToList();

            var lessonResults = units
                                .GroupBy(x => x?.Id)
                                .Select(x => new { x.Key, Lessons = x.SelectMany(n => (n ?? new()).UnitLessons).Select(x => x.Lesson) })
                                .SelectMany(x => x.Lessons.Select(n => new LessonResult
                                {
                                    StudentId = _authContext.CurrentUserId,
                                    CourseId = course.Id,
                                    UnitId = x.Key ?? default,
                                    LessonId = n?.Id ?? default,
                                    VideoResult = new VideoResult
                                    {
                                        VideoId = (n ?? new()).LessonVideos.FirstOrDefault()?.VideoId ?? default,
                                        StudentId = _authContext.CurrentUserId
                                    }
                                })).ToList();

            course.CourseResult = courseResult;
            course.UnitResults = unitResults;
            course.LessonResults = lessonResults;
            student = await _userService.UpdateStudentByClassAsync(classId ?? Guid.Empty);

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Update(course ?? new Course());
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });
            return methodResult;
        }
    }
}
