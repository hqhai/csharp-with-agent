// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
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

    public class GetStudentByUserIdsQuery : IRequest<MethodResult<IList<StudentModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class GetStudentByUserIdsQueryHandler : IRequestHandler<GetStudentByUserIdsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;

        public GetStudentByUserIdsQueryHandler(IMapper mapper, IStudentRepository studentRepository, ISystemService systemService)
        {
            _mapper = mapper;
            _studentRepository = studentRepository;
            _systemService = systemService;
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


            #region #Batch
            var students = new List<StudentModel>();
            int batchSize = 100;

            // Chia danh sách UserIds thành nhiều batch
            var userIdBatches = request.UserIds
                .Distinct() // Loại bỏ trùng lặp nếu có
                .Select((id, index) => new { id, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.id).ToList())
                .ToList();

            foreach (var batch in userIdBatches)
            {
                var batchStudents = await _studentRepository.Queryable
                    .Include(i => i.Human)
                    .Where(i => i.Human != null && i.Human.UserId.HasValue && batch.Contains(i.Human.UserId.Value))
                    .Select(x => new StudentModel
                    {
                        Id = x.Id,
                        ClassId = x.ClassId,
                        Occupation = x.Occupation,
                        CourseLevel = x.CourseLevel,
                        CreatedDate = x.CreatedDate,
                        School = x.School,
                        SchoolId = x.SchoolId,
                        Human = _mapper.Map<HumanProfileModel>(x.Human)
                    })
                    .ToListAsync(cancellationToken);

                students.AddRange(batchStudents); // Gộp kết quả vào danh sách chính
            }
            #endregion

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
            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
