// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentByUserIdsQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetStudentByUserIdsQueryHandler : IRequestHandler<GetStudentByUserIdsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentByUserIdsQueryHandler(IMapper mapper, IStudentRepository studentRepository)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(GetStudentByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<StudentModel>> methodResult = new MethodResult<IList<StudentModel>>();
            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var students = await _studentRepository.Queryable
                .Where(i => i.User != null)
                .WhereBulkContains(request.UserIds, i => i.UserId)
                .Select(x => new StudentModel
                {
                    Id = x.Id,
                    ClassId = x.ClassId,
                    Occupation = x.Occupation,
                    CourseLevel = x.CourseLevel,
                    CreatedDate = x.CreatedDate,
                    School = x.School,
                    SchoolId = x.SchoolId,
                    SchoolClass = x.SchoolClass,
                    SchoolGrade = x.SchoolGrade,
                    BaseCourseLevel = x.BaseCourseLevel,
                    ExpiredDate = x.ExpiredDate,
                    UserId = x.UserId,
                    User = _mapper.Map<UserModel>(x.User)
                })
                .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
