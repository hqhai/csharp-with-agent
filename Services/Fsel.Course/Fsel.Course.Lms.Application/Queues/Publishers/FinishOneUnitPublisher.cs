using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    public class FinishOneUnitPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public FinishOneUnitPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(UnitResult? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.QuestBoardFinishOneUnit, new QuestBoardStudentQueueModel
            {
                ObjectId = request.Id,
                StudentId = request.StudentId,
                QuestBoardType = EnumQuestBoardType.MainQuests,
                QuestBoardCategory = EnumQuestBoardCategory.FinishOneUnit
            }, cancellationToken);
        }
    }
}
