// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Courses;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StudentServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateClassCommand : CreateClassCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;

        public CreateClassCommandHandler(ICourseRepository courseRepository, IStudentService studentService
            , IMapper mapper
            , ITrainingService trainingService)
        {
            _courseRepository = courseRepository;
            _studentService = studentService;
            _mapper = mapper;
            _trainingService = trainingService;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var training = await _trainingService.CreateTrainingByCheckId(new Services.TrainingServices.Models.CreateClassStudentModel
            {
                ClassId = request.ClassId,
                Code = request.Code,
                UserId = request.UserId
            });
            var classId = training?.Content?.Result?.Id;

            var students = await _studentService.GetStudentByClassIdAsync(classId ?? Guid.Empty);
            var liststudent = students?.Content?.Result;

            if (liststudent == null || liststudent.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumCourseErrorCode.StudentsNotExist));
                return methodResult;
            }
            else if (liststudent.Count > 12)
            {
                training = await _trainingService.CreateTrainingByCheckId(new Services.TrainingServices.Models.CreateClassStudentModel
                {
                    ClassId = request.ClassId,
                    Code = request.Code,
                    UserId = request.UserId
                });
            }
            classId = training?.Content?.Result?.Id;

            var course = await _courseRepository.Queryable.Include(e => e.CourseStudentTrainings)
                                                            .Where(e => e.Id == request.CourseId)
                                                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (course == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumCourseErrorCode.CourseNotExist));
                return methodResult;
            }
            var student = await _studentService.GetStudentByUserIdAsync(request.UserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            course.CourseStudentTrainings.Add(new CourseStudent
            {
                CourseId = request.CourseId,
                StudentId = studentId ?? Guid.Empty
            });
            course.CourseClasses.Add(new CourseClassStudent
            {
                CourseId = request.CourseId,
                ClassId = classId ?? Guid.Empty
            });

            student = await _studentService.UpdateStudentByClassAsync(classId ?? Guid.Empty);

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Update(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });
            return methodResult;
        }
    }
}
