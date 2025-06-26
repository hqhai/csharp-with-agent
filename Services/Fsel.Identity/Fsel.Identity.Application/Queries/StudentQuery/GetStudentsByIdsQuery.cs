// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsByIdsQuery : IRequest<MethodResult<List<StudentModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class GetStudentsByIdsQueryHandler : IRequestHandler<GetStudentsByIdsQuery, MethodResult<List<StudentModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;

        public GetStudentsByIdsQueryHandler(IStudentRepository studentRepository, ISystemService systemService)
        {
            _studentRepository = studentRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<List<StudentModel>>> Handle(GetStudentsByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<StudentModel>>();

            if (request.Ids == null || request.Ids.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Ids));
                return methodResult;
            }
            var students = await _studentRepository.Queryable.Where(i => request.Ids.Contains(i.UserId)).Select(p => new StudentModel
            {
                Id = p.Id,
                PackageId = p.PackageId,
                Occupation = p.Occupation,
                School = p.School,
                SchoolId = p.SchoolId,
                CourseLevel = p.CourseLevel,
                BaseCourseLevel = p.BaseCourseLevel,
                ClassId = p.ClassId,
                UserId = p.UserId,
            }).ToListAsync(cancellationToken);

            //var schoolResults = await _systemService.ExecuteListSchoolQueryAsync(new BaseQueryModel
            //{
            //    Filters = new List<GenericFilterModel>() { new GenericFilterModel { Property = "Id", Operator = Common.Enums.EnumFilterOperator.Equal, Value = students.Select(x => x.SchoolId).ToList() } },
            //    IncludePaths = new List<string>() { "School" }
            //});
            //if (!schoolResults.IsSuccessStatusCode || schoolResults.Content?.Result == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
            //    return methodResult;
            //}

            //foreach (var student in students)
            //{
            //    if (student.SchoolId != null)
            //    {
            //        student.School = schoolResults.Content?.Result?.Where(x => x.Id == student.SchoolId).FirstOrDefault()?.Name;
            //    }
            //}

            methodResult.Result = students;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
