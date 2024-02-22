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
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Entities;
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
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public AddStudentIntoClassCommandHandler(IClassRepository classRepository, IUserService userService, IMediator mediator, ICourseService courseService)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mediator = mediator;
            _courseService = courseService;
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

            var courseResult = await _courseService.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result;

            var @class = await _classRepository.Queryable.Include(x => x.ClassStudents).FirstOrDefaultAsync(p => p.CourseId == request.CourseId, cancellationToken);

            if (@class == null)
            {
                var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = course?.Code, CourseLevel = course?.CourseLevel }, cancellationToken).ConfigureAwait(false);
                var code = codeResult.Result;
                var newClass = new Class
                {
                    Code = code,
                    Name = code,
                    CourseId = request.CourseId,
                    Status = EnumClassStatus.Active,
                    PackageId = request.PackageId,
                    ClassStudents = new List<ClassStudent>()
                    {
                        new ClassStudent() { StudentId = student!.Id, IsActive = true }
                    }
                };
                @class = _classRepository.Add(newClass);
            }
            else
            {
                if (@class.ClassStudents.Any(p => p.StudentId == student!.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                    return methodResult;
                }

                @class.Status = EnumClassStatus.Active;
                @class.ClassStudents.Add(new ClassStudent() { StudentId = student!.Id, IsActive = true });
                _classRepository.Update(@class);
            }

            await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var updateStudentResult = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel()
            {
                StudentId = student!.Id,
                ClassId = @class.Id,
                PackageId = @class.PackageId,
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
