// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Linq;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ToolUpdateVideoTimeCodeCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ToolUpdateVideoTimeCodeCommandHandler : IRequestHandler<ToolUpdateVideoTimeCodeCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;

        public ToolUpdateVideoTimeCodeCommandHandler(
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ISystemService systemService,
            ILessonResultRepository lessonResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolUpdateVideoTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<bool>();
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable
                .Include(x => x.VideoTimeCode)
                .Include(x => x.VideoResult)
                .Where(x => x.Status == EnumResultStatus.Done)
                .ToListAsync(cancellationToken);

            var lessonResultIds = videoTimeCodeResults.Where(x => x.VideoResult != null).Select(x => x.VideoResult!.LessonResultId).ToList();

            var lessonResults = await _lessonResultRepository.Queryable.Include(x => x.Course).Where(x => lessonResultIds.Contains(x.Id)).ToListAsync(cancellationToken);
            int batchSize = 2000;
            var batches = videoTimeCodeResults
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.value).ToList())
                .ToList();
            List<VideoTimeCodeAnswer> listVideoTimeCodeAnswer = new List<VideoTimeCodeAnswer>();
            foreach (var batch in batches)
            {
                var batchIds = batch.Select(v => v.Id).ToList();
                var batchResults = await _videoTimeCodeAnswerRepository
                                        .Queryable
                                        .Where(x => x.VideoTimeCodeResultId.HasValue && batchIds.Contains(x.VideoTimeCodeResultId.Value) && x.TokenReceived == 0)
                                        .ToListAsync(cancellationToken);
                listVideoTimeCodeAnswer.AddRange(batchResults);
            }

            foreach (var batch in batches)
            {
                List<VideoTimeCodeAnswer> listVideoTimeCodeAnswerTemp = new List<VideoTimeCodeAnswer>();
                foreach (var videoTimeCodeResult in batch)
                {
                    if (videoTimeCodeResult == null || videoTimeCodeResult.VideoResult == null || videoTimeCodeResult.VideoTimeCode == null)
                    {
                        return methodResult;
                    }
                    var lessonResult = lessonResults.FirstOrDefault(x => x.Id == videoTimeCodeResult.VideoResult.LessonResultId);
                    if (lessonResult == null || lessonResult.Course == null)
                    {
                        return methodResult;
                    }
                    var videoTimeCodeAnswers = listVideoTimeCodeAnswer.Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id).ToList();
                    EnumCourseType courseType = lessonResult.Course.CourseType;
                    var tokenConfigs = await GetTokenConfigsAsync(videoTimeCodeResult.VideoTimeCode, courseType);
                    foreach (var videoTimeCodeAnswer in videoTimeCodeAnswers)
                    {
                        var mission = GetTokenMission(tokenConfigs, videoTimeCodeResult.VideoTimeCode.TimeCodeType, videoTimeCodeAnswer.IsFirstSubmit);
                        var tokenConfig = tokenConfigs.FirstOrDefault(x => x.Mission == mission);
                        var token = tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
                        videoTimeCodeAnswer.TokenReceived = (int)token * videoTimeCodeAnswer.CorrectCount;
                    }
                    listVideoTimeCodeAnswerTemp.AddRange(videoTimeCodeAnswers);
                }
                _videoTimeCodeAnswerRepository.UpdateList(listVideoTimeCodeAnswerTemp);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            return methodResult;
        }

        private static EnumTokenMission GetTokenMission(IList<TokenConfigModel> tokenConfigs, EnumTimeCodeType type, bool isFirstSubmit)
        {
            ArgumentNullException.ThrowIfNull(tokenConfigs);
            if (type == EnumTimeCodeType.Standalone)
            {
                return isFirstSubmit ? EnumTokenMission.TimeCodeFirstSubmit : EnumTokenMission.TimeCodeSecondSubmit;
            }
            return type == EnumTimeCodeType.UnitTest ? EnumTokenMission.UnitTest : EnumTokenMission.SkillTest;
        }

        private async Task<IList<TokenConfigModel>> GetTokenConfigsAsync(VideoTimeCode videoTimeCode, EnumCourseType courseType)
        {
            var getTokenQuery = new GetTokenConfigsQueryModel
            {
                CourseType = courseType
            };

            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                getTokenQuery.Feature = EnumTokenFeature.Learn;
                getTokenQuery.Missions = string.Join(",", new List<EnumTokenMission> { EnumTokenMission.TimeCodeFirstSubmit, EnumTokenMission.TimeCodeSecondSubmit });
            }
            else
            {
                getTokenQuery.Feature = EnumTokenFeature.Test;
                getTokenQuery.Missions = videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? EnumTokenMission.UnitTest.ToString() : EnumTokenMission.SkillTest.ToString();
            }
            var tokenConfigResults = await _systemService.GetTokenConfigsAsync(getTokenQuery);
            if (!tokenConfigResults.IsSuccessStatusCode)
            {
                return new List<TokenConfigModel>();
            }
            var tokenConfigs = tokenConfigResults?.Content?.Result ?? new List<TokenConfigModel>();
            return tokenConfigs;
        }
    }
}
