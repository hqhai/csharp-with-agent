// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReviewLessonVideoCommand : ReviewLessonVideoCommandModel, IRequest<MethodResult<VideoResultModel>>
    {
    }

    public class ReviewLessonVideoCommandHandler : IRequestHandler<ReviewLessonVideoCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;

        public ReviewLessonVideoCommandHandler(IVideoResultRepository videoResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IMapper mapper,
            IVideoRepository videoRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ReviewLessonVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken: cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            _mapper.Map(request, videoResult);
            if (!videoResult.IsValid())
            {
                methodResult.AddErrorBadRequest(videoResult.ErrorMessages);
                return methodResult;
            }
            var isVideoTimeCodeDone = await _videoTimeCodeResultRepository.Queryable.AnyAsync(x => x.VideoResultId == videoResult.Id && x.Status != EnumResultStatus.Done, cancellationToken);
            if (isVideoTimeCodeDone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isVideoTimeCodeDone));
                return methodResult;
            }
            var video = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                        .ThenInclude(x => x.VideoTimeCodeResults.Where(x => x.VideoResultId == videoResult.Id))
                                                        .FirstOrDefaultAsync(x => x.Id == videoResult.VideoId, cancellationToken: cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            var videoTimeCodeResults = video.VideoTimeCodes.Select(x => x.VideoTimeCodeResults).ToList();
            if (videoTimeCodeResults.Count != video.VideoTimeCodes.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodesNotCompleted), nameof(videoTimeCodeResults));
                return methodResult;
            }
            videoResult = GetVideoResult(videoResult, video);
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                videoResult = _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
                return methodResult;
            });
            return methodResult;
        }

        private static VideoResult GetVideoResult(VideoResult videoResult, Video video)
        {
            var videoSkillScores = video.VideoTimeCodes.GroupBy(x => x.TimeCodeType)
                .Select(x => new VideoSkillScores
                {
                    Type = x.Key,
                    SkillScores = x.SelectMany(x => x.VideoTimeCodeResults).Where(x => x.SkillScores != null && x.SkillScores.Any())
                                                .SelectMany(x => x.SkillScores!)
                                                .GroupBy(x => x.Skill)
                                                .Select(x => GetSkillScore(x))
                                                .ToList()
                }).ToList();
            var skillScores = videoSkillScores.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
            if (skillScores != null)
            {
                videoResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            }
            videoResult.Status = EnumResultStatus.Done;
            videoResult.VideoSkillScores = videoSkillScores;
            return videoResult;
        }

        private static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, SkillScores>? x)
        {
            SkillScores skillScores = new SkillScores();
            skillScores.Skill = x.Key;
            skillScores.TotalQuestion = x.Sum(x => x.TotalQuestion);
            skillScores.CountQuestion = x.Sum(x => x.CountQuestion);
            skillScores.TotalCount = x.Sum(x => x.TotalCount);
            skillScores.CorrectCount = x.Sum(x => x.CorrectCount);
            return skillScores;
        }
    }
}
