// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLeaderBoardQuery : IRequest<MethodResult<LeaderBoardSearchModel>>
    {
    }

    public class GetLeaderBoardQueryHandler : IRequestHandler<GetLeaderBoardQuery, MethodResult<LeaderBoardSearchModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetLeaderBoardQueryHandler(AuthContext authContext
            , IUserService userService
            , ISystemService systemService
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _systemService = systemService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<LeaderBoardSearchModel>> Handle(GetLeaderBoardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LeaderBoardSearchModel> methodResult = new MethodResult<LeaderBoardSearchModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;

            var studentIds = await _courseResultRepository.Queryable.Where(x => x.Status != EnumResultStatus.New).Select(c => c.StudentId).Distinct().ToListAsync(cancellationToken);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var students = studentResults?.Content?.Result;
            LeaderBoardSearchModel leaderBoardSearch = new LeaderBoardSearchModel();
            IList<LeaderBoardModel> leaderBoards = new List<LeaderBoardModel>();
            var userIds = students?.Select(x => x.Human).Where(x => x != null && x.UserId != null).Select(x => x!.UserId ?? default).ToList();
            var logActionResults = await _systemService.GetLogActionsByUserIdsAsync(userIds ?? new List<Guid>());
            if (!logActionResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(logActionResults));
                return methodResult;
            }
            var logActions = logActionResults?.Content?.Result;
            if (students != null && students.Any())
            {
                foreach (var item in students)
                {
                    var logAction = logActions?.FirstOrDefault(x => x.Id == item.Human?.UserId);
                    var scores = await _unitResultRepository.Queryable.Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Unfinished).SumAsync(x => x.CorrectCount, cancellationToken);
                    var leaderBoard = new LeaderBoardModel
                    {
                        Id = item.Id,
                        AvatarPath = item.Human?.AvatarPath,
                        FullName = item.Human?.FullName,
                        DailyStreak = logAction?.NumberOfDaysStreak ?? default,
                        TotalScore = scores
                    };
                    leaderBoards.Add(leaderBoard);
                }
            }
            leaderBoards = leaderBoards.OrderByDescending(x => x.TotalScore).ThenBy(x => x.DailyStreak).Select((x, index) => { x.DisplayOrder = index; return x; }).ToList();
            leaderBoardSearch.LeaderBoards = leaderBoards.Take(30).ToList();
            leaderBoardSearch.LeaderBoard = leaderBoards.FirstOrDefault(x => x.Id == studentId);
            methodResult.Result = leaderBoardSearch;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
