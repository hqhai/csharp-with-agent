using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    public class FinishOneLevelPassPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public FinishOneLevelPassPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(CourseResult? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.QuestBoardFinishOneLevelPass, new QuestBoardStudentQueueModel
            {
                ObjectId = request.CourseId,
                StudentId = request.StudentId,
                QuestBoardType = EnumQuestBoardType.MainQuests,
                QuestBoardCategory = EnumQuestBoardCategory.FinishOneLevelPass
            }, cancellationToken);
        }
    }
}
