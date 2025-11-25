// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdQuery : IRequest<MethodResult<StudentModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetStudentByUserIdQueryHandler : IRequestHandler<GetStudentByUserIdQuery, MethodResult<StudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;

        public GetStudentByUserIdQueryHandler(IMapper mapper, IStudentRepository studentRepository, ISystemService systemService)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<StudentModel>> Handle(GetStudentByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();
            var student = await _studentRepository.Queryable
                                        .Include(p => p.User).ThenInclude(p => p.Receiver)
                                        .FirstOrDefaultAsync(i => i.UserId == request.Id, cancellationToken);
            //if (student?.SchoolId != null)
            //{
            //    var schoolResult = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel
            //    {
            //        Filters = new List<GenericFilterModel>() { new GenericFilterModel { Property = "Id", Operator = Common.Enums.EnumFilterOperator.Equal, Value = student.SchoolId } },
            //        IncludePaths = new List<string>() { "School" }
            //    });
            //    if (!schoolResult.IsSuccessStatusCode || schoolResult.Content?.Result == null)
            //    {
            //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
            //        return methodResult;
            //    }
            //    student.School = schoolResult.Content?.Result?.FirstOrDefault(x => x.Id == student.SchoolId)?.Name;
            //}
            methodResult.Result = _mapper.Map<StudentModel>(student);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
