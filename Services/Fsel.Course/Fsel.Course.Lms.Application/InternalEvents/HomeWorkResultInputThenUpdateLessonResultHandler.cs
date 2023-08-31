// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkResultInputThenUpdateLessonResultHandler :
        INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IQuestionRepository _questionRepository;

        public HomeWorkResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , IHomeWorkAnswerRepository homeWorkAnswerRepository
            , IHomeWorkRepository homeWorkRepository
            , IHomeWorkQuestionRepository homeWorkQuestionRepository
            , IQuestionRepository questionRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _questionRepository = questionRepository;
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var homeWorkResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults)
                                         .FirstOrDefaultAsync(x => x.Id == homeWorkResult.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                var skillScores = from baseQ in _lessonResultRepository.Queryable
                                  join hr in _homeWorkResultRepository.Queryable on baseQ.Id equals hr.LessonResultId
                                  join h in _homeWorkRepository.Queryable on hr.HomeWorkId equals h.Id
                                  join hq in _homeWorkQuestionRepository.Queryable on h.Id equals hq.HomeWorkId
                                  join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
                                  join ha in _homeWorkAnswerRepository.Queryable on hr.Id equals ha.HomeWorkResultId
                                  group new { h, ha, q } by h.CourseSkill into g
                                  select new SkillScores
                                  {
                                      Skill = g.Key,
                                      CorrectCount = g.Select(x => x.ha).Sum(x => x.CorrectCount),
                                      TotalCount = g.Select(x => x.q).Sum(x => x.CorrectTotal),
                                      CountQuestion = g.Select(x => x.q).Count(),
                                      TotalQuestion = g.Select(x => x.ha).Count(),
                                  };

                var percentHomeWork = (double)lessonResult.HomeWorkResults.Average(x => x.Percent) * 30;
                lessonResult.Percent = percentHomeWork;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
