// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.UrBoxQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using MediatR;

    public class GetListCategoryQuery : IRequest<MethodResult<IList<CategoryModel>>>
    {
        public int? ParentId { get; set; }
    }

    public class GetListCategoryQueryHandler : IRequestHandler<GetListCategoryQuery, MethodResult<IList<CategoryModel>>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _languageContext;

        public GetListCategoryQueryHandler(IUrBoxService urBoxService, AppSetting appSetting, AuthContext languageContext)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _languageContext = languageContext;
        }

        public async Task<MethodResult<IList<CategoryModel>>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CategoryModel>>();

            var categoriesResult = await _urBoxService.GetListCategory(new GetListCategoryQueryModel(_appSetting)
            {
                ParentId = request.ParentId,
                Language = _languageContext.CurrentCountryInfo?.CultureCode?.Substring(0, 2),
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
