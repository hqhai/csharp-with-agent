// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Lms.Application.Services.OrderServices;
using Fsel.Course.Lms.Application.Services.OrderServices.Model;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Shared.Enums;
using Fsel.Shared.Enums.ErrorCodes;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    public class ChooseStudentLevelToCourseCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel Level { get; set; }
    }

    public class ChooseStudentLevelToCourseCommandHandler : IRequestHandler<ChooseStudentLevelToCourseCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;

        public ChooseStudentLevelToCourseCommandHandler(IUserService userService, IOrderService orderService, AuthContext authContext, ICourseRepository courseRepository)
        {
            _userService = userService;
            _orderService = orderService;
            _authContext = authContext;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChooseStudentLevelToCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var isCheckLevel = request.Level.IsCheckCourseLevel(student?.CourseLevel ?? default);
            if (!isCheckLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.YouChoseTheWrongLevel), nameof(isCheckLevel));
                return methodResult;
            }
            Random random = new Random();
            var courses = await _courseRepository.Queryable.Where(x => x.CourseLevel == request.Level && x.Status == EnumCourseStatus.Active).ToListAsync(cancellationToken);
            var course = courses.OrderBy(x => random.Next(courses.Count)).FirstOrDefault();
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            var orderResult = await _orderService.CreateOrder(new CreateOrderCommandModel
            {
                Address = "Viet Nam",
                Country = "Viet Nam",
                CourseId = course.Id,
                CourseLevel = request.Level,
                FullName = student?.Human?.FullName,
                PaymentMethod = EnumPaymentMethodStatus.Card,
                CodeCourse = course.Code,
                UserId = _authContext.CurrentUserId
            });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddError(orderResult.Error);
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
