namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class ChooseDailyQuizWinnersWorker : IWorker
    {
        private readonly ChooseDailyQuizWinnersPublisher _chooseDailyQuizWinnersPublisher;

        public ChooseDailyQuizWinnersWorker(ChooseDailyQuizWinnersPublisher chooseDailyQuizWinnersPublisher)
        {
            _chooseDailyQuizWinnersPublisher = chooseDailyQuizWinnersPublisher;
        }

        public async Task RunAsync()
        {
            await _chooseDailyQuizWinnersPublisher.Publish(CancellationToken.None);
        }
    }
}
