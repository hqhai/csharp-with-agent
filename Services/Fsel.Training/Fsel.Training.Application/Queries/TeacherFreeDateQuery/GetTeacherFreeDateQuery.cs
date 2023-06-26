// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeDateQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherFreeDateQuery : IRequest<MethodResult<TeacherFreeDateModel>>
    {
        public Guid TeacherFreeDateId { get; set; }
    }

    public class GetTeacherFreeDateQueryHandler : IRequestHandler<GetTeacherFreeDateQuery, MethodResult<TeacherFreeDateModel>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetTeacherFreeDateQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<TeacherFreeDateModel>> Handle(GetTeacherFreeDateQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TeacherFreeDateModel> methodResult = new MethodResult<TeacherFreeDateModel>();

            var teacher = await _userService.GetTeacherByIdAsync(_authContext.CurrentUserId);
            if (!teacher.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherFreeDateErrorCode.TeacherNotExits));
                return methodResult;
            }
            var teacherId = teacher.Content?.Result?.Id;
            var teacherFreeDate = await _teacherFreeDateRepository.Queryable.Include(x => x.TeacherFreeTimes).FirstOrDefaultAsync(x => x.TeacherId == teacherId, cancellationToken);
            methodResult.Result = _mapper.Map<TeacherFreeDateModel>(teacherFreeDate);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
