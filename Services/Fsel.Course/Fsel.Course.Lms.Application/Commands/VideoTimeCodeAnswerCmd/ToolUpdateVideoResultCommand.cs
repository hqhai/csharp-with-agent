// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ToolUpdateVideoResultCommand : IRequest<MethodResult<bool>>
    {

    }

    public class ToolUpdateVideoResultCommandHandler : IRequestHandler<ToolUpdateVideoResultCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly VideoConverter _videoConverter;

        public ToolUpdateVideoResultCommandHandler(
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ISystemService systemService,
            ILessonResultRepository lessonResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IVideoResultRepository videoResultRepository,
            VideoConverter videoConverter)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
            _videoConverter = videoConverter;
        }

        public async Task<MethodResult<bool>> Handle(ToolUpdateVideoResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<bool>();
            var videoResults = await _videoResultRepository.Queryable
                .Include(x => x.VideoTimeCodeResults)
                .Where(x => x.Status == EnumResultStatus.Done)
                .ToListAsync(cancellationToken);

            List<VideoResult> listVideoResult = new List<VideoResult>();

            int batchSize = 2000;
            var batches = videoResults
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.value).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                foreach (var videoResult in batch)
                {
                    listVideoResult.Add(await GetVideoResult(videoResult, cancellationToken));
                }
                _videoResultRepository.UpdateList(listVideoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<VideoResult> GetVideoResult(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var method = await _videoConverter.GetSkillScoreAndTokens(videoResult, cancellationToken);
            var skillScores = method.Item1.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
            if (skillScores != null)
            {
                videoResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                videoResult.TokenFirstTime = method.Item2;
                videoResult.TokenLastTime = method.Item3;
            }
            videoResult.Status = EnumResultStatus.Done;
            videoResult.VideoSkillScores = method.Item1;
            return videoResult;
        }
    }
}
