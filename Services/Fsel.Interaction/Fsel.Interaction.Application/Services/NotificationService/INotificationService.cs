// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.NotificationService
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Application.Services.NotificationService.Models;
    using Refit;

    public interface INotificationService
    {
        [Post("/notifications/remind-by-status")]
        Task<IApiResponse<MethodResult<IList<NotificationRemindModel>>>> GetListNotificationRemind([Body] GetListNotificationRemindCommand cmd);
    }
}
