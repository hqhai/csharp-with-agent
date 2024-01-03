// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetListCategoryQuery : IRequest<MethodResult<IList<CategoryModel>>>
    {
        public int? ParentId { get; set; }
        public string? Language { get; set; }
    }

    public class GetListCategoryQueryHandler : IRequestHandler<GetListCategoryQuery, MethodResult<IList<CategoryModel>>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;

        public GetListCategoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<IList<CategoryModel>>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CategoryModel>>();

            var categoriesResult = await _urBoxService.GetListCategory(new GetListCategoryQueryModel
            {
                AppSecret = _appSetting.UrBoxConfig?.AppSecret,
                AppId = _appSetting.UrBoxConfig?.AppId,
                ParentId = request.ParentId,
                Language = request.Language,
            });

            var categories = categoriesResult.Content?.Data;
            if (categories?.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            methodResult.Result = categories;
            return methodResult;
        }
    }
}
