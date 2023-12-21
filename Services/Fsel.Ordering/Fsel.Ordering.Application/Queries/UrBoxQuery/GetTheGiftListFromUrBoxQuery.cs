// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetTheGiftListFromUrBoxQuery : IRequest<MethodResult<UrBoxModel>>
    {
        public int? CategoryId { get; set; }
    }

    public class GetTheGiftListFromUrBoxQueryHandler : IRequestHandler<GetTheGiftListFromUrBoxQuery, MethodResult<UrBoxModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetTheGiftListFromUrBoxQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<UrBoxModel>> Handle(GetTheGiftListFromUrBoxQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UrBoxModel>();

            var theGiftListResult = await _urBoxService.GetList(new GetTheGiftListFromUrBoxQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId ?? 20,
                CatId = request.CategoryId
            });
            if (!theGiftListResult.IsSuccessStatusCode)
            {
                methodResult.AddError(theGiftListResult.Error);
                return methodResult;
            }
            methodResult.Result = theGiftListResult.Content;
            return methodResult;
        }
    }
}
