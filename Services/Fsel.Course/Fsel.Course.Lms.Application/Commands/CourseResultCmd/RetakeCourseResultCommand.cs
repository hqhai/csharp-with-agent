// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.CommandModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RetakeCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class RetakeCourseResultCommandHandler : IRequestHandler<RetakeCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public RetakeCourseResultCommandHandler(IMapper mapper
            , ITrainingService trainingService
            , AuthContext authContext
            , IMediator mediator
            , IUserService userService
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository)
        {
            _mapper = mapper;
            _trainingService = trainingService;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(RetakeCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var isUsedCourseDone = await _courseResultRepository.Queryable.AnyAsync(x => x.StudentId == student.Id && x.Status == EnumResultStatus.Done, cancellationToken);
            if (!isUsedCourseDone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isUsedCourseDone));
                return methodResult;
            }

            if (!student.CourseLevel.CheckLevelByPass(request.CourseLevel, isUsedCourseDone))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.CourseLevel));
                return methodResult;
            }

            var course = await GetCourseAsync(request.CourseLevel);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var classResult = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel
            {
                Code = course.Code,
                CourseId = course.Id,
                CourseLevel = course.CourseLevel,
                UserId = _authContext.CurrentUserId,
            });
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError), nameof(classResult));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Course?> GetCourseAsync(EnumCourseLevel courseLevel)
        {
            var course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.Active && x.CourseLevel == courseLevel)
                                                        .OrderByDescending(x => x.UpdatedDate)
                                                        .ThenByDescending(x => x.CreatedDate)
                                                        .FirstOrDefaultAsync();
            if (course == null)
            {
                course = await _courseRepository.Queryable.Where(x => x.Status == EnumCourseStatus.InActive && x.CourseLevel == courseLevel)
                                                      .OrderByDescending(x => x.UpdatedDate)
                                                      .ThenByDescending(x => x.CreatedDate)
                                                      .FirstOrDefaultAsync();
            }

            return course;
        }

        private async Task<Course?> GetAndCreateCourseResultAsync(Course course, StudentModel student, CancellationToken cancellationToken)
        {
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == course.Id && x.StudentId == student.Id, cancellationToken);
            if (courseResult == null)
            {
                courseResult = new CourseResult
                {
                    CourseId = course.Id,
                    StudentId = student.Id,
                    Status = EnumResultStatus.New,
                    WorkingStatus = EnumWorkingStatus.Active
                };
                _courseResultRepository.Add(courseResult);
            }
            else
            {
                var courseResults = await _courseResultRepository.Queryable.Where(x => x.StudentId == student.Id).ToListAsync(cancellationToken);
                var courses = await _courseRepository.Queryable.Where(x => x.ParentCourseId == course.Id && !courseResults.Any(y => y.CourseId == x.Id)).OrderBy(x => x.Priority).ToListAsync(cancellationToken);
            }

            await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var courseResultActive = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.WorkingStatus == EnumWorkingStatus.Active && x.Id != courseResult.Id, cancellationToken);
            if (courseResultActive != null)
            {
                courseResultActive.WorkingStatus = EnumWorkingStatus.InActive;
                _courseResultRepository.Update(courseResultActive);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return course;
        }
    }
}
