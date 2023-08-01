// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.Admins
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByCSOIdsQuery : IRequest<MethodResult<IList<UserClassModel>>>
    {
        public IList<Guid>? TeacherIds { get; set; }
    }

    public class GetClassByCSOIdsQueryHandler : IRequestHandler<GetClassByCSOIdsQuery, MethodResult<IList<UserClassModel>>>
    {
        private readonly IClassRepository _classRepository;

        public GetClassByCSOIdsQueryHandler(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        public async Task<MethodResult<IList<UserClassModel>>> Handle(GetClassByCSOIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<UserClassModel>> methodResult = new MethodResult<IList<UserClassModel>>();

            if (request.TeacherIds == null || request.TeacherIds.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var classStudents = await _classRepository.Queryable
                                            .Where(c => c.TeacherId != null && request.TeacherIds!.Contains(c.TeacherId ?? default))
                                            .GroupBy(c => c.TeacherId)
                                            .Select(g => new UserClassModel
                                            {
                                                Id = g.Key ?? default,
                                                TotalClass = g.Count()
                                            })
                                            .ToListAsync(cancellationToken);

            methodResult.Result = classStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
