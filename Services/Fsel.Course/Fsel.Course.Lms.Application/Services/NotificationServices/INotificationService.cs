// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.NotificationServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.NotificationServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface INotificationService
    {
        [Get("/notification/remind-by-status")]
        Task<IApiResponse<MethodResult<NotificationRemindModel>>> GetStudentByUserIdAsync([FromQuery] GetListNotificationRemindQuery query);
    }
}
