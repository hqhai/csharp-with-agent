// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.StudentTagNameQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetListStudentTagNameQuery : IRequest<MethodResult<IList<StudentTagNameModel>>>
    {
    }

    public class GetListStudentTagNameQueryHandler : IRequestHandler<GetListStudentTagNameQuery, MethodResult<IList<StudentTagNameModel>>>
    {
        private readonly IStudentTagNameRepository _studentTagNameRepository;

        public GetListStudentTagNameQueryHandler(IStudentTagNameRepository studentTagNameRepository)
        {
            _studentTagNameRepository = studentTagNameRepository;
        }

        public async Task<MethodResult<IList<StudentTagNameModel>>> Handle(GetListStudentTagNameQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentTagNameModel>>();

            var studentTagName = await _studentTagNameRepository.Queryable.Select(x => new StudentTagNameModel
            {
                Id = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                Level = x.Level,
                MaxLevelSpaceShipId = x.MaxLevelSpaceShipId,
                TagName = x.TagName,
            }).ToListAsync(cancellationToken);

            methodResult.Result = studentTagName;
            return methodResult;
        }
    }
}
