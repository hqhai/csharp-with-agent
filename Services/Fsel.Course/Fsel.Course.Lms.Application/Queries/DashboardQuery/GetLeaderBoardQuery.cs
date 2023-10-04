// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLeaderBoardQuery : IRequest<MethodResult<LeaderBoardSearchModel>>
    {
    }

    public class GetLeaderBoardQueryHandler : IRequestHandler<GetLeaderBoardQuery, MethodResult<LeaderBoardSearchModel>>
    {
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private const int LEADERBOARD_TOP = 50; // Chỉ lấy ra 50 người đứng đầu , sau đó sẽ lọc theo daily streak để lấy ra 30 người đứng đầu

        public GetLeaderBoardQueryHandler(IUserService userService
            , ISystemService systemService
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<LeaderBoardSearchModel>> Handle(GetLeaderBoardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LeaderBoardSearchModel> methodResult = new MethodResult<LeaderBoardSearchModel>();

            //List CourseLevel hiện có
            EnumCourseLevel[] enumValues = (EnumCourseLevel[])Enum.GetValues(typeof(EnumCourseLevel));

            // Lấy ra danh sách StudentId đã hoàn thành khóa học
            var studentIds = await _courseResultRepository.Queryable.Where(x => x.Status != EnumResultStatus.New).Select(c => c.StudentId).Distinct().ToListAsync(cancellationToken);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }

            LeaderBoardSearchModel leaderBoardSearch = new LeaderBoardSearchModel();
            IList<LeaderBoardModel> leaderBoards = new List<LeaderBoardModel>();

            // Duyệt dữ liệu của từng Level
            foreach (EnumCourseLevel courseLevel in enumValues)
            {
                var students = studentResults?.Content?.Result?.Where(x => x.CourseLevel == courseLevel);
                if (students == null || !students.Any())
                {
                    continue;
                }

                var userIds = students.Select(x => x.Human).Where(x => x != null && x.UserId != null).Select(x => x!.UserId ?? default).ToList();
                var logActionResults = await _systemService.GetLogActionsByUserIdsAsync(userIds ?? new List<Guid>());

                if (!logActionResults.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallSystemServiceError), nameof(logActionResults));
                    return methodResult;
                }
                var logActions = logActionResults?.Content?.Result;

                foreach (var item in students)
                {
                    var logAction = logActions?.FirstOrDefault(x => x.Id == item.Human?.UserId);
                    var scores = await _unitResultRepository.Queryable.Where(x => x.StudentId == item.Id && x.Status != EnumResultStatus.Unfinished).SumAsync(x => x.CorrectCount, cancellationToken);
                    var leaderBoard = new LeaderBoardModel
                    {
                        Id = item.Id,
                        AvatarPath = item.Human?.AvatarPath,
                        FullName = item.Human?.FullName,
                        TotalScore = scores,
                        UserId = item.Human?.UserId ?? default,
                        CourseLevel = item.CourseLevel
                    };
                    leaderBoards.Add(leaderBoard);
                }
            }

            // Nhóm dữ liệu theo CourseLevel
            var finalLeaderBoards = leaderBoards
                                    .GroupBy(x => x.CourseLevel)
                                    .SelectMany(group => group
                                        .OrderByDescending(x => x.TotalScore)
                                        .Select((item, index) => { item.DisplayOrder = index + 1; return item; })
                                        .Take(LEADERBOARD_TOP)
                                    )
                                    .OrderBy(x => x.CourseLevel)
                                    .ToList();


            leaderBoardSearch.LeaderBoards = finalLeaderBoards;

            methodResult.Result = leaderBoardSearch;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
