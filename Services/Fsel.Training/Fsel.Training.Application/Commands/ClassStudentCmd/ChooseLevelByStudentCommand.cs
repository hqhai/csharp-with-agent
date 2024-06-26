// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using MediatR;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Common.Models;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Application.Services.UserServices.Models;

    public class ChooseLevelByStudentCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class ChooseLevelByStudentCommandHandler : IRequestHandler<ChooseLevelByStudentCommand, MethodResult<bool>>
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IClassRepository _classRepository;
        private readonly IMediator _mediator;

        public ChooseLevelByStudentCommandHandler(ICourseService courseService, IUserService userService, AuthContext authContext, IClassRepository classRepository, IMediator mediator)
        {
            _courseService = courseService;
            _userService = userService;
            _authContext = authContext;
            _classRepository = classRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(ChooseLevelByStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var courseResult = await _courseService.GetCourseByLevel(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>()
                {
                    new GenericFilterModel()
                    {
                        Property = "Status",
                        Operator = EnumFilterOperator.Equal,
                        Value = "Active"
                    },
                    new GenericFilterModel()
                    {
                        Property = "CourseLevel",
                        Operator = EnumFilterOperator.Equal,
                        Value = request.CourseLevel.ToString()
                    }
                }
            });
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var @class = await _classRepository.Queryable.Include(x => x.ClassStudents).FirstOrDefaultAsync(p => p.CourseId == course.Id, cancellationToken);

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                if (@class == null)
                {
                    var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = course.Code, CourseLevel = course.CourseLevel }, cancellationToken).ConfigureAwait(false);
                    var code = codeResult.Result;
                    var newClass = new Class
                    {
                        Code = code,
                        Name = code,
                        CourseId = course.Id,
                        Status = EnumClassStatus.Active,
                        PackageId = default,
                        ClassStudents = new List<ClassStudent>()
                    {
                        new ClassStudent() { StudentId = student.Id, IsActive = true }
                    }
                    };
                    @class = _classRepository.Add(newClass);
                }
                else
                {
                    if (!@class.ClassStudents.Any(p => p.StudentId == student.Id))
                    {
                        @class.ClassStudents.Add(new ClassStudent() { StudentId = student.Id, IsActive = true });
                    }
                    @class.Status = EnumClassStatus.Active;
                    _classRepository.Update(@class);
                }

                await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var updateStudentResult = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel
                {
                    StudentId = student.Id,
                    ClassId = @class.Id,
                    CourseLevel = course.CourseLevel,
                    CourseId = @class.CourseId
                });
                if (!updateStudentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateStudentResult.Error);
                    return methodResult;
                }

                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
