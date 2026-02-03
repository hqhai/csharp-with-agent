// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using System.Threading;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using MediatR;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;

    public class ChangeSubjectCommand : IRequest<MethodResult<ChangeSubjectDirective>>
    {
        public Guid? UserId { get; set; }
    }

    public class ChangeSubjectCommandHandler : IRequestHandler<ChangeSubjectCommand, MethodResult<ChangeSubjectDirective>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IChangeCourseService _changeCourseService;
        private readonly IMediator _mediator;

        public ChangeSubjectCommandHandler(AuthContext authContext,
            IUserService userService,
            IChangeCourseService changeCourseService,
            IMediator mediator)
        {
            _authContext = authContext;
            _userService = userService;
            _changeCourseService = changeCourseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<ChangeSubjectDirective>> Handle(ChangeSubjectCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChangeSubjectDirective>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId ?? _authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student?.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var navigateActionResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!navigateActionResult.IsOK
                || navigateActionResult.Result?.Status != EnumNavigateActionStatus.ContinueLearning)
            {
                methodResult.AddErrorBadRequest("Cant not change subject this time");
                return methodResult;
            }

            await _changeCourseService.InitForChangeSubject(student.Id);

            //var changeCourseAggregate = await _changeCourseService.GetChangeSubjectAggreate(student, cancellationToken);

            //var currentLearningSubjectId = changeCourseAggregate.GetCurrentLearningSubject();
            //if (currentLearningSubjectId == request.SubjectId)
            //{
            //    methodResult.AddErrorBadRequest("This subject is already the current learning subject");
            //    return methodResult;
            //}

            //var changeCourseDirective = changeCourseAggregate.ChangeSubject(new ChangeProgramRequest { ProgramId = request.SubjectId });

            //switch (changeCourseDirective?.Action)
            //{
            //    case EnumChangeSubjectAction.ChangeAndStartPt:
            //        await _changeCourseService.InitForChangeSubject(student.Id);
            //        break;

            //    case EnumChangeSubjectAction.ChangeToRecentCourse:
            //        await _changeCourseService.SwitchDirectlyToExistCourseForChangeLevel(changeCourseDirective.CourseResultId.Value, student.Id);
            //        break;

            //    case EnumChangeSubjectAction.None:
            //        methodResult.AddErrorBadRequest("Subject change not allowed");
            //        break;
            //}

            //methodResult.Result = changeCourseDirective;

            return methodResult;
        }
    }
}
