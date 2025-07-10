// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetVideoTimeCodeQueryHandler : IRequestHandler<GetVideoTimeCodeQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly AuthContext _authContext;
        private readonly VideoConverter _videoConverter;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public GetVideoTimeCodeQueryHandler(IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            VideoConverter videoConverter,
            IMapper mapper,
            DateTimeConverter dateTimeConverter,
            IUserService userService,
            QuestBoardPublisher questBoardPublisher)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _videoConverter = videoConverter;
            _mapper = mapper;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoTimeCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;

            var videoResult = await _videoResultRepository.Queryable.Where(x => x.LessonResultId == request.LessonResultId && x.VideoId == request.VideoId && x.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var video = await _videoRepository.Queryable.Include(i => i.VideoTimeCodes)
                                                        .Include(p => p.VideoSubFilePaths)
                                                        .Where(x => x.Id == request.VideoId)
                                                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            var videoModel = _mapper.Map<VideoModel>(video);
            videoModel.VideoTimeCodes = await _videoConverter.GetTimeCodes(video, videoResult);
            videoModel.VideoResult = _mapper.Map<VideoResultModel>(videoResult);
            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            #region Do QuestBoard

            if (videoResult.Status == EnumResultStatus.Done)
            {
                await DoQuestBoard(videoResult.StudentId, cancellationToken);
            }

            #endregion Do QuestBoard

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = EnumQuestBoardCategory.HistoryOfDiscovery,
                Value = 1
            }, cancellationToken);
        }
    }
}
