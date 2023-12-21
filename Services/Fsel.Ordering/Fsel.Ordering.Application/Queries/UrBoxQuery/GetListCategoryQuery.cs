// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetListCategoryQuery : IRequest<MethodResult<CategoryModel>>
    {
        public int? ParentId { get; set; }
        public string? Language { get; set; }
    }

    public class GetListCategoryQueryHandler : IRequestHandler<GetListCategoryQuery, MethodResult<CategoryModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetListCategoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<CategoryModel>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CategoryModel>();

            var categoriesResult = await _urBoxService.GetListCategory(new GetListCategoryQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId ?? 20,
                ParentId = request.ParentId,
                Language = request.Language,
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
