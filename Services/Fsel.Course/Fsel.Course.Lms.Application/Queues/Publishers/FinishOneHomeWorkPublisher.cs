using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Course.Lms.Application.Queues.Publishers
{
    public class FinishOneHomeWorkPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public FinishOneHomeWorkPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(HomeWorkResult? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            //await _queueProvider.Publish(QueueSettings.LmsQueue.NameQueue.QuestBoardMainFinish, new QuestBoardStudentQueueModel
            //{
            //    StudentId = request.StudentId,
            //    QuestBoardType = EnumQuestBoardType.MainQuests,
            //    QuestBoardCategory = EnumQuestBoardCategory.FinishOneHomeworkMiniProject
            //}, cancellationToken);
        }
    }
}
