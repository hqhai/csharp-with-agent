// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Queries.TeacherQuery;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentByIdsQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetStudentByIdsQueryHandler : IRequestHandler<GetStudentByIdsQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByIdsQueryHandler(IMapper mapper, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();
            if (request.Id == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var students = await _studentRepository.GetIncludeByIdAsync(request.Id);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
