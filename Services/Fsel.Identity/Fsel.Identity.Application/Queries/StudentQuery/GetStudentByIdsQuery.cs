// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MassTransit;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByIdsQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetStudentByIdsQueryHandler : IRequestHandler<GetStudentByIdsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentByIdsQueryHandler(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var students = await _studentRepository.Queryable
                    .Include(u => u.User)
                    .Include(pr => pr.ParentStudents)
                        .ThenInclude(p => p.Parent)
                        .ThenInclude(hm => hm.User)
                    .WhereBulkContains(request.StudentIds, i => i.Id)
                    .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
