// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonInputThenUpdateLessionResultHandler :
        INotificationHandler<EntityChangedEvent<VideoResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetLessonInputThenUpdateLessionResultHandler(ILessonResultRepository lessonResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoRepository videoRepository,
            IHomeWorkRepository homeWorkRepository,
            IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoRepository = videoRepository;
            _homeWorkRepository = homeWorkRepository;
            _videoResultRepository = videoResultRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task Handle(EntityChangedEvent<VideoResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult)
                .Include(x => x.HomeWorkResults)
                .Include(x => x.ClassForumResults)
                .FirstOrDefaultAsync(x => x.Id == notification.Data.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.Status == EnumResultStatus.Done, cancellationToken);
                var homewordResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);
                var homewordResultCount = lessonResult.HomeWorkResults.Count;
                var classForum = await _classForumResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResult.Id && x.Status == EnumClassForumResultStatus.Graded, cancellationToken);
                if (videoResult != null && (homewordResultCount == homewordResults.Count) && classForum != null)
                {
                }
            }
        }

        //public async Task<List<SkillScores>> SkillScoreVideo(VideoResult videoResult)
        //{
        //    List<SkillScores> skillScores = new List<SkillScores>();
        //    var video = await _videoRepository.Queryable.Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
        //                            .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
        //                            .ThenInclude(x => x.Exercise)
        //                            .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
        //                            .ThenInclude(x => x.Question)
        //                            .ThenInclude(x => x!.VideoTimeCodeAnswers!.Where(x => videoResult != null && x.VideoResultId == videoResult.Id))
        //                        .Include(i => i.VideoResults.Where(x => !x.IsDeleted))
        //                        .Where(x => x.Id == videoResult.VideoId && x.VideoTimeCodes.Any(x => x.TimeCodeType == EnumTimeCodeType.Standalone))
        //                        .AsNoTracking()
        //                        .FirstOrDefaultAsync();
        //}
    }
}
