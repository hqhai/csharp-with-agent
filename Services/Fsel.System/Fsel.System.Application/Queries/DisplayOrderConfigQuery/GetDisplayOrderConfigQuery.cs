// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DisplayOrderConfigQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetDisplayOrderConfigQuery : IRequest<MethodResult<IList<DisplayOrderConfigModel>>>
    {
    }

    public class GetDisplayOrderConfigQueryHandler : IRequestHandler<GetDisplayOrderConfigQuery, MethodResult<IList<DisplayOrderConfigModel>>>
    {
        public async Task<MethodResult<IList<DisplayOrderConfigModel>>> Handle(GetDisplayOrderConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<DisplayOrderConfigModel>> methodResult = new MethodResult<IList<DisplayOrderConfigModel>>();

            List<DisplayOrderConfigModel> displayOrderConfigs = new List<DisplayOrderConfigModel>
            {
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 1,
                    Name = Shared.Enums.EnumDisplayOrder.Popup,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 2,
                    Name = Shared.Enums.EnumDisplayOrder.Heading,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 3,
                    Name = Shared.Enums.EnumDisplayOrder.GoodMorning,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 4,
                    Name = Shared.Enums.EnumDisplayOrder.FselCoin,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 5,
                    Name = Shared.Enums.EnumDisplayOrder.StreakTimeCode,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 6,
                    Name = Shared.Enums.EnumDisplayOrder.TwentyFiveMinutesBreak,
                    Status = true
                },
                new DisplayOrderConfigModel
                {
                    DisplayOrder = 7,
                    Name = Shared.Enums.EnumDisplayOrder.Beginner,
                    Status = true
                }
            };

            methodResult.Result = displayOrderConfigs;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
