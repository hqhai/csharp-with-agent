// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Application.Queries.ClassQuery
{
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Class.Doman.Enums;
    using Fsel.Class.Doman.Enums.ErrorCodes;
    using Fsel.Class.Doman.IRepositories;
    using Fsel.Class.Doman.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassByStatusNewQuery : IRequest<MethodResult<IList<ClassModel>>>
    {
    }

    public class GetTeacherByIdQueryHandler : IRequestHandler<GetClassByStatusNewQuery, MethodResult<IList<ClassModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IClassRepository _classRepository;

        public GetTeacherByIdQueryHandler(IMapper mapper, IClassRepository classRepository)
        {
            _mapper = mapper;
            _classRepository = classRepository;
        }

        public async Task<MethodResult<IList<ClassModel>>> Handle(GetClassByStatusNewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<ClassModel>> methodResult = new MethodResult<IList<ClassModel>>();

            var classNew = await _classRepository.Queryable.Where(e => e.Status == EnumClassType.New)
                                                            .ToListAsync(cancellationToken: cancellationToken);
            if (classNew.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<ClassModel>>(classNew);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
