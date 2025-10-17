// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.CourseServices.CommandModels;
    using Fsel.Training.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChooseLevelByStudentCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class ChooseLevelByStudentCommandHandler : IRequestHandler<ChooseLevelByStudentCommand, MethodResult<bool>>
    {
        private readonly ICourseService _courseService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public ChooseLevelByStudentCommandHandler(ICourseService courseService, AuthContext authContext, IMediator mediator, IUserService userService)
        {
            _courseService = courseService;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(ChooseLevelByStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

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

            var courseResult = await _courseService.GetCourseForChooseLevel(new GetCourseForChooseLevelQueryModel()
            {
                StudentId = student.Id,
                CourseLevel = request.CourseLevel
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

            if (course.IsHasCourseResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(course.IsHasCourseResult));
                return methodResult;
            }

            var addStudentIntoClass = await _mediator.Send(new AddStudentIntoClassCommand()
            {
                UserId = _authContext.CurrentUserId,
                CourseId = course.CourseId,
                NumberOfShield = 0
            }, cancellationToken);

            if (!addStudentIntoClass.IsOK)
            {
                methodResult.AddError(addStudentIntoClass.ErrorMessages);
                return methodResult;
            }

            await _courseService.ChooseLevelPTAsync(new SavePlacementTestGroupResultCommandModel
            {
                ChooseCourseLevel = request.CourseLevel
            });

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
