// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseChangeQuery
{
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Core.Base;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.UserNavigationActionModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.UserServices;
    using Shared.Enums;
    using Shared.Enums.ErrorCodes;

    public class GetUserNavigationQuery : IRequest<MethodResult<NavigateAction>>
    {
    }

    public class GetUserNavigationQueryHandler : IRequestHandler<GetUserNavigationQuery, MethodResult<NavigateAction>>
    {
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ICourseChangingHistoryRepository _courseChangingHistoryRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public GetUserNavigationQueryHandler(IUserService userService,
            ICourseChangingHistoryRepository courseChangingHistoryRepository,
            ICourseResultRepository courseResultRepository,
            ITestGroupResultRepository testGroupResultRepository,
            AuthContext authContext)
        {
            _userService = userService;
            _authContext = authContext;
            _courseChangingHistoryRepository = courseChangingHistoryRepository;
            _courseResultRepository = courseResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
        }

        public async Task<MethodResult<NavigateAction>> Handle(GetUserNavigationQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<NavigateAction>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var ptResults = await _testGroupResultRepository.ReadQueryable
                .Include(x => x.CourseChangingHistories)
                .Where(x => x.StudentId == student.Id && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            var allCourseChangeHistories = await _courseChangingHistoryRepository.ReadQueryable
                .Where(x => x.StudentId == student.Id)
                .ToListAsync(cancellationToken);

            var courseResults =
                await _courseResultRepository.ReadQueryable
                    .Include(x => x.Course)
                    .ThenInclude(x => x.Level)
                    .Where(x => x.StudentId == student.Id && x.CourseId == student.CourseId && x.WorkingStatus == EnumWorkingStatus.Active)
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync(cancellationToken);

            var currentCourseResult = courseResults.FirstOrDefault();

            var currentStateInfo = new CurrentStateInfo
            {
                LevelId = currentCourseResult?.Course?.LevelId,
                CourseResultId = currentCourseResult?.Id,
                ProgramId = currentCourseResult?.Course?.Level?.ProgramId,
                CourseId = student.CourseId,
                PtResultId = allCourseChangeHistories.Where(x => x.SelectedProgramId == currentCourseResult?.Course?.Level?.ProgramId)
                .OrderByDescending(x => x.CreatedDate).FirstOrDefault()?.PtResultId
            };

            if (currentStateInfo.PtResultId == null && ptResults.Count == 1)
            {
                currentStateInfo.PtResultId = ptResults.First().Id;
            }

            var navigationAggregate = new NavigateUserAggregate(currentStateInfo, allCourseChangeHistories, ptResults);

            methodResult.Result = navigationAggregate.GetNavigateAction();

            return methodResult;
        }
    }
}
