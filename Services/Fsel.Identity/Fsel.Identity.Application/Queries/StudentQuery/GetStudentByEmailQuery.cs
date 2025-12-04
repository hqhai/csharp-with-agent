// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
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
        private readonly UserManager<User> _userManager;

        public GetStudentByEmailQueryHandler(IStudentRepository studentRepository, IMapper mapper, UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var query = await (from u in _userManager.Users
                               join s in _studentRepository.Queryable on u.Id equals s.UserId
                               where u.UserName == request.Email || u.Email == request.Email
                               select new { User = u, Student = s }).FirstOrDefaultAsync(cancellationToken);
            if (query != null)
            {
                var student = _mapper.Map<StudentModel>(query.Student);
                student.User = _mapper.Map<UserModel>(query.User);
                methodResult.Result = student;
            }

            return methodResult;
        }
    }
}
