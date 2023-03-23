// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.StudentServices;
    using Fsel.Course.Lms.Application.Services.StudentServices.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByCourseQuery : IRequest<MethodResult<CourseModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetUnitByCourseQueryHandler : IRequestHandler<GetUnitByCourseQuery, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _studentService;

        public GetUnitByCourseQueryHandler(IMapper mapper,
            ICourseRepository courseRepository,
            IUserService studentService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _studentService = studentService;
        }

        public async Task<MethodResult<CourseModel>> Handle(GetUnitByCourseQuery request, CancellationToken cancellationToken)
        {
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var courseQuery = from i in _courseRepository.Queryable
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.Unit)
                             .Include(x => x.CourseUnitMockTests)
                             .ThenInclude(unit => unit.MockTest)
                             .Where(x => x.Id == request.Id)
                              select new CourseModel
                              {
                                  Id = i.Id,
                                  Name = i.Name,
                                  CourseLevel = i.CourseLevel,
                                  CourseUnitMockTests = _mapper.Map<IList<CourseUnitMockTestModel>>(i.CourseUnitMockTests),
                                  CourseClasses = _mapper.Map<IList<CourseClassModel>>(i.CourseClasses),
                              };
            /* var students = await _studentService.GetStudentByIdsAsync(new GetStudentByIdQueryModel { Ids = courseQuery.Select(x => x.) });*/
            var course = courseQuery.FirstOrDefault();
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotExist),
                                                nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = course;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
