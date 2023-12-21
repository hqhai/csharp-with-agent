// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;

    public class GetListBrandQuery : IRequest<MethodResult<BrandModel>>
    {
        public int? CategoryId { get; set; }
    }

    public class GetListBrandQueryHandler : IRequestHandler<GetListBrandQuery, MethodResult<BrandModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetListBrandQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<BrandModel>> Handle(GetListBrandQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<BrandModel>();

            var categoriesResult = await _urBoxService.GetListBrand(new GetListBrandQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId ?? 20,
                CategoryId = request.CategoryId
            });
            if (!categoriesResult.IsSuccessStatusCode)
            {
                methodResult.AddError(categoriesResult.Error);
                return methodResult;
            }
            methodResult.Result = categoriesResult.Content;
            return methodResult;
        }
    }
}
