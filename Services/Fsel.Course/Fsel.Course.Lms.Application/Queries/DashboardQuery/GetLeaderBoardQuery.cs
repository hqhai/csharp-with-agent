// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System.Collections.Generic;
    using System.Globalization;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ICourseRepository _courseRepository;
        private const int LEADERBOARD_TOP = 50; // Chỉ lấy ra 50 người đứng đầu , sau đó sẽ lọc theo daily streak để lấy ra 30 người đứng đầu
        private const int ROUND_DIGIT = 2; // Làm tròn đến số thập phân thú 2

        public GetLeaderBoardQueryHandler(IUserService userService,
                                          ISystemService systemService,
                                          IUnitResultRepository unitResultRepository,
                                          ICourseResultRepository courseResultRepository,
                                          AuthContext authContext,
                                          NotificationMessagePublisher notificationMessagePublisher,
                                          ICourseRepository courseRepository)
        {
            _userService = userService;
            _systemService = systemService;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _authContext = authContext;
            _notificationMessagePublisher = notificationMessagePublisher;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<LeaderBoardSearchModel>> Handle(GetLeaderBoardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LeaderBoardSearchModel> methodResult = new MethodResult<LeaderBoardSearchModel>();

            #region Cmt code lỏ của Hải Đây Lè
            ////List CourseLevel hiện có
            //EnumCourseLevel[] enumValues = (EnumCourseLevel[])Enum.GetValues(typeof(EnumCourseLevel));

            //// Lấy ra danh sách StudentId đã hoàn thành khóa học
            //var studentIds = await _courseResultRepository.Queryable.Where(x => x.Status != EnumResultStatus.New && x.WorkingStatus == EnumWorkingStatus.Active).Select(c => c.StudentId).Distinct().ToListAsync(cancellationToken);

            //var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            //if (!studentResults.IsSuccessStatusCode)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResults));
            //    return methodResult;
            //}

            LeaderBoardSearchModel leaderBoardSearch = new LeaderBoardSearchModel();
            //IList<LeaderBoardModel> leaderBoards = new List<LeaderBoardModel>();

            //// Duyệt dữ liệu của từng Level
            //foreach (EnumCourseLevel courseLevel in enumValues!)
            //{
            //    var students = studentResults?.Content?.Result?.Where(x => x.CourseLevel == courseLevel);
            //    if (students == null || !students.Any())
            //    {
            //        continue;
            //    }
            //    var userIds = students.Select(x => x.Human).Where(x => x != null && x.UserId != null).Select(x => x!.UserId ?? default).ToList();
            //    var leaderBoardsToAdd = students.Select(student =>
            //    {
            //        var unitResultCaculate = _unitResultRepository.Queryable.Where(x => x.StudentId == student.Id && x.Status != EnumResultStatus.Unfinished);
            //        double totalQuestion = unitResultCaculate.Sum(x => x.CorrectTotal);
            //        var scores = unitResultCaculate.Sum(x => x.CorrectCount);

            //        return new LeaderBoardModel
            //        {
            //            Id = student.Id,
            //            TotalScore = totalQuestion != 0 ? Math.Round((scores / totalQuestion) * 100, ROUND_DIGIT) : 0,
            //            CourseLevel = student.CourseLevel
            //        };
            //    }).ToList();

            //    // Add items to leaderBoards
            //    foreach (var leaderBoardToAdd in leaderBoardsToAdd)
            //    {
            //        leaderBoards.Add(leaderBoardToAdd);
            //    }
            //}

            //// Nhóm dữ liệu theo CourseLevel
            //var finalLeaderBoards = leaderBoards
            //                        .GroupBy(x => x.CourseLevel)
            //                        .SelectMany(group => group
            //                            .OrderByDescending(x => x.TotalScore)
            //                            .Select((item, index) => { item.DisplayOrder = index + 1; return item; })
            //                            .Take(LEADERBOARD_TOP)
            //                        )
            //                        .OrderBy(x => x.CourseLevel)
            //                        .ToList(); 
            #endregion

            // Lấy dữ liệu

            var leaderBoards = await (from a in _courseResultRepository.Queryable
                                      join b in _courseRepository.Queryable on a.CourseId equals b.Id
                                      where a.WorkingStatus == EnumWorkingStatus.Active
                                      group a by b.CourseLevel into g
                                      select new
                                      {
                                          CourseLevel = g.Key,
                                          ScoreStudents = g.Select(courseResult => new LeaderBoardModel()
                                          {
                                              Id = courseResult.StudentId,
                                              TotalScore = courseResult.CorrectTotal != 0 ? Math.Round((courseResult.CorrectCount / (double)courseResult.CorrectTotal) * 100, ROUND_DIGIT) : 0,
                                              CourseLevel = g.Key
                                          }).OrderByDescending(x => x.TotalScore).Take(LEADERBOARD_TOP).ToList()
                                      }).ToListAsync(cancellationToken);

            // Thêm DisplayOrder
            var finalLeaderBoards = leaderBoards
                                    .SelectMany(group => group.ScoreStudents
                                                              .Select((item, index) => { item.DisplayOrder = index + 1; return item; })
                                                              .Take(LEADERBOARD_TOP)
                                    ).OrderBy(x => x.CourseLevel)
                                     .ToList();

            // Gửi thông báo khi đạt top
            var student = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            if (studentId != null)
            {
                var location = finalLeaderBoards.FindIndex(x => x.Id == studentId);
                int locationStudent = 0;
                if (location != -1)
                {
                    locationStudent = location + 1;
                }

                if (locationStudent <= 100 && locationStudent != 0)
                {
                    NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                    {
                        ObjectId = (Guid)studentId,
                        UserIds = new List<Guid>() { (Guid)studentId },
                        SenderId = _authContext.CurrentUserId,
                        ParamsMessage = new List<object> { locationStudent.ToString(CultureInfo.CurrentCulture) },
                        Type = EnumNotificationType.LinkPage,
                        Content = EnumNotificationContent.LeaderBoard
                    };

                    await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                }
            }

            leaderBoardSearch.LeaderBoards = finalLeaderBoards;

            methodResult.Result = leaderBoardSearch;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
