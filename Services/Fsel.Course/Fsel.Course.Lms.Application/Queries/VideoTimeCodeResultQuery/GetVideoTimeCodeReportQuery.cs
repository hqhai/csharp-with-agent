// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid? VideoTimeCodeResultId { get; set; }
        public Guid? LessonResultId { get; set; }
        public EnumTimeCodeType? Type { get; set; }
    }

    public class GetVideoTimeCodeReportQueryHandler : IRequestHandler<GetVideoTimeCodeReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMapper _mapper;

        public GetVideoTimeCodeReportQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IVideoResultRepository videoResultRepository, IMapper mapper)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetVideoTimeCodeReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestResultReportModel> methodResult = new MethodResult<TestResultReportModel>();
            if (request.VideoTimeCodeResultId.HasValue)
            {
                methodResult = await HandleTimeCodeToStudent(methodResult, request.VideoTimeCodeResultId.Value);
            }
            else if (request.LessonResultId.HasValue && request.Type.HasValue)
            {
                methodResult = await HandleGeneralTimeCodeToStudent(methodResult, request.LessonResultId.Value, request.Type.Value);
            }
            return methodResult;
        }

        public async Task<MethodResult<TestResultReportModel>> HandleTimeCodeToStudent(MethodResult<TestResultReportModel> methodResult, Guid videoTimeCodeResultId)
        {
            ArgumentNullException.ThrowIfNull(methodResult);
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.GetByIdAsync(videoTimeCodeResultId);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.Result = _mapper.Map<TestResultReportModel>(videoTimeCodeResult);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<MethodResult<TestResultReportModel>> HandleGeneralTimeCodeToStudent(MethodResult<TestResultReportModel> methodResult, Guid lessonResultId, EnumTimeCodeType type)
        {
            ArgumentNullException.ThrowIfNull(methodResult);
            if (type == EnumTimeCodeType.Standalone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumTimeCodeType.Standalone));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            if (videoResult.Status == EnumResultStatus.Done)
            {
                var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable
                                      .Where(x => x.VideoTimeCode != null && x.VideoTimeCode.TimeCodeType == type && x.VideoResultId == videoResult.Id)
                                      .Where(x => x.CreatedDate >= videoResult.CreatedDate && x.CreatedDate <= videoResult.UpdatedDate)
                                      .ToListAsync();

                if (videoTimeCodeResults == null || !videoTimeCodeResults.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResults));
                    return methodResult;
                }
                methodResult.Result = GetTestResultReport(videoTimeCodeResults);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static TestResultReportModel? GetTestResultReport(IList<VideoTimeCodeResult>? videoTimeCodeResults)
        {
            if (videoTimeCodeResults == null)
            {
                return null;
            }
            return videoTimeCodeResults.GroupBy(x => x.StudentId).Select(x => new TestResultReportModel
            {
                CorrectCount = x.Sum(x => x.CorrectCount),
                CorrectTotal = x.Sum(x => x.CorrectTotal),
                StudentId = x.Key,
                HighestStreak = x.Max(x => x.HighestStreak),
                SkillScores = x.Where(x => x.SkillScores != null).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => new SkillScores
                {
                    Skill = x.Key,
                    CorrectCount = x.Sum(x => x.CorrectCount),
                    TotalCount = x.Sum(x => x.TotalCount),
                    CountQuestion = x.Sum(x => x.CountQuestion),
                    TotalQuestion = x.Sum(x => x.TotalQuestion),
                    Scores = x.Average(x => x.Scores)
                }).ToList(),
                Percent = NumberHelper.GetPercent(x.Sum(x => x.CorrectCount), x.Sum(x => x.CorrectTotal)),
                WorkingTime = x.Sum(x => x.WorkingTime),
                Status = x.Select(x => x.VideoResult).Select(x => x!.Status).FirstOrDefault(),
            }).FirstOrDefault();
        }
    }
}
