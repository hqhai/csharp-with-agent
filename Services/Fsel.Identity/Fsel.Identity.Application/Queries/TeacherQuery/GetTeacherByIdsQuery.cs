// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Identity.Application.Queries.TeacherQuery
{
    public class GetTeacherByIdsQuery : IRequest<MethodResult<IList<TeacherModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetTeacherQueryHandler : IRequestHandler<GetTeacherByIdsQuery, MethodResult<IList<TeacherModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ITeacherRepository _teacherRepository;

        public GetTeacherQueryHandler(IMapper mapper, ITeacherRepository teacherRepository)
        {
            _mapper = mapper;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<IList<TeacherModel>>> Handle(GetTeacherByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<IList<TeacherModel>> methodResult = new MethodResult<IList<TeacherModel>>();
            if (request.Ids == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var teachers = await _teacherRepository.GetIncludeByIdsAsync(request.Ids);
            if (teachers == null || teachers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumTeacherErrorCode.TeachersNotExist));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<TeacherModel>>(teachers);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
