// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassLiveCalenderQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Domain.Models.EntityModels;
    using Fsel.Training.Domain.Models.QueryModels;
    using MediatR;

    public class SearchClassLiveCalenderQuery : SearchClassLiveCalendarQueryModel, IRequest<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
    }

    public class SearchClassLiveCalenderQueryHandler : IRequestHandler<SearchClassLiveCalenderQuery, MethodResult<PagingItemsModel<ClassLiveCalendarModel>>>
    {
        public Task<MethodResult<PagingItemsModel<ClassLiveCalendarModel>>> Handle(SearchClassLiveCalenderQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
