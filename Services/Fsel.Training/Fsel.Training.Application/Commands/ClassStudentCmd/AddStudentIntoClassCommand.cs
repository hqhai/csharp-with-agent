// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.SystemServices.Models;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AddStudentIntoClassCommand : AddStudentIntoClassCommandModel, IRequest<MethodResult<Guid>>
    {
    }

    public class AddStudentIntoClassCommandHandler : IRequestHandler<AddStudentIntoClassCommand, MethodResult<Guid>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public AddStudentIntoClassCommandHandler(IClassRepository classRepository, IUserService userService, IMediator mediator, ICourseService courseService, IClassStudentRepository classStudentRepository, ISystemService systemService)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mediator = mediator;
            _courseService = courseService;
            _classStudentRepository = classStudentRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<Guid>> Handle(AddStudentIntoClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Guid>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
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

            var checkCourseSuggest = await _systemService.CheckCourseSuggetConfigByStudent(new CheckCourseSuggetConfigByStudentQueryModel
            {
                BaseCourseLevel = student.BaseCourseLevel ?? default,
                ChooseCourseLevel = course.CourseLevel,
                Age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday)
            });

            if (!checkCourseSuggest.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.YouChoseTheWrongLevel), nameof(course.CourseLevel));
                return methodResult;
            }

            var @class = await _classRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == request.CourseId, cancellationToken);

            if (@class == null)
            {
                var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = course.Code, CourseLevel = course.CourseLevel }, cancellationToken).ConfigureAwait(false);
                var code = codeResult.Result;
                var newClass = new Class
                {
                    Code = code,
                    Name = code,
                    CourseId = request.CourseId,
                    Status = EnumClassStatus.Active,
                    PackageId = request.PackageId ?? default,
                    ClassStudents = new List<ClassStudent>()
                        {
                            new ClassStudent() { StudentId = student.Id, IsActive = true }
                        }
                };
                @class = _classRepository.Add(newClass);
                await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (!await _classStudentRepository.Queryable.AnyAsync(p => p.ClassId == @class.Id && p.StudentId == student.Id, cancellationToken))
                {
                    _classStudentRepository.Add(new ClassStudent() { ClassId = @class.Id, StudentId = student.Id, IsActive = true });
                    await _classStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                if (@class.Status != EnumClassStatus.Active)
                {
                    @class.Status = EnumClassStatus.Active;
                    _classRepository.Update(@class);
                    await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            var updateStudentResult = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel
            {
                StudentId = student.Id,
                ClassId = @class.Id,
                PackageId = request.PackageId,
                CourseLevel = course.CourseLevel,
                NumberOfShield = request.NumberOfShield,
                CourseId = @class.CourseId
            });
            if (!updateStudentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(updateStudentResult.Error);
                return methodResult;
            }

            methodResult.Result = @class.Id;
            return methodResult;
        }
    }
}
