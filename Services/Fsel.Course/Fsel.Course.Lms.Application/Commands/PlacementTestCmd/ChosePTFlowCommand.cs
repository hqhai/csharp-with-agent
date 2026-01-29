// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.UserServices;
    using Shared.Enums.ErrorCodes;
    using Shared.Helpers;

    public class ChosePtFlowCommand : IRequest<MethodResult<PtStateModel>>
    {
        public ChosePtFlowCommand(Guid programId)
        {
            ProgramId = programId;
        }

        public Guid ProgramId { get; set; }
    }

    public class ChosePtFlowCommandHandler : IRequestHandler<ChosePtFlowCommand, MethodResult<PtStateModel>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IChangeCourseService _changeCourseService;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICategoryRepository _categoryRepository;
        private readonly SendMailFinishPTPublisher _sendMailFinishPTPublisher;

        public ChosePtFlowCommandHandler(AuthContext authContext,
            IUserService userService,
            IChangeCourseService changeCourseService,
            IMediator mediator,
            IServiceProvider serviceProvider,
            ICategoryRepository categoryRepository,
            SendMailFinishPTPublisher sendMailFinishPTPublisher)
        {
            _authContext = authContext;
            _userService = userService;
            _changeCourseService = changeCourseService;
            _mediator = mediator;
            _serviceProvider = serviceProvider;
            _categoryRepository = categoryRepository;
            _sendMailFinishPTPublisher = sendMailFinishPTPublisher;
        }

        public async Task<MethodResult<PtStateModel>> Handle(ChosePtFlowCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PtStateModel>();
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

            if (student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User));
                return methodResult;
            }

            var navigateActionResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (!navigateActionResult.IsOK
                || (navigateActionResult.Result?.Status != EnumNavigateActionStatus.ChooseProgram
                && navigateActionResult.Result?.Status != EnumNavigateActionStatus.NotDoingYetAnything))
            {
                methodResult.AddErrorBadRequest("This is not time to select program");
                return methodResult;
            }

            var category = await _categoryRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.ProgramId);

            if (category.Type == Shared.Enums.EnumTypeCategory.Program)
            {
                var changeProgramAggregate = await _changeCourseService.GetChangeProgramAggreate(student, cancellationToken);

                var changeProgramDirective = changeProgramAggregate.ChangeProgram(new ChangeProgramRequest
                {
                    ProgramId = request.ProgramId,
                });

                if (changeProgramDirective?.Action == EnumChangeProgramAction.ChangeAndStartPt)
                {
                    var testGroupResult = await _changeCourseService.InitForMustDoPtProgram(new ChangeCourseModel
                    {
                        OwnPtProgramId = changeProgramDirective.ProgramOwnPt.Value,
                        ToProgramId = changeProgramDirective.ToProgramId,
                        StudentId = student.Id,
                        Age = DateTimeHelper.GetYearOld(student.User.Birthday),
                    });

                    var aggregate = new FlowTestResultAggregate(testGroupResult, _serviceProvider, _sendMailFinishPTPublisher);
                    await aggregate.Start();
                    methodResult.Result = await aggregate.ExpotStateData();
                }
                else if (changeProgramDirective?.Action == EnumChangeProgramAction.ChangeDirectlyBecauseByPass)
                {
                    var testGroupResult = await _changeCourseService.InitForProgramByPassPt(new ChangeCourseModel
                    {
                        ToProgramId = changeProgramDirective.ToProgramId,
                        OwnPtProgramId = changeProgramDirective.ToProgramId,
                        StudentId = student.Id,
                        Age = DateTimeHelper.GetYearOld(student.User.Birthday),
                    });

                    methodResult.Result = new PtStateModel { Status = testGroupResult.Status, TestGroupResultId = testGroupResult.Id, StudentId = student.Id };
                }
                else if (changeProgramDirective?.Action == EnumChangeProgramAction.ChangeToProgramExistedPt
                    && navigateActionResult?.Result?.RelatedHistoryId != null
                    && changeProgramDirective?.RelatedPtResultId != null)
                {
                    //var testGroupResult = await _changeCourseService.InitForProgramExistedPt(navigateActionResult.Result.RelatedHistoryId.Value, request.ProgramId, changeProgramDirective.RelatedPtResultId.Value);
                    //methodResult.Result = new PtStateModel { Status = testGroupResult.Status, TestGroupResultId = testGroupResult.Id, StudentId = student.Id };
                    methodResult.AddErrorBadRequest("Cannot select this program becahse it had pt");
                }
                else
                {
                    methodResult.AddErrorBadRequest("Data is wrong for changeProgramDirective");
                }

                return methodResult;
            }
            else
            {
                var changeProgramAggregate = await _changeCourseService.GetChangeSubjectAggreate(student, cancellationToken);

                var changeProgramDirective = changeProgramAggregate.SelectProjectSubject(new ChangeProgramRequest
                {
                    ProgramId = request.ProgramId,
                });

                if (changeProgramDirective?.Action != EnumChangeSubjectAction.ChangeToRecentCourse || changeProgramDirective?.CourseResultId == null)
                {
                    methodResult.AddErrorBadRequest("The selected ProjectId is wrong");
                }
                else
                {
                    var ptResult = await _changeCourseService.SwitchDirectlyToExistCourseForChangeLevel(changeProgramDirective.CourseResultId.Value, student.Id);
                    methodResult.Result = new PtStateModel { StudentId = student.Id, TestGroupResultId = ptResult.Id, Status = ptResult.Status };
                }

                return methodResult;
            }
        }
    }
}
