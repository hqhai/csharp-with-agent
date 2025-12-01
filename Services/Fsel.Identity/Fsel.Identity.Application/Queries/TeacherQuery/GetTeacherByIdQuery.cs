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
    using Microsoft.EntityFrameworkCore;

    public class GetTeacherByIdQuery : IRequest<MethodResult<TeacherModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, MethodResult<TeacherModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITeacherRepository _teacherRepository;

        public GetTeacherByIdQueryHandler(IMapper mapper, ITeacherRepository teacherRepository)
        {
            _mapper = mapper;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<TeacherModel>> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<TeacherModel> methodResult = new MethodResult<TeacherModel>();
            var teacher = await _teacherRepository.Queryable.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            methodResult.Result = _mapper.Map<TeacherModel>(teacher);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
