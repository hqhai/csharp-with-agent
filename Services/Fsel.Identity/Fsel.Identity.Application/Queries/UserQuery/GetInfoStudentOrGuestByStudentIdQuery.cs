// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetInfoStudentOrGuestByStudentIdQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetInfoStudentOrGuestByStudentIdQueryHandler : IRequestHandler<GetInfoStudentOrGuestByStudentIdQuery, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetInfoStudentOrGuestByStudentIdQueryHandler(IStudentRepository studentRepository, IMapper mapper, UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetInfoStudentOrGuestByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.Queryable.Include(x => x.User).FirstOrDefaultAsync(p => p.Id == request.StudentId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var studentModel = _mapper.Map<StudentModel>(student);

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id == student.UserId, cancellationToken);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var role = await _userManager.GetRolesAsync(user);

            studentModel.Role = role.FirstOrDefault();

            methodResult.Result = studentModel;
            return methodResult;
        }
    }
}
