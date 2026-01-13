// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentRankingsCommand : CreateListStudentRankingCommandModel, IRequest<MethodResult<List<StudentRankingModel>>>
    {
    }

    public class CreateStudentRankingsCommandHandler : IRequestHandler<CreateStudentRankingsCommand, MethodResult<List<StudentRankingModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRankingRepository _studentRankingRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly LeaderBoardPublisher _leaderBoardPublisher;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private const int POSITION_CHANGE = 31; // Vị trí nằm ngoài leaderboard là 31 (của tất cả học sinh)
        private const int TOP_LEADER = 30; //top leaderboard sẽ lấy(30 học sinh đầu tiên của level)
        private readonly IStudentRepository _studentRepository;

        public CreateStudentRankingsCommandHandler(IMapper mapper,
            IStudentRankingRepository studentRankingRepository,
            ILmsCourseService lmsCourseService,
            LeaderBoardPublisher leaderBoardPublisher,
            IStudentDailyStreakRepository studentDailyStreakRepository,
            IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRankingRepository = studentRankingRepository;
            _lmsCourseService = lmsCourseService;
            _leaderBoardPublisher = leaderBoardPublisher;
            _studentDailyStreakRepository = studentDailyStreakRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<List<StudentRankingModel>>> Handle(CreateStudentRankingsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<StudentRankingModel>> methodResult = new MethodResult<List<StudentRankingModel>>();

            // Tổng hợp dữ liệu LeaderBoard
            var currentLeaderBoard = await _lmsCourseService.GetLeaderBoard().ConfigureAwait(false);
            var currentLeaderBoardResult = currentLeaderBoard?.Content?.Result;

            if (currentLeaderBoardResult == null || currentLeaderBoardResult!.LeaderBoards?.Count == 0)
            {
                methodResult.Result = new List<StudentRankingModel>();
                return methodResult;
            }

            // Lấy dữ liệu DailyStreak của list học sinh
            var studentIds = currentLeaderBoardResult!.LeaderBoards?.Select(x => x.Id).ToList();
            if (studentIds == null || !studentIds.Any())
            {
                methodResult.Result = new List<StudentRankingModel>();
                return methodResult;
            }
            var dailyStreaks = _studentDailyStreakRepository.Queryable
                                .WhereBulkContains(studentIds, s => (s.StudentId))
                                .OrderBy(s => s.StudentId)
                                .ThenByDescending(s => s.DailyDate)
                                .ToList();

            //Tính toán chuỗi dailystreak của học sinh
            var studentStreaks = CalculateStudentDailyStreaks(studentIds, dailyStreaks);

            //Lấy danh sách leaderboard hiện tại
            List<StudentRanking> studentRankings = GetCurrentLeaderBoard(currentLeaderBoardResult, studentStreaks);
            if (studentRankings.Count == 0)
            {
                methodResult.Result = new List<StudentRankingModel>();
                return methodResult;
            }

            // Lấy dữ liệu leaderboard trước đó
            var previousLeaderBoard = await _studentRankingRepository.Queryable.ToListAsync(cancellationToken);
            var previousLeaderBoardResult = _mapper.Map<List<StudentRanking>>(previousLeaderBoard);

            //Kiểm tra sự thay đổi của 2 danh sách, nếu không có thay đổi thì không làm gì cả
            bool allElementsMatch = studentRankings.All(currentItem =>
                previousLeaderBoardResult.Exists(prevItem =>
                    prevItem.StudentId == currentItem.StudentId &&
                    prevItem.CurrentPosition == currentItem.CurrentPosition &&
                    prevItem.CourseLevel == currentItem.CourseLevel &&
                    prevItem.DailyStreak == currentItem.DailyStreak
                    ));

            if (allElementsMatch)
            {
                methodResult.Result = _mapper.Map<List<StudentRankingModel>>(previousLeaderBoardResult);
                return methodResult;
            }

            // Tính toán thứ hạng thay đổi khi so sánh 2 danh sách trước đó và hiện tại
            var (toAdd, toUpdate, toDelete) = CalculateRankingChanges(studentRankings, previousLeaderBoardResult);

            //Thực hiện các hành động lưu xuống database , gửi lên websocket
            await _studentRankingRepository.ExecuteTransactionAsync(async () =>
            {
                await UpdateStudentRankingDatabase(toAdd, toUpdate, toDelete);
                var studentIds = studentRankings.Select(s => s.StudentId);
                var studentInfo = _studentRepository.Queryable.Include(x => x.User).Where(x => studentIds.Contains(x.Id)).ToList();
                var studentRankingRealTime = _mapper.Map<List<StudentRankingRealTime>>(studentRankings);
                studentRankingRealTime.ForEach(x =>
                {
                    var student = studentInfo.FirstOrDefault(s => s.Id == x.StudentId);
                    if (student != null)
                    {
                        x.FullName = student.User?.FullName;
                        x.AvatarPath = student.User?.AvatarPath;
                    }
                });

                await SendToWebSocket(studentRankingRealTime, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<StudentRankingModel>>(studentRankings);
                return methodResult;
            });

            return methodResult;
        }

        #region Handler Data

        /// <summary>
        /// Lấy LeaderBoard hiện tại
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private static List<StudentRanking> GetCurrentLeaderBoard(LeaderBoardSearchModel currentLeaderBoardResult, Dictionary<Guid, int> studentStreaks)
        {
            List<StudentRanking> studentRankings = currentLeaderBoardResult.LeaderBoards!
                                                   .Select(item => new StudentRanking
                                                   {
                                                       StudentId = item.Id,
                                                       DailyStreak = studentStreaks.ContainsKey(item.Id) ? studentStreaks[item.Id] : 0,
                                                       CurrentPosition = item.DisplayOrder,
                                                       TotalScore = item.TotalScore,
                                                       CourseLevel = item.CourseLevel,
                                                   })
                                                   .ToList();

            var groupedStudentRankings = studentRankings
                .GroupBy(x => x.CourseLevel)
                .SelectMany(group => group
                    .OrderByDescending(x => x.TotalScore)
                    .ThenByDescending(x => x.DailyStreak)
                    .Select((x, index) => { x.CurrentPosition = index + 1; return x; })
                    .Take(TOP_LEADER)
                )
                .ToList();

            return groupedStudentRankings;
        }

        /// <summary>
        /// Tính toán chuỗi dailystreaks của học sinh
        /// </summary>
        /// <param name="studentIds"></param>
        /// <param name="dailyStreaks"></param>
        /// <returns></returns>
        private static Dictionary<Guid, int> CalculateStudentDailyStreaks(List<Guid> studentIds, List<StudentDailyStreak> dailyStreaks)
        {
            Dictionary<Guid, int> studentStreaks = new Dictionary<Guid, int>();
            foreach (var studentId in studentIds)
            {
                var studentData = dailyStreaks.Where(s => s.StudentId == studentId).ToList();
                int streak = 0;
                DateTime currentDate = DateTime.UtcNow.Date;

                // Kiểm tra bản ghi gần nhất với currentDate
                var nearestRecord = studentData.FirstOrDefault();
                if (nearestRecord == null || nearestRecord.DailyDate > currentDate)
                {
                    studentStreaks[studentId] = 0; // Không có bản ghi cho ngày hiện tại
                    continue;
                }
                else if (nearestRecord.DailyDate == currentDate)
                {
                    streak++; // Bản ghi cho ngày hiện tại
                }

                DateTime lastValidDate = nearestRecord.DailyDate;

                // Đếm chuỗi ngày liên tiếp từ bản ghi gần nhất với currentDate
                foreach (var record in studentData.Skip(1))
                {
                    if ((record.DailyDate - lastValidDate).Days == -1) // đi ngược về ngày trước đó
                    {
                        streak++;
                        lastValidDate = record.DailyDate;
                    }
                    else
                    {
                        break;
                    }
                }

                studentStreaks[studentId] = streak;
            }

            return studentStreaks;
        }

        /// <summary>
        /// Tính toán thứ hạng
        /// </summary>
        /// <param name="current"></param>
        /// <param name="previous"></param>
        /// <returns></returns>
        private static (List<StudentRanking> ToAdd, List<StudentRanking> ToUpdate, List<StudentRanking> ToDelete) CalculateRankingChanges(List<StudentRanking> current, List<StudentRanking> previous)
        {
            List<StudentRanking> toAdd = new List<StudentRanking>();
            List<StudentRanking> toUpdate = new List<StudentRanking>();
            List<StudentRanking> toDelete = new List<StudentRanking>();

            foreach (var currentRanking in current)
            {
                var prevRanking = previous.FirstOrDefault(p => p.StudentId == currentRanking.StudentId);

                if (prevRanking == null)
                {
                    // Học sinh này không có trong leaderboard trước đó, nên cần thêm vào.
                    currentRanking.PositionChange = POSITION_CHANGE - currentRanking.CurrentPosition;
                    toAdd.Add(currentRanking);
                }
                else
                {
                    // Học sinh này đã có trong leaderboard trước đó, nên cần so sánh và cập nhật.
                    int positionChange = prevRanking.CurrentPosition - currentRanking.CurrentPosition;
                    if (positionChange != 0)
                    {
                        prevRanking.PositionChange = positionChange;
                        prevRanking.CurrentPosition = currentRanking.CurrentPosition;
                        prevRanking.TotalScore = currentRanking.TotalScore;
                        toUpdate.Add(prevRanking);
                    }
                }
            }

            // Xác định những học sinh bị rớt khỏi leaderboard hiện tại.
            toDelete = previous.Where(prev => !current.Any(curr => curr.StudentId == prev.StudentId)).ToList();

            return (ToAdd: toAdd, ToUpdate: toUpdate, ToDelete: toDelete);
        }

        /// <summary>
        /// Gửi dữ liệu qua webSocket
        /// </summary>
        /// <param name="studentRankingRealTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task SendToWebSocket(List<StudentRankingRealTime> studentRankingRealTime, CancellationToken cancellationToken)
        {
            EnumCourseLevel[] enumValues = (EnumCourseLevel[])Enum.GetValues(typeof(EnumCourseLevel));

            foreach (EnumCourseLevel courseLevel in enumValues)
            {
                LeaderBoardQueueModel leaderBoards = new LeaderBoardQueueModel
                {
                    StudentRankings = studentRankingRealTime.Where(x => x.CourseLevel == courseLevel).ToList(),
                    CourseLevel = courseLevel
                };

                await _leaderBoardPublisher.Publish(leaderBoards, cancellationToken);
            }
        }

        /// <summary>
        /// Lưu dữ liệu xuống database
        /// </summary>
        /// <param name="toAdd"></param>
        /// <param name="toUpdate"></param>
        /// <param name="toDelete"></param>
        /// <returns></returns>
        private async Task UpdateStudentRankingDatabase(List<StudentRanking> toAdd, List<StudentRanking> toUpdate, List<StudentRanking> toDelete)
        {
            if (toDelete.Count > 0)
            {
                await _studentRankingRepository.DeleteListAsync(toDelete);
            }

            if (toUpdate.Count > 0)
            {
                _studentRankingRepository.UpdateList(toUpdate);
            }

            if (toAdd.Count > 0)
            {
                await _studentRankingRepository.AddList(toAdd);
            }

            await _studentRankingRepository.UnitOfWork.SaveEntitiesAsync();
        }

        #endregion Handler Data
    }
}
