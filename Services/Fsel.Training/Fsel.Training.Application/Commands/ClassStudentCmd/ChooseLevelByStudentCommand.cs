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
    using Fsel.Training.Application.Services.OrderServices;
    using Fsel.Training.Application.Services.OrderServices.Model;
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
        private readonly IOrderService _orderService;

        public ChooseLevelByStudentCommandHandler(ICourseService courseService, AuthContext authContext, IMediator mediator, IUserService userService, IOrderService orderService)
        {
            _courseService = courseService;
            _authContext = authContext;
            _mediator = mediator;
            _userService = userService;
            _orderService = orderService;
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

            var eventResults = await _userService.GetEventByUserId(_authContext.CurrentUserId);

            if (eventResults.IsSuccessStatusCode && eventResults.Content != null && eventResults.Content.Result != null && eventResults.Content.Result.Any(p => p.EventContent != null && p.EventContent.IsByPassPayment))
            {
                var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                var student = studentResult.Content?.Result;

                var @events = eventResults.Content.Result;

                var @event = @events.Where(p => p.EventContent != null && p.EventContent.IsByPassPayment).Select(p => p.EventContent).FirstOrDefault();

                if (@event != null)
                {
                    await _orderService.CreateOrderForUserLeaderBoard(new CreateOrderForUserFromLeaderBoardCommandModel()
                    {
                        UserId = _authContext.CurrentUserId,
                        Month = @event.PaymentMonth,
                        FullName = student?.User?.FullName,
                        Email = student?.User?.Email,
                        PaymentMethod = EnumPaymentMethodStatus.BankTransfer,
                        PackageId = default,
                        EventId = default,
                        ExpiredDate = @event.PaymentDate,
                    });
                }
            }

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
