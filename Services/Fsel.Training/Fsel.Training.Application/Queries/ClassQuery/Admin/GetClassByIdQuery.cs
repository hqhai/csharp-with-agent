// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.ClassQuery.Admin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetClassByIdQuery : IRequest<MethodResult<ClassModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public GetClassByIdQueryHandler(IClassRepository classRepository, IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();

            var classes = await _classRepository.GetByIdAsync(request.Id);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classes));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<ClassModel>(classes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
