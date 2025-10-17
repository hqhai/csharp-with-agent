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
        private readonly IHumanRepository _humanRepository;

        public GetStudentByUserIdsQueryHandler(IMapper mapper, IHumanRepository humanRepository)
        {
            _mapper = mapper;
            _humanRepository = humanRepository;
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

            var students = await _humanRepository.Queryable
                .Include(x => x.User)
                .Where(i => i.UserId != null)
                .Where(i => i.Student != null)
                .WhereBulkContains(request.UserIds, i => i.UserId)
                .Select(x => new StudentModel
                {
                    Id = x.Student!.Id,
                    ClassId = x.Student!.ClassId,
                    Occupation = x.Student.Occupation,
                    CourseLevel = x.Student.CourseLevel,
                    CreatedDate = x.Student.CreatedDate,
                    School = x.Student.School,
                    SchoolId = x.Student.SchoolId,
                    SchoolClass = x.Student.SchoolClass,
                    SchoolGrade = x.Student.SchoolGrade,
                    BaseCourseLevel = x.Student.BaseCourseLevel,
                    ExpiredDate = x.Student.ExpiredDate,
                    Human = _mapper.Map<HumanProfileModel>(x)
                })
                .ToListAsync(cancellationToken);

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
