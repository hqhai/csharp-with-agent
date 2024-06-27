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

        public ChooseLevelByStudentCommandHandler(ICourseService courseService, AuthContext authContext, IMediator mediator)
        {
            _courseService = courseService;
            _authContext = authContext;
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
            var addStudentIntoClass = await _mediator.Send(new AddStudentIntoClassCommand()
            {
                UserId = _authContext.CurrentUserId,
                CourseId = course.Id,
                NumberOfShield = 0
            }, cancellationToken);
            if (!addStudentIntoClass.IsOK)
            {
                methodResult.AddError(addStudentIntoClass.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
