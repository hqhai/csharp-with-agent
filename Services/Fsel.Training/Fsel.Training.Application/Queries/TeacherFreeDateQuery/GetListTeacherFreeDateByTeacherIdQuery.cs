// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Queries.TeacherFreeDateQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListTeacherFreeDateByTeacherIdQuery : IRequest<MethodResult<IList<TeacherFreeDateModel>>>
    {
        public Guid TeacherId { get; set; }
    }
    public class GetListTeacherFreeDateByTeacherIdQueryHandler : IRequestHandler<GetListTeacherFreeDateByTeacherIdQuery, MethodResult<IList<TeacherFreeDateModel>>>
    {
        private readonly ITeacherFreeDateRepository _teacherFreeDateRepository;
        private readonly IMapper _mapper;

        public GetListTeacherFreeDateByTeacherIdQueryHandler(ITeacherFreeDateRepository teacherFreeDateRepository, IMapper mapper)
        {
            _teacherFreeDateRepository = teacherFreeDateRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<TeacherFreeDateModel>>> Handle(GetListTeacherFreeDateByTeacherIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<TeacherFreeDateModel>> methodResult = new MethodResult<IList<TeacherFreeDateModel>>();

            var teacherFreeDate = await _teacherFreeDateRepository.Queryable
                            .Where(x => x.TeacherId == request.TeacherId)
                            .Select(x => new TeacherFreeDateModel
                            {
                                Id = x.Id,
                                StartDate = x.StartDate,
                                EndDate = x.EndDate,
                                TeacherId = x.TeacherId,
                                CreatedDate = x.CreatedDate,
                            }).ToListAsync(cancellationToken);

            methodResult.Result = teacherFreeDate;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
