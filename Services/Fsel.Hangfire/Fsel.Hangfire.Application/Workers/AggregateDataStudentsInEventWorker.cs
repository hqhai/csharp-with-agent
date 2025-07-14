namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class AggregateDataStudentsInEventWorker : BaseWorker
    {
        private readonly AggregateDataStudentsInEventPublisher _aggregateDataStudentsInEventPublisher;

        public AggregateDataStudentsInEventWorker(AggregateDataStudentsInEventPublisher aggregateDataStudentsInEventPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _aggregateDataStudentsInEventPublisher = aggregateDataStudentsInEventPublisher;
        }

        public override async Task RunAsync()
        {
            await _aggregateDataStudentsInEventPublisher.Publish(CancellationToken.None);
        }
    }
}
