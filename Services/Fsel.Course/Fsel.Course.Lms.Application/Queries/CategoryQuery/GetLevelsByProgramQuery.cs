// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common.ActionResults;
    using Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using MediatR;
    using Services.UserServices;
    using Shared.Helpers;

    public class GetLevelsByProgramQuery : IRequest<MethodResult<List<SelectionLevelModel>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetLevelsByProgramQueryHandler : IRequestHandler<GetLevelsByProgramQuery, MethodResult<List<SelectionLevelModel>>>
    {
        private readonly IMediator _mediator;
        private readonly ITestService _testService;
        private readonly IUserService _userService;

        public GetLevelsByProgramQueryHandler(
            IUserService userService,
            IMediator mediator,
            ITestService testService)
        {
            _userService = userService;
            _mediator = mediator;
            _testService = testService;
        }

        public async Task<MethodResult<List<SelectionLevelModel>>> Handle(GetLevelsByProgramQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<SelectionLevelModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return methodResult;
            }

            var user = studentResult?.Content?.Result?.User;

            if (user == null)
            {
                return methodResult;
            }

            var navigateActionResult = await _mediator.Send(new GetUserNavigationQuery(), cancellationToken);
            if (navigateActionResult?.Result == null || !navigateActionResult.IsOK)
            {
                methodResult.AddErrorBadRequest("PT is not found");
                return methodResult;
            }

            var navigateAction = navigateActionResult.Result;

            if (navigateAction.Status == EnumNavigateActionStatus.ContinuePt)
            {
                methodResult.AddErrorBadRequest("PT is not completed");
                return methodResult;
            }

            if (navigateAction.Status != EnumNavigateActionStatus.ChooseLevel)
            {
                methodResult.AddErrorBadRequest("No subject need select course");
                return methodResult;
            }

            var suggestLevels = await _testService.GetSuggestLevels(
                navigateAction.PtResultId.Value,
                DateTimeHelper.GetYearOld(student.User.Birthday),
                useHighestLevelIdOfPt: true,
                cancellationToken: cancellationToken);

            return new MethodResult<List<SelectionLevelModel>> { Result = suggestLevels, StatusCode = 200 };
        }
    }
}
