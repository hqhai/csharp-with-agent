// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseCmd
{
    using Core.Base;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;
    using IMediator = MediatR.IMediator;

    public class ChooseCourseByLevelCommand : IRequest<MethodResult<CourseModel>>
    {
        public Guid LevelId { get; set; }

        public Guid ProgramId { get; set; }
    }

    public class ChooseCourseByLevelCommandHandler : IRequestHandler<ChooseCourseByLevelCommand, MethodResult<CourseModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IChangeCourseService _changeCourseService;
        private readonly IMediator _mediator;

        public ChooseCourseByLevelCommandHandler(
            AuthContext authContext,
            IChangeCourseService changeCourseService,
            IUserService userService,
            IMediator mediator)
        {
            _userService = userService;
            _authContext = authContext;
            _changeCourseService = changeCourseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<CourseModel>> Handle(ChooseCourseByLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var navigateActionResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!navigateActionResult.IsOK
                || navigateActionResult.Result?.Status != EnumNavigateActionStatus.ChooseLevel)
            {
                methodResult.AddErrorBadRequest("Cant not select level this time");
                return methodResult;
            }

            var changeCourseAggregate = await _changeCourseService.GetChangeCourseAggreate(student, request.LevelId, cancellationToken);

            var changeCourseDirective =
                changeCourseAggregate.ChangeCourse(new ChangeCourseRequest { LevelId = request.LevelId, RequestType = EnumChangeCourseRequest.ChangeCourse });

            switch (changeCourseDirective?.Action)
            {
                case EnumChangeCourseAction.ChangeDirectly:
                    await _changeCourseService.SelectCourseLevelAfterPt(new SelectCourseLevelRequest
                    {
                        StudentId = student.Id,
                        SelectedProgramId = changeCourseDirective.ToProgramId,
                        SelectedLevelId = changeCourseDirective.ToLevelId,
                        RelatedHistoryId = navigateActionResult.Result.RelatedHistoryId
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
                            RelatedHistoryId = navigateActionResult.Result.RelatedHistoryId,
                            Action = EnumChangeCourseAction.ChangeDirectlyBecauseByPass
                        });
                    break;

                case EnumChangeCourseAction.SwitchToExistedCourse:
                    await _changeCourseService.SwitchDirectlyToExistCourseForSelectLevelAfterPt(changeCourseDirective.CourseResultId.Value, student.Id, navigateActionResult.Result.RelatedHistoryId);
                    break;

                case EnumChangeCourseAction.NotAllow or EnumChangeCourseAction.ChangeAndStartPt:
                    methodResult.AddErrorBadRequest("Level change not allowed");
                    break;
            }

            return methodResult;
        }
    }
}
