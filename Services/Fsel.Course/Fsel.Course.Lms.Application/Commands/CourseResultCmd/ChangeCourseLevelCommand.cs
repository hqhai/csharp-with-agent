// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using System.Threading;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Entities;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using MediatR;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;
    using Shared.Helpers;

    public class ChangeCourseLevelCommand : IRequest<MethodResult<ChangeCourseDirective>>
    {
        public Guid LevelId { get; set; }
        public Guid? UserId { get; set; }
    }

    public class ChangeCourseLevelCommandHandler : IRequestHandler<ChangeCourseLevelCommand, MethodResult<ChangeCourseDirective>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IChangeCourseService _changeCourseService;
        private readonly MediatR.IMediator _mediator;

        public ChangeCourseLevelCommandHandler(AuthContext authContext,
            IUserService userService,
            IChangeCourseService changeCourseService,
            MediatR.IMediator mediator)
        {
            _authContext = authContext;
            _userService = userService;
            _changeCourseService = changeCourseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<ChangeCourseDirective>> Handle(ChangeCourseLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ChangeCourseDirective>();

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
                methodResult.AddErrorBadRequest("Cant not select level this time");
                return methodResult;
            }

            var changeCourseAggregate = await _changeCourseService.GetChangeCourseAggreate(student, request.LevelId, cancellationToken);

            var changeCourseDirective =
                changeCourseAggregate.ChangeCourse(new ChangeCourseRequest { LevelId = request.LevelId, RequestType = EnumChangeCourseRequest.ChangeCourse });
            var age = DateTimeHelper.GetYearOld(student.User.Birthday);

            switch (changeCourseDirective?.Action)
            {
                case EnumChangeCourseAction.ChangeAndStartPt:
                    await _changeCourseService.InitForMustDoPtProgram(new ChangeCourseModel
                    {
                        StudentId = student.Id,
                        Age = age,
                        OwnPtProgramId = changeCourseDirective.ProgramOwnPt,
                        ToProgramId = changeCourseDirective.ToProgramId,
                        ToLevelId = changeCourseDirective.ToLevelId,
                        Action = changeCourseDirective.Action,
                        FromInfo = changeCourseDirective.FromInfo
                    });
                    break;

                case EnumChangeCourseAction.ChangeDirectly:
                    await _changeCourseService.SwitchDirectlyToNewCourse(new SelectCourseLevelRequest
                    {
                        StudentId = student.Id,
                        ToProgramId = changeCourseDirective.ToProgramId,
                        ToLevelId = changeCourseDirective.ToLevelId,
                        SelectedLevelId = changeCourseDirective.ToLevelId,
                        SelectedProgramId = changeCourseDirective.ToProgramId,
                        FromInfo = changeCourseDirective.FromInfo,
                        Action = changeCourseDirective.Action,
                        PtResultId = changeCourseDirective.PtResultId
                    });
                    break;

                case EnumChangeCourseAction.ChangeDirectlyBecauseByPass:
                    await _changeCourseService.SwitchDirectlyToNewCourse(
                        new SelectCourseLevelRequest
                        {
                            StudentId = student.Id,
                            ToProgramId = changeCourseDirective.ToProgramId,
                            ToLevelId = changeCourseDirective.ToLevelId,
                            SelectedLevelId = changeCourseDirective.ToLevelId,
                            SelectedProgramId = changeCourseDirective.ToProgramId,
                            FromInfo = changeCourseDirective.FromInfo,
                            Action = changeCourseDirective.Action,
                            PtResultId = changeCourseDirective.PtResultId
                        });
                    break;

                case EnumChangeCourseAction.SwitchToExistedCourse:
                    await _changeCourseService.SwitchDirectlyToExistCourse(changeCourseDirective.CourseResultId.Value, student.Id);
                    break;

                case EnumChangeCourseAction.NotAllow:
                    methodResult.AddErrorBadRequest("Level change not allowed");
                    break;
            }

            methodResult.Result = changeCourseDirective;

            return methodResult;
        }
    }
}
