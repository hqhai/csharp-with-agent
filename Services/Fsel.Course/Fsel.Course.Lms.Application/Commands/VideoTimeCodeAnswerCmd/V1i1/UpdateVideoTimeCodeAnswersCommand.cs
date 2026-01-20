// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1
{
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

    public class UpdateVideoTimeCodeAnswersCommand : IRequest<MethodResult<bool>>
    {
        public Guid VideoTimeCodeResultId { get; set; }
    }

    public class UpdateVideoTimeCodeAnswersCommandHandler : IRequestHandler<UpdateVideoTimeCodeAnswersCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ISystemService _systemService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;

        public UpdateVideoTimeCodeAnswersCommandHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            ISystemService systemService,
            ILessonResultRepository lessonResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _systemService = systemService;
            _lessonResultRepository = lessonResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateVideoTimeCodeAnswersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.ReadQueryable.Include(x => x.VideoTimeCode)
                                                                          .Include(x => x.VideoResult)
                                                                          .FirstOrDefaultAsync(x => x.Id == request.VideoTimeCodeResultId, cancellationToken);
            if (videoTimeCodeResult == null || videoTimeCodeResult.VideoResult == null || videoTimeCodeResult.VideoTimeCode == null)
            {
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.ReadQueryable.Include(x => x.Course)
                                        .FirstOrDefaultAsync(x => x.Id == videoTimeCodeResult.VideoResult.LessonResultId, cancellationToken);
            if (lessonResult == null || lessonResult.Course == null)
            {
                return methodResult;
            }
            var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.VideoTimeCodeResultId == request.VideoTimeCodeResultId)
                                                                           .ToListAsync(cancellationToken);

            EnumCourseType courseType = lessonResult.Course.CourseType;
            var tokenConfigs = await GetTokenConfigsAsync(videoTimeCodeResult.VideoTimeCode, courseType);

            var isFirstSubmit = videoTimeCodeResult.VideoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.New;

            foreach (var videoTimeCodeAnswer in videoTimeCodeAnswers)
            {
                var mission = GetTokenMission(tokenConfigs, videoTimeCodeResult.VideoTimeCode.TimeCodeType, videoTimeCodeAnswer.IsFirstSubmit);
                var tokenConfig = tokenConfigs.FirstOrDefault(x => x.Mission == mission);
                var token = tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
                videoTimeCodeAnswer.TokenReceived = (int)token * videoTimeCodeAnswer.CorrectCount;
                if (videoTimeCodeAnswer.Status != EnumAnswerStatus.Done)
                {
                    videoTimeCodeAnswer.IsFirstSubmit = isFirstSubmit;
                }
            }
            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoTimeCodeAnswerRepository.BulkUpdateList(videoTimeCodeAnswers, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.TokenReceived, entity.IsFirstSubmit };
                });
                methodResult.Result = true;
                return methodResult;
            });

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
