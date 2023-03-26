// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdQuery : IRequest<MethodResult<StudentModel>>
    {
        public string? Id { get; set; }
    }

    public class GetStudentByUserIdQueryHandler : IRequestHandler<GetStudentByUserIdQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByUserIdQueryHandler(IMapper mapper, UserManager<User> userManager, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var student = await _studentRepository.Queryable
                                        .Include(i => i.Human)
                                        .FirstOrDefaultAsync(i => i.Human != null && i.Human.UserId == request.Id, cancellationToken);

            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            methodResult.Result = _mapper.Map<StudentModel>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
