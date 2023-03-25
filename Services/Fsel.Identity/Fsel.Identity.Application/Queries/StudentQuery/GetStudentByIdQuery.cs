// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid id { get; set; }
    }

    public class GetStudentByUserIdQueryHandler : IRequestHandler<GetStudentByUserIdQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public GetStudentByUserIdQueryHandler(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var user = await _userManager.Users.Include(x => x.Human)
                                                    .ThenInclude(x => x.Student)
                                                    .FirstOrDefaultAsync(x => x.Id == request.id.ToString(), cancellationToken: cancellationToken);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddError(nameof(EnumStudentErrorCode.StudentNull));
                return methodResult;
            }
            var student = user.Human?.Student;
            methodResult.Result = _mapper.Map<StudentModel>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
