// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<IList<SectionDetailModel>>>
    {
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<IList<SectionDetailModel>>>
    {
        public GetSectionBySectionGroupIdQueryHandler()
        {
        }

        public async Task<MethodResult<IList<SectionDetailModel>>> Handle(GetSectionBySectionGroupIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SectionDetailModel>>();

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
