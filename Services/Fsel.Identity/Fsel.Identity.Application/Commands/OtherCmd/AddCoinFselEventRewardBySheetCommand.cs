namespace Fsel.Identity.Application.Commands.OtherCmd
{
    using System.Data;
    using System.Globalization;
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.GoogleSheetServices;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AddCoinFselEventRewardBySheetCommand : IRequest<MethodResult<bool>>
    {
    }

    public class AddCoinFselEventRewardBySheetCommandHandler : IRequestHandler<AddCoinFselEventRewardBySheetCommand, MethodResult<bool>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly UserManager<User> _userManager;
        private readonly ISystemService _systemService;
        private const string Sheet = "Sheet1";

        public AddCoinFselEventRewardBySheetCommandHandler(AppSetting appSetting,
                                                           UserManager<User> userManager,
                                                           ISystemService systemService)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _appSetting = appSetting;
            _userManager = userManager;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(AddCoinFselEventRewardBySheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.AddCoinFselEventSpreadSheetId;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            IList<IList<object>> dataResults = _googleSheetService.ReadDataFromSheet(spreadSheetId, Sheet);
            if (dataResults == null || dataResults.Count < 2)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var headers = dataResults[0].Select(h => h.ToString()).ToList();
            IList<AddCoinFselEventReward> values = new List<AddCoinFselEventReward>();
            for (int i = 1; i < dataResults.Count; i++)
            {
                var row = dataResults[i];
                var email = row[headers.IndexOf("Email")].ToString();
                var user = await _userManager.Users.FirstOrDefaultAsync(x => email.Trim() == x.Email.Trim(), cancellationToken);
                var coin = row[headers.IndexOf("Coin")].ToString();

                if (user == null || coin == null)
                {
                    continue;
                }
                values.Add(new AddCoinFselEventReward { UserId = user.Id, Coin = int.Parse(coin, CultureInfo.CurrentCulture) });
            }

            var addCoin = await _systemService.AddCoinFselEventReward(new AddCoinFselEventRewardModel
            {
                Values = values
            });

            if (!addCoin.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
