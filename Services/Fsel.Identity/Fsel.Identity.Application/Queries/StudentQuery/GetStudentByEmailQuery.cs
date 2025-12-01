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
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;

        public GetStudentByEmailQueryHandler(IStudentRepository studentRepository, IMapper mapper, IHumanRepository humanRepository, UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _humanRepository = humanRepository;
            _userManager = userManager;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var query = await (from u in _userManager.Users
                               join h in _humanRepository.Queryable on u.Id equals h.UserId
                               join s in _studentRepository.Queryable on h.Id equals s.HumanId
                               where u.UserName == request.Email || u.Email == request.Email
                               select new { User = u, Human = h, Student = s }).FirstOrDefaultAsync(cancellationToken);
            if (query != null)
            {
                var student = _mapper.Map<StudentModel>(query.Student);
                student.Human = _mapper.Map<HumanProfileModel>(query.Human);
                student.Human.User = _mapper.Map<UserInformationModel>(query.User);
                methodResult.Result = student;
            }

            return methodResult;
        }
    }
}
