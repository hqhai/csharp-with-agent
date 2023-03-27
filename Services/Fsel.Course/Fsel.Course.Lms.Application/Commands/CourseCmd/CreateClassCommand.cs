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

            var classs = await _trainingService.CreateClassByCheckId(new Services.TrainingServices.Models.CreateClassStudentModel
            {
                Code = request.Code,
                UserId = request.UserId
            });
            var classId = classs?.Content?.Result?.Id;

            var student = await _studentService.GetStudentByUserIdAsync(request.UserId.ToString());
            if (!student.IsSuccessStatusCode)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var course = await _courseRepository.Queryable.Include(e => e.CourseClassStudents)
                                                            .Where(e => e.Id == request.CourseId)
                                                            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (course != null && course.CourseClassStudents.Count == 0)
            {
                course.CourseClassStudents.Add(new CourseClassStudent
                {
                    CourseId = request.CourseId,
                    ClassId = classId ?? Guid.Empty,
                    StudentId = request.UserId
                });
            }

            student = await _studentService.UpdateStudentByClassAsync(classId ?? Guid.Empty);

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
