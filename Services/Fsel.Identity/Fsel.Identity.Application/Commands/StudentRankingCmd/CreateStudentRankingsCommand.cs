// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRankingCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using Fsel.Identity.Domain.Models.EntityModels;
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
        private readonly AuthContext _authContext;
        private const int POSITION_CHANGE = 31;

        public CreateStudentRankingsCommandHandler(IMapper mapper,
            IStudentRankingRepository studentRankingRepository,
            ILmsCourseService lmsCourseService,
            LeaderBoardPublisher leaderBoardPublisher,
            AuthContext authContext,
            IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _mapper = mapper;
            _studentRankingRepository = studentRankingRepository;
            _lmsCourseService = lmsCourseService;
            _leaderBoardPublisher = leaderBoardPublisher;
            _authContext = authContext;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<List<StudentRankingModel>>> Handle(CreateStudentRankingsCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<StudentRankingModel>> methodResult = new MethodResult<List<StudentRankingModel>>();

            // Lấy dữ liệu leaderboard hiện tại

            var testUserId = new Guid("c0b6a166-02c3-4de4-a770-2d76052c9507");
            var currentLeaderBoard = await _lmsCourseService.GetLeaderBoard(testUserId).ConfigureAwait(false);
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
                                .Where(s => studentIds.Contains(s.StudentId))
                                .OrderBy(s => s.StudentId)
                                .ThenByDescending(s => s.DailyDate)
                                .ToList();

            var studentStreaks = new Dictionary<Guid, int>(); // <StudentId, StreakCount>

            foreach (var studentId in studentIds)
            {
                var studentData = dailyStreaks.Where(s => s.StudentId == studentId).ToList();
                int streak = 0;
                DateTime currentDate = DateTime.Now.Date;

                // Kiểm tra bản ghi gần nhất với currentDate
                var nearestRecord = studentData.FirstOrDefault();
                if (nearestRecord == null || nearestRecord.DailyDate != currentDate)
                {
                    studentStreaks[studentId] = 0; // Không có bản ghi cho ngày hiện tại
                    continue;
                }

                DateTime lastValidDate = nearestRecord.DailyDate;
                streak++; // Bản ghi cho ngày hiện tại

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

            List<StudentRanking> studentRankings = new List<StudentRanking>();
            foreach (var item in currentLeaderBoardResult.LeaderBoards!)
            {
                var studentRanking = new StudentRanking
                {
                    StudentId = item.Id,
                    DailyStreak = studentStreaks.ContainsKey(item.Id) ? studentStreaks[item.Id] : 0,  // Gán giá trị từ studentStreaks
                    CurrentPosition = item.DisplayOrder,
                    TotalScore = item.TotalScore,
                    Level = item.Level,
                };
                studentRankings.Add(studentRanking);
            }

            if (studentRankings.Count == 0)
            {
                methodResult.Result = new List<StudentRankingModel>();
                return methodResult;
            }


            // Lấy dữ liệu leaderboard trước đó
            var previousLeaderBoard = await _studentRankingRepository.Queryable.ToListAsync(cancellationToken);
            var previousLeaderBoardResult = _mapper.Map<List<StudentRanking>>(previousLeaderBoard);

            bool allElementsMatch = studentRankings.All(currentItem =>
                previousLeaderBoardResult.Exists(prevItem =>
                    prevItem.StudentId == currentItem.StudentId &&
                    prevItem.CurrentPosition == currentItem.CurrentPosition &&
                    prevItem.Level == currentItem.Level));

            if (allElementsMatch)
            {
                methodResult.Result = _mapper.Map<List<StudentRankingModel>>(previousLeaderBoardResult);
                return methodResult;
            }

            // Tính toán thứ hạng
            List<StudentRanking> toUpdate = new List<StudentRanking>();
            List<StudentRanking> toAdd = new List<StudentRanking>();

            foreach (var current in studentRankings)
            {
                var previous = previousLeaderBoardResult.FirstOrDefault(p => p.StudentId == current.StudentId);
                if (previous == null)
                {
                    current.PositionChange = POSITION_CHANGE - current.CurrentPosition;
                    toAdd.Add(current);
                }
                else
                {
                    int positionChange = previous.CurrentPosition - current.CurrentPosition;
                    if (positionChange != 0)
                    {
                        previous.PositionChange = positionChange;
                        previous.CurrentPosition = current.CurrentPosition;
                        previous.TotalScore = current.TotalScore;
                        toUpdate.Add(previous);
                    }
                }
            }
            //Lấy list học sinh bị rớt khỏi bảng xếp hạng
            var toDelete = previousLeaderBoardResult
                .Where(prev => !studentRankings.Any(curr => curr.StudentId == prev.StudentId))
                .ToList();

            //// Cập nhật PositionChange cho StudentRanking
            //foreach (var item in toAdd.Concat(toUpdate))
            //{
            //    var target = studentRankings.First(s => s.StudentId == item.StudentId);
            //    target.PositionChange = item.PositionChange;
            //}

            await _studentRankingRepository.ExecuteTransactionAsync(async () =>
            {
                //Cập nhật vào bảng StudentRanking
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
                await _studentRankingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var studentRankingRealTime = _mapper.Map<List<StudentRankingRealTime>>(studentRankings);


                // Gửi dữ liệu qua websocket
                LeaderBoardQueueModel leaderBoards = new LeaderBoardQueueModel
                {
                    StudentRankings = studentRankingRealTime,
                    UserId = _authContext.CurrentUserId
                };

                await _leaderBoardPublisher.Publish(leaderBoards, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<StudentRankingModel>>(studentRankings);
                return methodResult;
            });

            return methodResult;
        }
    }
}
