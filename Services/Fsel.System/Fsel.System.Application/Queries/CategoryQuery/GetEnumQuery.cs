// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Common.Models;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.System.Application.Queries.CategoryQuery
{
    public class GetEnumQuery : IRequest<MethodResult<IList<EnumModel>>>
    {
        public EnumSystemSourceData? SystemSourceData { get; set; }
    }

    public class GetEnumHandler : IRequestHandler<GetEnumQuery, MethodResult<IList<EnumModel>>>
    {
        public GetEnumHandler()
        {
        }

        public async Task<MethodResult<IList<EnumModel>>> Handle(GetEnumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<EnumModel>> methodResult = new MethodResult<IList<EnumModel>>();

            switch (request.SystemSourceData)
            {
                case EnumSystemSourceData.TokenFeature:
                    methodResult.Result = ConvertHelper.EnumToListModel<EnumTokenFeature>();
                    break;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
