// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class InterationActionQueueModel : NotificationSendingQueueModel
    {
        public EnumInteractionActionType InterationType { get; set; }

    }
}
