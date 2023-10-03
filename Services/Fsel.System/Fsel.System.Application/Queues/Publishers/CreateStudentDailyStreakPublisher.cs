using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Domain.Entities;

namespace Fsel.System.Application.Queues.Publishers
{
    public class CreateStudentDailyStreakPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public CreateStudentDailyStreakPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(QuestBoardStudent? questBoardStudent, CancellationToken cancellationToken)
        {
            if (questBoardStudent == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.SystemQueue.NameQueue.CreateStudentDailyStreak, new CreateStudentDailyStreakQueueModel
            {
                StudentId = questBoardStudent.StudentId,
                IsUseShield = false
            }, cancellationToken);
        }
    }
}
