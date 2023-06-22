// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Refit;

    public interface IInteractionService
    {
        [Post("/interaction-action/actions")]
        Task<IApiResponse<MethodResult<IList<InteractionActionModel>>>> GetsActionAsync([Body] InteractionActionCommandModel command);
    }
}
