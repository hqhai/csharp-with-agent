// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsByUserIdsQuery : SearchStudentsByUserIdsQueryModel, IRequest<MethodResult<IList<StudentModel>>>
    {
    }

    public class SearchStudentsByUserIdsQueryHandler : IRequestHandler<SearchStudentsByUserIdsQuery, MethodResult<IList<StudentModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;

        public SearchStudentsByUserIdsQueryHandler(IMapper mapper, IHumanRepository humanRepository)
        {
            _mapper = mapper;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<IList<StudentModel>>> Handle(SearchStudentsByUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentModel>>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var students = await _humanRepository.Queryable
                .Include(x => x.User)
                .Where(i => i.UserId != null)
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
                    ExpiredDate = x.Student.ExpiredDate,
                    Human = _mapper.Map<HumanProfileModel>(x)
                })
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    students = students.Where(p => p.Human != null && p.Human.Email == request.Keyword).ToList();
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    students = students.Where(p => p.Human != null && p.Human.PhoneNumber == request.Keyword).ToList();
                }
                else
                {
                    students = students.Where(p => p.Human != null && !string.IsNullOrEmpty(p.Human.FullName) && p.Human.FullName.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
