// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.NotificationServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.NotificationService.Models;
    using Fsel.Course.Lms.Application.Services.NotificationServices.Models;
    using Refit;

    public interface INotificationService
    {
        [Post("/notifications/remind-by-status")]
        Task<IApiResponse<MethodResult<IList<NotificationRemindModel>>>> GetListNotificationRemind([Body] GetListNotificationRemindQuery query);
    }
}
