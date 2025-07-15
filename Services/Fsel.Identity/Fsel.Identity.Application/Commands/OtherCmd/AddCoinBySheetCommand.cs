namespace Fsel.Identity.Application.Commands.OtherCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using System.Data;
    using MediatR;
    using Fsel.Identity.Application.Services.GoogleSheetServices;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using System.Linq.Dynamic.Core;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Shared.Constants;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;

    public class AddCoinBySheetCommand : IRequest<MethodResult<bool>>
    {
        public double Coin { get; set; }
    }

    public class AddCoinBySheetCommandHandler : IRequestHandler<AddCoinBySheetCommand, MethodResult<bool>>
    {
        private readonly IGoogleSheetService _googleSheetService;
        private readonly AppSetting _appSetting;
        private readonly UserManager<User> _userManager;
        private readonly ISystemService _systemService;
        private const string Sheet = "Sheet1";
        private const string RangeError = "SheetError!A1";

        public AddCoinBySheetCommandHandler(AppSetting appSetting,
                                            UserManager<User> userManager,
                                            ISystemService systemService)
        {
            _googleSheetService = new GoogleSheetService(ResourceSettings.StudentCredentialsFilePath);
            _appSetting = appSetting;
            _userManager = userManager;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(AddCoinBySheetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var spreadSheetId = _appSetting.GoogleSheetConfig?.AddCoinSpreadSheetId;

            if (string.IsNullOrEmpty(spreadSheetId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            IList<IList<object>> data = _googleSheetService.ReadDataFromSheet(spreadSheetId, Sheet);
            var dataResults = data.SelectMany(x => x.Select(c => c.ToString()?.Trim())).ToList();
            if (dataResults == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var users = await _userManager.Users.WhereBulkContains(dataResults, x => x.UserName).ToListAsync(cancellationToken);
            var userIds = users.DistinctBy(x => x.Id).Select(x => x.Id).ToList();
            if (userIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }

            var addCoin = await _systemService.AddCoinSurveyReward(new AddCoinSurveyRewardModel
            {
                Coin = request.Coin,
                UserIds = userIds
            });
            if (!addCoin.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var userNames = users.Select(x => x.UserName).ToList();
            var exceptUserNames = dataResults.Except(userNames).Union(userNames.Except(dataResults)).ToList();
            if (exceptUserNames.Any())
            {
                var datas = new List<IList<object>>();

                exceptUserNames.ForEach(userName =>
                {
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var objectData = new List<object> { userName };
                        datas.Add(objectData);
                    }
                });

                _googleSheetService.CreateDataFromSheet(spreadSheetId, RangeError, datas);
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
