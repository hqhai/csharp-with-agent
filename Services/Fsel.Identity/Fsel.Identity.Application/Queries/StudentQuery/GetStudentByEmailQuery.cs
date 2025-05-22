// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByEmailQuery : IRequest<MethodResult<StudentModel>>
    {
        public string? Email { get; set; }
    }

    public class GetStudentByEmailQueryHandler : IRequestHandler<GetStudentByEmailQuery, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public GetStudentByEmailQueryHandler(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var queryUserName = _studentRepository.Queryable
                                    .Include(x => x.User)
                                    .Where(x => x.User.UserName == request.Email);

            var queryEmail = _studentRepository.Queryable
                                    .Include(x => x.User)
                                    .Where(x => x.User.Email == request.Email);

            var student = await queryUserName.Union(queryEmail).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            methodResult.Result = _mapper.Map<StudentModel>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
