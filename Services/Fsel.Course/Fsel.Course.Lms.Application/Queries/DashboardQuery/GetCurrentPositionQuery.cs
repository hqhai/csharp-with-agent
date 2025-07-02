// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System.Collections.Generic;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentPositionQuery : IRequest<MethodResult<LeaderBoardSearchModel>>
    {
    }

    public class GetCurrentPositionQueryHandler : IRequestHandler<GetCurrentPositionQuery, MethodResult<LeaderBoardSearchModel>>
    {
        private readonly IUserService _userService;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly AuthContext _authContext;
        private const int ROUND_DIGIT = 2; // Làm tròn đến số thập phân thú 2

        public GetCurrentPositionQueryHandler(IUserService userService
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository
            , AuthContext authContext)
        {
            _userService = userService;

            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<LeaderBoardSearchModel>> Handle(GetCurrentPositionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LeaderBoardSearchModel> methodResult = new MethodResult<LeaderBoardSearchModel>();

            var studentResults = await _userService.GetUserByIds(new List<Guid> { _authContext.CurrentUserId });
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
                return methodResult;
            }
            var student = studentResults?.Content?.Result;

            LeaderBoardSearchModel leaderBoardSearch = new LeaderBoardSearchModel();
            IList<LeaderBoardModel> leaderBoards = new List<LeaderBoardModel>();

            //Case này cho tài khoản mới tạo, chưa tham gia bất cứ lớp học nào, chỉ trả về avatar và fullname
            if (student != null && !student.Any())
            {
                var studentQuery = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                var studentInfoResult = studentQuery?.Content?.Result;
                var studentInfo = new LeaderBoardModel
                {
                    AvatarPath = studentInfoResult?.User?.AvatarPath,
                    FullName = studentInfoResult?.User?.FullName,
                    TotalScore = 0
                };

                leaderBoards.Add(studentInfo);
                leaderBoardSearch.LeaderBoards = leaderBoards;
                methodResult.Result = leaderBoardSearch;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var leaderBoardsToAdd = student!.Select(student =>
            {
                var unitResultCaculate = _unitResultRepository.Queryable.Include(x => x.Unit).Where(x => x.StudentId == student.Id && x.Status != EnumResultStatus.Unfinished && x.Unit!.CourseLevel == student.CourseLevel);
                double totalQuestion = unitResultCaculate.Sum(x => x.CorrectTotal);
                var scores = unitResultCaculate.Sum(x => x.CorrectCount);
                return new LeaderBoardModel
                {
                    Id = student.Id,
                    AvatarPath = student.User?.AvatarPath,
                    FullName = student.User?.FullName,
                    TotalScore = totalQuestion != 0 ? Math.Round((scores / totalQuestion) * 100, ROUND_DIGIT) : 0,
                    CourseLevel = student.CourseLevel
                };
            }).ToList();

            //Add Item vào leaderBoard
            foreach (var leaderBoardToAdd in leaderBoardsToAdd)
            {
                leaderBoards.Add(leaderBoardToAdd);
            }

            // Nhóm dữ liệu theo CourseLevel
            var finalLeaderBoards = leaderBoards
                                    .GroupBy(x => x.CourseLevel)
                                    .SelectMany(group => group
                                        .OrderByDescending(x => x.TotalScore)
                                        .Select((item, index) => { item.DisplayOrder = index + 1; return item; })
                                        .Take(1)
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
