// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using CourseChangeQuery;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.UserNavigationActionModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckDonePtByStudentIdQuery : IRequest<MethodResult<PtStateModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckDonePtByStudentIdHandler : IRequestHandler<CheckDonePtByStudentIdQuery, MethodResult<PtStateModel>>
    {
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;

        public CheckDonePtByStudentIdHandler(IRepository<TestGroupResult> testGroupResult,
            ICourseResultRepository courseResultRepository,
            IUserService userService,
            IMediator mediator)
        {
            _testGroupResult = testGroupResult;
            _courseResultRepository = courseResultRepository;
            _mediator = mediator;
            _userService = userService;
        }

        public async Task<MethodResult<PtStateModel>> Handle(CheckDonePtByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var student = await _userService.GetUserByStudentId(request.StudentId);
            var getUserNavigationResult = await _mediator.Send(new GetUserNavigationQuery
            {
                UserId = student?.Content?.Result?.UserId
            }, cancellationToken);
            if (!getUserNavigationResult.IsOK || getUserNavigationResult.Result == null)
            {
                return new MethodResult<PtStateModel> { StatusCode = StatusCodes.Status400BadRequest, };
            }

            var userNavigation = getUserNavigationResult.Result;
            if (userNavigation.Status == EnumNavigateActionStatus.NotDoingYetAnything
                || userNavigation.Status == EnumNavigateActionStatus.ChooseProgram)
            {
                return new MethodResult<PtStateModel>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Result = new PtStateModel
                    {
                        StudentId = request.StudentId,

                        Status = EnumResultStatus.NotStarted
                    }
                };
            }
            else
            {
                var testGroupResult = await _testGroupResult.ReadQueryable
                 .Include(x => x.CurrentLevel)
                 .OrderByDescending(x => x.CreatedDate)
                 .FirstOrDefaultAsync(x => x.Id == userNavigation.PtResultId, cancellationToken);

                PtStateModel ptState = null;
                if (testGroupResult == null
                    && userNavigation.Status == EnumNavigateActionStatus.ContinueLearning
                    && userNavigation.CurrentCourseResultId.HasValue)
                {
                    var coureResult = await _courseResultRepository.ReadQueryable
                        .Where(x => x.Id == userNavigation.CurrentCourseResultId.Value)
                        .Include(x => x.Course)
                        .FirstOrDefaultAsync(cancellationToken);
                    ptState = new PtStateModel
                    {
                        StudentId = request.StudentId,
                        LevelId = coureResult?.Course.LevelId,
                        SelectedProgramId = coureResult?.Course.ProgramId,
                        SelectedPtProgramId = coureResult?.Course.ProgramId,
                        Status = EnumResultStatus.Done
                    };
                }
                else
                {
                    ptState = new PtStateModel
                    {
                        FlowId = testGroupResult?.FlowId,
                        TestGroupResultId = testGroupResult?.Id,
                        StudentId = request.StudentId,
                        Level = testGroupResult?.CurrentLevel?.Name,
                        LevelId = testGroupResult?.CurrentLevel?.Id,
                        SelectedProgramId = testGroupResult?.ProgramId,
                        SelectedPtProgramId = testGroupResult?.ProgramIdOfPt,
                        Status = testGroupResult?.Status ?? EnumResultStatus.NotStarted
                    };
                }

                return new MethodResult<PtStateModel> { StatusCode = StatusCodes.Status200OK, Result = ptState };
            }
        }
    }
}
