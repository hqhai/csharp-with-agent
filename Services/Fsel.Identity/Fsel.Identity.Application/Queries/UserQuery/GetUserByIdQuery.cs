// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetUserByIdQuery : IRequest<MethodResult<UserModel>>
    {
        public Guid? Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, MethodResult<UserModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;

        public GetUserByIdQueryHandler(IMapper mapper, UserManager<User> userManager, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _userManager = userManager;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<UserModel> methodResult = new MethodResult<UserModel>();

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
            if (user == null)
            {
                return methodResult;
            }

            var result = _mapper.Map<UserModel>(user);

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == user.Id, cancellationToken);
            if (student != null)
            {
                result.CourseId = student.CourseId;
            }
            methodResult.Result = result;
            return methodResult;
        }
    }
}
