// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CSOQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetCsoByIdQuery : IRequest<MethodResult<CSOModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetCsoByIdQueryHandler : IRequestHandler<GetCsoByIdQuery, MethodResult<CSOModel>>
    {
        private readonly IMapper _mapper;
        private readonly ITeacherRepository _teacherRepository;

        public GetCsoByIdQueryHandler(IMapper mapper, ITeacherRepository teacherRepository)
        {
            _mapper = mapper;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<CSOModel>> Handle(GetCsoByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<CSOModel> methodResult = new MethodResult<CSOModel>();
            var teacher = await _teacherRepository.GetByIdAsync(request.Id);
            methodResult.Result = _mapper.Map<CSOModel>(teacher);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
