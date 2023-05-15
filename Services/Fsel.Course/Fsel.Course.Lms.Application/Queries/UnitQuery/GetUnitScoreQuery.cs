// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;

    public class GetUnitScoreQuery : IRequest<MethodResult<UnitScoreModel>>
    {
        public Guid CourseId { get; set; }

        public Guid UnitId { get; set; }
    }
}
