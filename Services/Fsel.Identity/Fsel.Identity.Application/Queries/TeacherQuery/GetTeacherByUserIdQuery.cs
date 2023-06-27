// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    public class GetTeacherByUserIdQuery : IRequest<MethodResult<TeacherModel>>
    {
        public Guid UserId { get; set; }
    }

    public class GetTeacherByUserIdQueryHandler : IRequestHandler<GetTeacherByUserIdQuery, MethodResult<TeacherModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITeacherRepository _teacherRepository;

        public GetTeacherByUserIdQueryHandler(IMapper mapper, ITeacherRepository teacherRepository)
        {
            _mapper = mapper;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<TeacherModel>> Handle(GetTeacherByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<TeacherModel> methodResult = new MethodResult<TeacherModel>();
            var teacher = await _teacherRepository.GetIncludeByUserIdAsync(request.UserId);
            methodResult.Result = _mapper.Map<TeacherModel>(teacher);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
