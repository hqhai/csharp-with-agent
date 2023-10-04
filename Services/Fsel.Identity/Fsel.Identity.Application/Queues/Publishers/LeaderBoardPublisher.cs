using Fsel.Core.Base.Interfaces;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Identity.Application.Queues.Publishers
{
    public class LeaderBoardPublisher
    {
        private readonly IQueueProvider _queueProvider;

        public LeaderBoardPublisher(IQueueProvider queueProvider)
        {
            _queueProvider = queueProvider;
        }

        public async Task Publish(LeaderBoardQueueModel leaderBoards, CancellationToken cancellationToken)
        {
            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.LeaderBoard, new LeaderBoardQueueModel
            {
                StudentRankings = leaderBoards?.StudentRankings,
                CourseLevel = leaderBoards!.CourseLevel,
            }, cancellationToken);

        }
    }
}
