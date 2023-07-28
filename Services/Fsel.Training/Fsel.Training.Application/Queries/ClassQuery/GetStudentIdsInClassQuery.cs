// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentIdsInClassQuery : IRequest<MethodResult<List<Guid>?>>
    {
        public Guid ClassId { get; set; }
    }

    public class GetStudentIdsInClassQueryHandler : IRequestHandler<GetStudentIdsInClassQuery, MethodResult<List<Guid>?>>
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IClassRepository _classRepository;

        public GetStudentIdsInClassQueryHandler(IClassStudentRepository classStudentRepository, IClassRepository classRepository)
        {
            _classStudentRepository = classStudentRepository;
            _classRepository = classRepository;
        }

        public async Task<MethodResult<List<Guid>?>> Handle(GetStudentIdsInClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<Guid>?> methodResult = new MethodResult<List<Guid>?>();

            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ClassId));
                return methodResult;
            }
            var studentIds = await _classStudentRepository.Queryable.Where(p => p.ClassId == request.ClassId).Select(x => x.StudentId).ToListAsync(cancellationToken);
            methodResult.Result = studentIds;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
