// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Publishers
{
    using System.Globalization;
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;

    public class CreateTokenHistoryPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly AuthContext _authContext;
        private readonly ICacheService<StudentModel> _cacheService;

        public CreateTokenHistoryPublisher(IQueueProvider queueProvider, AuthContext authContext, ICacheService<StudentModel> cacheService)
        {
            _queueProvider = queueProvider;
            _authContext = authContext;
            _cacheService = cacheService;
        }

        public async Task Publish(IList<TokenHistoryQueueModel>? request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return;
            }

            var key = $"GetCoinOfStudent_{_authContext.CurrentUserId}".ToLower(CultureInfo.InvariantCulture);
            await _cacheService.RemoveAsync(key);

            await _queueProvider.Publish(QueueSettings.UserQueue.NameQueue.CreateTokenHistory, new TokenHistoryQueuesModel { TokenHistories = request }, cancellationToken);
        }
    }
}
