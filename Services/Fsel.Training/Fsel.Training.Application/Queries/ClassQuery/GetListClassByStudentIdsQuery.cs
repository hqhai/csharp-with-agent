// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
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

    public class GetListClassByStudentIdsQuery : IRequest<MethodResult<IList<ClassStudentDetailModel>>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetListClassByStudentIdsQueryHandler : IRequestHandler<GetListClassByStudentIdsQuery, MethodResult<IList<ClassStudentDetailModel>>>
    {
        private readonly IClassStudentRepository _classStudentRepository;

        public GetListClassByStudentIdsQueryHandler(IClassStudentRepository classStudentRepository)
        {
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<IList<ClassStudentDetailModel>>> Handle(GetListClassByStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<ClassStudentDetailModel>> methodResult = new MethodResult<IList<ClassStudentDetailModel>>();
            if (request.StudentIds == null || request.StudentIds.Count == 0)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var classStudents = await _classStudentRepository.Queryable.Include(x => x.Class)
                                            .Where(e => request.StudentIds.Contains(e.StudentId))
                                            .Select(x => new ClassStudentDetailModel
                                            {
                                                StudentId = x.StudentId,
                                                Code = x.Class!.Code
                                            }).ToListAsync(cancellationToken: cancellationToken);

            methodResult.Result = classStudents;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
