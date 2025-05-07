// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeRankingQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        public Guid? VideoTimeCodeResultId { get; set; }
        public Guid? LessonResultId { get; set; }
        public EnumTimeCodeType? Type { get; set; }
    }

    public class GetVideoTimeCodeRankingQueryHandler : IRequestHandler<GetVideoTimeCodeRankingQuery, MethodResult<PagingItemsModel<TestResultRankingModel>>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;

        public GetVideoTimeCodeRankingQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IUserService userService)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> Handle(GetVideoTimeCodeRankingQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult = new MethodResult<PagingItemsModel<TestResultRankingModel>>();

            if (request.VideoTimeCodeResultId.HasValue)
            {
                methodResult = await GetRankingToTimeCode(methodResult, request, cancellationToken);
            }
            else if (request.LessonResultId.HasValue && request.Type.HasValue)
            {
                methodResult = await GetRankingToTimeCode(methodResult, request, request.Type.Value, cancellationToken);
            }
            return methodResult;
        }

        private async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> GetRankingToTimeCode(MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult, GetVideoTimeCodeRankingQuery request, CancellationToken cancellationToken)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Include(x => x.VideoResult).Include(x => x.VideoTimeCode).FirstOrDefaultAsync(x => x.Id == request.VideoTimeCodeResultId, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            if (videoTimeCodeResult.VideoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult.VideoResult));
                return methodResult;
            }
            if (videoTimeCodeResult.VideoTimeCode?.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.GetByIdAsync(videoTimeCodeResult.VideoResult.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var query = from bastQ in _videoTimeCodeResultRepository.Queryable
                        where bastQ.VideoTimeCodeId == videoTimeCodeResult.VideoTimeCodeId &&
                              bastQ.Status == EnumResultStatus.Done &&
                              bastQ.VideoResult!.LessonResult!.CourseId == lessonResult.CourseId &&
                              bastQ.VideoResult.LessonResult.UnitId == lessonResult.UnitId &&
                              bastQ.VideoResult.LessonResult.LessonId == lessonResult.LessonId
                        select new TestResultRankingModel
                        {
                            WorkingTime = bastQ.WorkingTime,
                            CorrectCount = bastQ.CorrectCount,
                            CorrectTotal = bastQ.CorrectTotal,
                            Id = bastQ.Id,
                            Percent = bastQ.CorrectTotal != 0 ? Math.Round((double)bastQ.CorrectCount * 100 / bastQ.CorrectTotal, 0) : default,
                            CreatedDate = bastQ.CreatedDate,
                            Status = bastQ.Status,
                            StudentId = bastQ.StudentId,
                            Score = bastQ.CorrectCount,
                            UpdatedDate = bastQ.UpdatedDate,
                        };

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Percent).ThenBy(x => x.WorkingTime)
                                   .ApplyPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result;

            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                item.IsCurrentStudent = item.Id == videoTimeCodeResult.StudentId;
                item.FullName = student?.Human?.FullName;
                item.AvatarPath = student?.Human?.AvatarPath;
            }
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> GetRankingToTimeCode(MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult, GetVideoTimeCodeRankingQuery request, EnumTimeCodeType type, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.LessonResultId);
            if (type == EnumTimeCodeType.Standalone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumTimeCodeType.Standalone));
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId.Value);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var query = from bastQ in _videoTimeCodeResultRepository.Queryable
                        where bastQ.VideoTimeCode!.VideoId == videoResult.VideoId &&
                              bastQ.VideoTimeCode.TimeCodeType == type &&
                              bastQ.VideoResult!.Status == EnumResultStatus.Done &&
                              bastQ.VideoResult.LessonResult!.CourseId == lessonResult.CourseId &&
                              bastQ.VideoResult.LessonResult.UnitId == lessonResult.UnitId &&
                              bastQ.VideoResult.LessonResult.LessonId == lessonResult.LessonId
                        group bastQ by new { bastQ.StudentId, bastQ.VideoResultId } into g
                        select new TestResultRankingModel
                        {
                            CorrectCount = g.Sum(x => x.CorrectCount),
                            CorrectTotal = g.Sum(x => x.CorrectTotal),
                            Score = g.Sum(x => x.CorrectCount),
                            StudentId = g.Key.StudentId,
                            Id = g.Key.VideoResultId,
                            WorkingTime = g.Sum(x => x.WorkingTime),
                            Percent = g.Sum(x => x.CorrectTotal) != 0 ? Math.Round((double)g.Sum(x => x.CorrectCount) * 100 / g.Sum(x => x.CorrectTotal), 0) : default,
                        };

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Percent).ThenBy(x => x.WorkingTime)
                                   .ApplyPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result;

            var videoResults = await _videoResultRepository.Queryable.WhereBulkContains(lists.Select(y => y.Id), x => x.Id).ToListAsync(cancellationToken: cancellationToken);

            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                var videoResultStudent = videoResults.FirstOrDefault(x => x.Id == item.Id);
                item.Status = videoResultStudent?.Status ?? default;
                item.IsCurrentStudent = student?.Id == videoResult.StudentId;
                item.FullName = student?.Human?.FullName;
                item.Percent = NumberHelper.ConvertRound(item.Percent);
                item.AvatarPath = student?.Human?.AvatarPath;
            }
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
