// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStudentIdQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetClassByStudentIdQueryHandler : IRequestHandler<GetClassByStudentIdQuery, MethodResult<ClassModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetClassByStudentIdQueryHandler(IMapper mapper, IClassStudentRepository classStudentRepository, IUserService userService, AuthContext authContext)
        {
            _mapper = mapper;
            _classStudentRepository = classStudentRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var classStudent = await _classStudentRepository.Queryable.Include(x => x.Class).FirstOrDefaultAsync(x => x.StudentId == student.Id && x.ClassId == student.ClassId, cancellationToken);
            methodResult.Result = _mapper.Map<ClassModel>(classStudent?.Class);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
