namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Hangfire.Application.Queues.Publishers;

    public class AggregateDataStudentsInEventWorker : IWorker
    {
        private readonly AggregateDataStudentsInEventPublisher _aggregateDataStudentsInEventPublisher;

        public AggregateDataStudentsInEventWorker(AggregateDataStudentsInEventPublisher aggregateDataStudentsInEventPublisher)
        {
            _aggregateDataStudentsInEventPublisher = aggregateDataStudentsInEventPublisher;
        }

        public async Task RunAsync()
        {
            await _aggregateDataStudentsInEventPublisher.Publish(CancellationToken.None);
        }
    }
}
