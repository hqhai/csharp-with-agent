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
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
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
        private readonly ICourseResultRepository _courseResultRepository;

        public GetVideoTimeCodeRankingQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, ILessonResultRepository lessonResultRepository, IVideoResultRepository videoResultRepository, IMapper mapper, IUserService userService, ICourseResultRepository courseResultRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
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
            List<TestResultRankingModel> testResultRankings = new List<TestResultRankingModel>();
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

            var (students, count) = await GetStudents(lessonResult.CourseId, request);
            if (students == null)
            {
                return methodResult;
            }

            var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable
                            .Include(x => x.VideoResult)
                            .ThenInclude(x => x.LessonResult)
                            .Where(x => x.VideoTimeCodeId == videoTimeCodeResult.VideoTimeCodeId && x.Status == EnumResultStatus.Done && students.Select(x => x.Id).Contains(x.StudentId))
                            .Where(x => x.VideoResult != null && x.VideoResult.LessonResult != null && x.VideoResult.LessonResult.UnitId == lessonResult.UnitId && x.VideoResult.LessonResult.CourseId == lessonResult.CourseId)
                            .ToListAsync(cancellationToken);

            foreach (var item in students)
            {
                var videoTimeCodeResultStudent = videoTimeCodeResults.FirstOrDefault(x => x.StudentId == item.Id);
                var videoTimeCodeResultDto = _mapper.Map<TestResultRankingModel>(videoTimeCodeResultStudent);
                if (videoTimeCodeResultStudent == null)
                {
                    videoTimeCodeResultDto = new TestResultRankingModel();
                }
                videoTimeCodeResultDto.IsCurrentStudent = item.Id == videoTimeCodeResult.StudentId;
                videoTimeCodeResultDto.FullName = item.Human?.FullName;
                videoTimeCodeResultDto.AvatarPath = item.Human?.AvatarPath;
                testResultRankings.Add(videoTimeCodeResultDto);
            }
            int totalItem = count;
            testResultRankings = testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<(IList<StudentModel>?, int count)> GetStudents(Guid courseId, GetVideoTimeCodeRankingQuery request)
        {
            var currentClass = await _courseResultRepository.Queryable.Where(x => x.CourseId == courseId).Select(x => x.StudentId).ToListAsync();
            var classStudentIds = currentClass.Distinct().ToList();
            if (classStudentIds == null)
            {
                return default;
            }

            var paging = classStudentIds.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(paging);
            var students = studentResults?.Content?.Result;
            return (students, classStudentIds.Count);
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
            var (students, count) = await GetStudents(lessonResult.CourseId, request);
            if (students == null)
            {
                return methodResult;
            }
            foreach (var item in students)
            {
                var videoTimeCodeResultStudent = await _videoTimeCodeResultRepository.Queryable
                                        .Include(x => x.VideoResult)
                                        .ThenInclude(x => x!.LessonResult)
                                        .Where(x => x.VideoTimeCode != null && x.VideoTimeCode.VideoId == videoResult.VideoId && x.VideoTimeCode.TimeCodeType == type)
                                        .Where(x => x.StudentId == item.Id && x.Status == EnumResultStatus.Done)
                                        .Where(x => x.VideoResult != null && x.VideoResult.LessonResult != null && x.VideoResult.LessonResult.UnitId == lessonResult.UnitId && x.VideoResult.LessonResult.CourseId == lessonResult.CourseId)
                                        .GroupBy(x => x.StudentId)
                                        .Select(x => new TestResultRankingModel
                                        {
                                            CorrectCount = x.Sum(x => x.CorrectCount),
                                            CorrectTotal = x.Sum(x => x.CorrectTotal),
                                            Score = x.Sum(x => x.CorrectCount),
                                            StudentId = x.Key,
                                            WorkingTime = x.Sum(x => x.WorkingTime),
                                            Percent = NumberHelper.GetPercent(x.Sum(x => x.CorrectCount), x.Sum(x => x.CorrectTotal)),
                                            Status = x.Select(x => x.VideoResult).Select(x => x!.Status).FirstOrDefault(),
                                        })
                                        .FirstOrDefaultAsync(cancellationToken);

                if (videoTimeCodeResultStudent == null)
                {
                    videoTimeCodeResultStudent = new TestResultRankingModel();
                }
                videoTimeCodeResultStudent.IsCurrentStudent = item.Id == videoResult.StudentId;
                videoTimeCodeResultStudent.FullName = item.Human?.FullName;
                videoTimeCodeResultStudent.AvatarPath = item.Human?.AvatarPath;
                testResultRankings.Add(videoTimeCodeResultStudent);
            }

            int totalItem = count;
            testResultRankings = testResultRankings.OrderByDescending(x => x.Status).ThenByDescending(x => x.Percent).ThenBy(x => x.FullName).ToList();
            methodResult.Result = new PagingItemsModel<TestResultRankingModel>(testResultRankings, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
