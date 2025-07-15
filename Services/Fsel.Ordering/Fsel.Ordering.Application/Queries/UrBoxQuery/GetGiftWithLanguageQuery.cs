namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetGiftWithLanguageQuery : IRequest<MethodResult<GiftDetailModel>>
    {
        public string? Id { get; set; }
        public string? Language { get; set; }
    }

    public class GetGiftWithLanguageQueryHandler : IRequestHandler<GetGiftWithLanguageQuery, MethodResult<GiftDetailModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _languageContext;

        public GetGiftWithLanguageQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext languageContext)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _languageContext = languageContext;
        }

        public async Task<MethodResult<GiftDetailModel>> Handle(GetGiftWithLanguageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<GiftDetailModel>();

            var theGiftResult = await _urBoxService.Get(new GetTheGiftQueryModel(_appSetting)
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                Id = request.Id,
                Language = request.Language,
            });

            var theGift = theGiftResult.Content;
            if (theGift?.Status != 200)
            {
                methodResult.AddErrorBadRequest(theGift?.Msg);
                return methodResult;
            }
            methodResult.Result = theGift.Data;
            return methodResult;
        }
    }
}
