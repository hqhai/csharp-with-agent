// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetStudentBySchoolIdQuery : IRequest<MethodResult<IList<StudentReportDashboardModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetStudentBySchoolIdQueryHandler : IRequestHandler<GetStudentBySchoolIdQuery, MethodResult<IList<StudentReportDashboardModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        public GetStudentBySchoolIdQueryHandler(IMapper mapper, IStudentRepository studentRepository, ISystemService systemService)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
            _systemService = systemService;
        }
        public async Task<MethodResult<IList<StudentReportDashboardModel>>> Handle(GetStudentBySchoolIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentReportDashboardModel>>();
            var students = _studentRepository.Queryable.Where(x => x.SchoolId == request.Id).ToList();
            methodResult.Result = _mapper.Map<IList<StudentReportDashboardModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
