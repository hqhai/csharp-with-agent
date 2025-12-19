namespace Fsel.Hangfire.Application.Workers
{
    using Fsel.Core.Base;
    using Fsel.Hangfire.Application.Queues.Publishers;
    using Microsoft.AspNetCore.Http;

    public class ChooseDailyQuizWinnersWorker : BaseWorker
    {
        private readonly ChooseDailyQuizWinnersPublisher _chooseDailyQuizWinnersPublisher;

        public ChooseDailyQuizWinnersWorker(ChooseDailyQuizWinnersPublisher chooseDailyQuizWinnersPublisher, AuthContext authContext, IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _chooseDailyQuizWinnersPublisher = chooseDailyQuizWinnersPublisher;
        }

        public override async Task RunAsync()
        {
            await _chooseDailyQuizWinnersPublisher.Publish(CancellationToken.None);
        }
    }
}
