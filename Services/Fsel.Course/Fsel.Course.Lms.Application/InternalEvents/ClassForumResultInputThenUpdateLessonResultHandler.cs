// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ClassForumResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<ClassForumResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public ClassForumResultInputThenUpdateLessonResultHandler(IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ICourseResultRepository courseResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IQuestionRepository questionRepository,
            IHomeWorkRepository homeWorkRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            FinishOneUnitPublisher finishOneUnitPublisher,
            FinishOneLevelPassPublisher finishOneLevelPassPublisher,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
            : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, courseRepository, unitRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var classFormResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == classFormResult.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                await GetLessonResult(lessonResult, cancellationToken);
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
