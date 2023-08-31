// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLeaderBoardQuery : IRequest<MethodResult<IList<LeaderBoardModel>>>
    {
    }

    public class GetLeaderBoardQueryHandler : IRequestHandler<GetLeaderBoardQuery, MethodResult<IList<LeaderBoardModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetLeaderBoardQueryHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , ICourseResultRepository courseResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<LeaderBoardModel>>> Handle(GetLeaderBoardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<LeaderBoardModel>> methodResult = new MethodResult<IList<LeaderBoardModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;

            var studentIds = await _courseResultRepository.Queryable.Select(c => c.StudentId).Distinct().ToListAsync(cancellationToken);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            if (!studentResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResults));
                return methodResult;
            }
            var students = studentResults?.Content?.Result;
            var leaderBoards = new List<LeaderBoardModel>();
            if (students != null && students.Any())
            {
                foreach (var item in students)
                {
                    var leaderBoard = new LeaderBoardModel
                    {
                        Id = item.Id,
                        AvatarPath = item.Human?.AvatarPath,
                        DailyStreak = 0,
                        DisplayOrder = 0,
                        FullName = item.Human?.FullName,
                        TotalScore = GetTotalScore(item.Id)
                    };
                    leaderBoards.Add(leaderBoard);
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public int GetTotalScore(Guid studentId)
        {
            var totalScore = 0;
            return 0;
        }
    }
}
