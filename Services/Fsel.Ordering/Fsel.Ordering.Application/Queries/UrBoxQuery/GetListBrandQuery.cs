// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

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

            var categoriesResult = await _urBoxService.GetListBrand(new GetListBrandQueryModel(_appSetting)
            {
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
