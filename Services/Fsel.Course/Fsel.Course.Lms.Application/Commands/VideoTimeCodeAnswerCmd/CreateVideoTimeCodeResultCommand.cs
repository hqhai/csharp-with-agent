// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Globalization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateVideoTimeCodeResultCommand : IRequest<MethodResult<VideoTimeCodeResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateVideoTimeCodeResultCommandHandler : IRequestHandler<CreateVideoTimeCodeResultCommand, MethodResult<VideoTimeCodeResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ILogger<object> _logger;
        private readonly TechieActionPublisher _techieActionPublisher;

        public CreateVideoTimeCodeResultCommandHandler(IVideoResultRepository videoResultRepository, IVideoTimeCodeRepository videoTimeCodeRepository, IMapper mapper, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, ILogger<object> logger, TechieActionPublisher techieActionPublisher)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _logger = logger;
            _techieActionPublisher = techieActionPublisher;
        }

        public async Task<MethodResult<VideoTimeCodeResultModel>> Handle(CreateVideoTimeCodeResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeResultModel> methodResult = new MethodResult<VideoTimeCodeResultModel>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            await StreakTimeCodeCheer(request, cancellationToken);
            methodResult.Result = _mapper.Map<VideoTimeCodeResultModel>(await GetAndUpdateVideoTimeCodeResultAsync(request, videoTimeCode, videoResult));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoTimeCodeResult> GetAndUpdateVideoTimeCodeResultAsync(CreateVideoTimeCodeResultCommand request, VideoTimeCode videoTimeCode, VideoResult videoResult)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.VideoResultId == request.VideoResultId).FirstOrDefaultAsync();
            if (videoTimeCodeResult == null)
            {
                _logger.LoggerRequest(request);
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = request.VideoResultId,
                    StudentId = request.StudentId,
                    IsWorking = true,
                    Status = EnumResultStatus.New,
                    WorkingTime = default,
                    VideoTimeCodeId = request.VideoTimeCodeId,
                };
                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                await UpdateVideoResult(videoResult, request.VideoTimeCodeId).ConfigureAwait(false);
            }

            return videoTimeCodeResult;
        }

        private async Task StreakTimeCodeCheer(CreateVideoTimeCodeResultCommand request, CancellationToken cancellation)
        {
            var videoTimeCodeResults = _videoTimeCodeResultRepository.Queryable.OrderBy(x => x.CreatedDate).Where(x => x.VideoResultId == request.VideoResultId).ToList();

            var currentVideoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.VideoResultId == request.VideoResultId).FirstOrDefaultAsync(cancellation);

            if (currentVideoTimeCodeResults != null && currentVideoTimeCodeResults.CorrectCount == currentVideoTimeCodeResults.CorrectTotal)
            {
                int indexOfCurrent = videoTimeCodeResults.IndexOf(currentVideoTimeCodeResults);


                int count = videoTimeCodeResults.Take(indexOfCurrent + 1).Count(x => x.CorrectCount == x.CorrectTotal);

                if (count >= 5)
                {
                    (EnumTechieAction action, int countStreak) = NumberOfCorrectTimeCode(count);
                    StudentTechieActionModel model = new StudentTechieActionModel()
                    {
                        Config = new TechieConfig
                        {
                            Value = countStreak.ToString(CultureInfo.InvariantCulture)
                        },
                        Feature = EnumTechieFeature.Cheer,
                        Action = action
                    };

                    if (count == countStreak)
                    {
                        await _techieActionPublisher.Publish(model, cancellation);
                    }
                }
            }
        }


        private async Task UpdateVideoResult(VideoResult videoResult, Guid videoTimeCodeId)
        {
            videoResult.CurrentVideoTimeCodeId = videoTimeCodeId;
            _videoResultRepository.Update(videoResult);
            await _videoResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private static (EnumTechieAction action, int count) NumberOfCorrectTimeCode(int correctCount)
        {
            return correctCount switch
            {
                ValueSettings.TimeCodeStreak.StreakFiveTimeCode => (EnumTechieAction.StreakFiveTimeCode, ValueSettings.TimeCodeStreak.StreakFiveTimeCode),
                ValueSettings.TimeCodeStreak.StreakTenTimeCode => (EnumTechieAction.StreakTenTimeCode, ValueSettings.TimeCodeStreak.StreakTenTimeCode),
                ValueSettings.TimeCodeStreak.StreakFifTeenTimeCode => (EnumTechieAction.StreakFifTeenTimeCode, ValueSettings.TimeCodeStreak.StreakFifTeenTimeCode),
                ValueSettings.TimeCodeStreak.StreakTwentyTimeCode => (EnumTechieAction.StreakTwentyTimeCode, ValueSettings.TimeCodeStreak.StreakTwentyTimeCode),
                ValueSettings.TimeCodeStreak.StreakTwentyFiveTimeCode => (EnumTechieAction.StreakTwentyFiveTimeCode, ValueSettings.TimeCodeStreak.StreakTwentyFiveTimeCode),
                ValueSettings.TimeCodeStreak.StreakThirtyTimeCode => (EnumTechieAction.StreakThirtyTimeCode, ValueSettings.TimeCodeStreak.StreakThirtyTimeCode),
                ValueSettings.TimeCodeStreak.StreakThirtyFiveTimeCode => (EnumTechieAction.StreakThirtyFiveTimeCode, ValueSettings.TimeCodeStreak.StreakThirtyFiveTimeCode),
                ValueSettings.TimeCodeStreak.StreakFourtyTimeCode => (EnumTechieAction.StreakFourtyTimeCode, ValueSettings.TimeCodeStreak.StreakFourtyTimeCode),
                ValueSettings.TimeCodeStreak.StreakFourtyFiveTimeCode => (EnumTechieAction.StreakFourtyFiveTimeCode, ValueSettings.TimeCodeStreak.StreakFourtyFiveTimeCode),
                ValueSettings.TimeCodeStreak.StreakFiftyTimeCode => (EnumTechieAction.StreakFiftyTimeCode, ValueSettings.TimeCodeStreak.StreakFiftyTimeCode),
                _ => (EnumTechieAction.StreakFiveTimeCode, ValueSettings.TimeCodeStreak.StreakFiveTimeCode)
            };
        }
    }
}
