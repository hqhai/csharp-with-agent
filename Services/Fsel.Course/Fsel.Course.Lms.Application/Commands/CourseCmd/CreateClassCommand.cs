// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Classes;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

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

            var course = await _courseRepository.GetIncludeLessonVideoByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist));
                return methodResult;
            }
            else if (course.Status != EnumCourseStatus.Active)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseMustActiveState));
                return methodResult;
            }

            var classs = await _trainingService.CreateClassByCheckId(new CreateClassStudentModel
            {
                Code = request.Code,
                UserId = _authContext.CurrentUserId
            });
            var classId = classs?.Content?.Result?.Id;

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var courseClassStudents = course.CourseClassStudents.Where(x => x.StudentId == studentId && x.ClassId == classId).ToList();
            if (courseClassStudents.Count == 0)
            {
                course.CourseClassStudents.Add(new CourseClassStudent
                {
                    CourseId = request.CourseId,
                    ClassId = classId ?? default,
                    StudentId = studentId ?? default
                });
            }
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
