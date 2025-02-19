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
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;

        public GetVideoTimeCodeRankingQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IMapper mapper,
            IUserService userService,
            IVideoTimeCodeRepository videoTimeCodeRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
            _userService = userService;
            _videoTimeCodeRepository = videoTimeCodeRepository;
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
                        join vr in _videoResultRepository.Queryable on bastQ.VideoResultId equals vr.Id
                        join lr in _lessonResultRepository.Queryable on vr.LessonResultId equals lr.Id
                        where bastQ.VideoTimeCodeId == videoTimeCodeResult.VideoTimeCodeId && bastQ.Status == EnumResultStatus.Done &&
                        lr.CourseId == lessonResult.CourseId && lr.UnitId == lessonResult.UnitId
                        select bastQ;

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent)
                                   .ApplySort(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);
            var studentIds = lists.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults?.Content?.Result;

            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();
            foreach (var item in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == item.StudentId);
                var videoTimeCodeResultDto = _mapper.Map<TestResultRankingModel>(item);

                videoTimeCodeResultDto.IsCurrentStudent = item.Id == videoTimeCodeResult.StudentId;
                videoTimeCodeResultDto.FullName = student?.Human?.FullName;
                videoTimeCodeResultDto.AvatarPath = student?.Human?.AvatarPath;
                testResultRankings.Add(videoTimeCodeResultDto);
            }
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList(), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<PagingItemsModel<TestResultRankingModel>>> GetRankingToTimeCode(MethodResult<PagingItemsModel<TestResultRankingModel>> methodResult, GetVideoTimeCodeRankingQuery request, EnumTimeCodeType type, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.LessonResultId);
            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();
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
                        join vtc in _videoTimeCodeRepository.Queryable on bastQ.VideoTimeCodeId equals vtc.Id
                        join vr in _videoResultRepository.Queryable on bastQ.VideoResultId equals vr.Id
                        join lr in _lessonResultRepository.Queryable on vr.LessonResultId equals lr.Id
                        where vtc.VideoId == videoResult.VideoId && vtc.TimeCodeType == type && vr.Status == EnumResultStatus.Done &&
                        lr.CourseId == lessonResult.CourseId && lr.UnitId == lessonResult.UnitId
                        group bastQ by new { bastQ.StudentId, bastQ.VideoResultId } into g
                        select new TestResultRankingModel
                        {
                            CorrectCount = g.Sum(x => x.CorrectCount),
                            CorrectTotal = g.Sum(x => x.CorrectTotal),
                            Score = g.Sum(x => x.CorrectCount),
                            StudentId = g.Key.StudentId,
                            Id = g.Key.VideoResultId,
                            WorkingTime = g.Sum(x => x.WorkingTime),
                            Percent = g.Sum(x => x.CorrectTotal) != 0 ? (g.Sum(x => x.CorrectCount) / g.Sum(x => x.CorrectTotal)) : default,
                            Status = g.Select(x => x.VideoResult).Select(x => x!.Status).FirstOrDefault(),
                        };

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent)
                                   .ApplySort(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            var studentIds = lists.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults?.Content?.Result;

            foreach (var videoTimeCodeResultStudent in lists)
            {
                var student = students?.FirstOrDefault(x => x.Id == videoTimeCodeResultStudent.StudentId);
                videoTimeCodeResultStudent.IsCurrentStudent = student?.Id == videoResult.StudentId;
                videoTimeCodeResultStudent.FullName = student?.Human?.FullName;
                videoTimeCodeResultStudent.Percent = NumberHelper.ConvertRound(videoTimeCodeResultStudent.Percent);
                videoTimeCodeResultStudent.AvatarPath = student?.Human?.AvatarPath;
                testResultRankings.Add(videoTimeCodeResultStudent);
            }
            testResultRankings = testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
