// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TokenHistorys
{
    using Fsel.Shared.Models.ShareModels;

    public class CreateTokenHistoryCommandModel
    {
        public IList<TokenHistoryQueueModel>? TokenHistorys { get; set; }
    }
}
