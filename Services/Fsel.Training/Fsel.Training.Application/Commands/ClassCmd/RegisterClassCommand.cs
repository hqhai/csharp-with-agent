// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RegisterClassCommand : RegisterClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class RegisterClassCommandHandler : IRequestHandler<RegisterClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICourseService _courseService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;

        public RegisterClassCommandHandler(IClassRepository classRepository,
            IClassStudentRepository classStudentRepository,
            IUserService userService,
            IMapper mapper,
            ICourseService courseService,
            AuthContext authContext,
            IMediator mediator)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _userService = userService;
            _mapper = mapper;
            _courseService = courseService;
            _authContext = authContext;
            _mediator = mediator;
        }

        public async Task<MethodResult<ClassModel>> Handle(RegisterClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId ?? _authContext.CurrentUserId);
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

            var courseResult = await _courseService.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = course.Code, CourseLevel = course.CourseLevel }, cancellationToken).ConfigureAwait(false);
            var code = codeResult.Result;

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                var classActive = await _classRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == course.Id, cancellationToken);
                if (classActive == null)
                {
                    classActive = await CreateClassAsync(code, request.CourseId, request.PackageId, request.LiveTimeFrameId, request.LiveDays);
                }
                var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.ClassId == classActive.Id, cancellationToken);
                if (classStudent == null)
                {
                    await CreateClassStudentAsync(classActive, student.Id);
                }
                else
                {
                    await UpdateActiveClassStudentAsync(classStudent);
                }
                await InActiveClassStudent(classActive, student.Id);

                var updateStudentResult = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel
                {
                    StudentId = student.Id,
                    ClassId = classActive.Id,
                    CourseLevel = course.CourseLevel,
                    CourseId = classActive.CourseId
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassModel>(classActive);
                return methodResult;
            });

            return methodResult;
        }

        private async Task<Class> CreateClassAsync(string? code, Guid courseId, Guid packageId, Guid? liveTimeFrameId, IList<DayOfWeek>? liveDays)
        {
            var newClass = new Class
            {
                Code = code,
                Name = code,
                CourseId = courseId,
                Status = EnumClassStatus.Active,
                PackageId = packageId,
                LiveTimeFrameId = liveTimeFrameId,
                LiveDays = liveDays,
            };
            _classRepository.Add(newClass);
            await _classRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return newClass;
        }

        private async Task<Class> CreateClassStudentAsync(Class classToUpdate, Guid studentId)
        {
            try
            {
                classToUpdate.ClassStudents.Add(new ClassStudent { StudentId = studentId, IsActive = true });
                _classRepository.Update(classToUpdate);
                await _classRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                return classToUpdate;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the class object.", ex);
            }
        }

        private async Task InActiveClassStudent(Class classToUpdate, Guid studentId)
        {
            var classStudents = await _classStudentRepository.Queryable.Where(x => x.StudentId == studentId && x.IsActive && x.ClassId != classToUpdate.Id)
                                                            .ToListAsync();
            if (classStudents == null || !classStudents.Any())
            {
                return;
            }
            foreach (var classStudent in classStudents)
            {
                await UpdateActiveClassStudentAsync(classStudent, false);
            }
        }

        private async Task UpdateActiveClassStudentAsync(ClassStudent classStudent, bool isActive = true)
        {
            try
            {
                classStudent.IsActive = isActive;
                _classStudentRepository.Update(classStudent);
                await _classStudentRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the class object.", ex);
            }
        }
    }
}
