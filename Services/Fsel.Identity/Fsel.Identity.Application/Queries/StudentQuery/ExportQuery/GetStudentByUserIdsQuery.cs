// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery.ExportQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
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
        private readonly UserManager<User> _userManager;
        private readonly IParentRepository _parentRepository;
        private readonly IParentStudentRepository _parentStudentRepository;
        private readonly ISystemService _systemService;

        public GetStudentByUserIdsQueryHandler(IMapper mapper,
            UserManager<User> userManager,
            IParentRepository parentRepository,
            IParentStudentRepository parentStudentRepository,
            ISystemService systemService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _parentRepository = parentRepository;
            _parentStudentRepository = parentStudentRepository;
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

            var students = await _userManager.Users
                .Where(i => i.Student != null)
                .WhereBulkContains(request.UserIds, i => i.Id)
                .Select(x => new StudentModel
                {
                    Id = x.Student!.Id,
                    ClassId = x.Student!.ClassId,
                    Occupation = x.Student.Occupation,
                    CourseLevel = x.Student.CourseLevel,
                    CreatedDate = x.Student.CreatedDate,
                    School = x.Student.School,
                    DistrictId = x.Student.DistrictId,
                    ProvinceId = x.Student.ProvinceId,
                    SchoolId = x.Student.SchoolId,
                    SchoolClass = x.Student.SchoolClass,
                    NumberOfToken = x.Student.NumberOfToken,
                    SchoolGrade = x.Student.SchoolGrade,
                    BaseCourseLevel = x.Student.BaseCourseLevel,
                    ExpiredDate = x.Student.ExpiredDate,
                    ParentEmail = x.Student.ParentEmail,
                    ParentPhoneNumber = x.Student.ParentPhoneNumber,
                    User = _mapper.Map<UserModel>(x)
                })
                .ToListAsync(cancellationToken);

            var localIds = students.Select(x => x.ProvinceId).Concat(students.Select(x => x.DistrictId)).Where(x => x.HasValue).Distinct().ToList();
            var localResults = await _systemService.GetLocationByIdsAsync(new GetLocationsByIdsQueryModel
            {
                IdsStr = string.Join(",", localIds)
            });
            var localDics = localResults.Content?.Result?.ToDictionary(x => x.Id, x => x.Name) ?? new Dictionary<Guid, string?>();
            var studentIds = students.Select(x => x.Id).Distinct().ToList();
            if (studentIds.Any())
            {
                var studentParents = (await (from baseQ in _parentRepository.Queryable
                                             join user in _userManager.Users on baseQ.UserId equals user.Id
                                             join parentStudent in _parentStudentRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId) on baseQ.Id equals parentStudent.ParentId
                                             select new
                                             {
                                                 StudentId = parentStudent.StudentId,
                                                 ParentFullName = user.FullName,
                                             }).ToListAsync(cancellationToken))
                                             .GroupBy(x => x.StudentId)
                                            .ToDictionary(x => x.Key, x => x.Select(x => x.ParentFullName).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x).FirstOrDefault());
                students.ForEach(x =>
                {
                    if (studentParents.TryGetValue(x.Id, out var parentFullName))
                    {
                        x.ParentFullName = parentFullName;
                    }
                    if (x.DistrictId.HasValue && localDics.TryGetValue(x.DistrictId.Value, out var district))
                    {
                        x.District = district;
                    }
                    if (x.ProvinceId.HasValue && localDics.TryGetValue(x.ProvinceId.Value, out var province))
                    {
                        x.Province = province;
                    }
                });
            }

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
