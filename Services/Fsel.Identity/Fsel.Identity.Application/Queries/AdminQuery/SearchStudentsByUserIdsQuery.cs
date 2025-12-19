// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
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
        private readonly UserManager<User> _userManager;

        public SearchStudentsByUserIdsQueryHandler(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
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

            var students = await _userManager.Users
                .WhereBulkContains(request.UserIds, i => i.Id)
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
                    User = _mapper.Map<UserModel>(x)
                })
                .ToListAsync(cancellationToken);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    students = students.Where(p => p.User != null && p.User.Email == request.Keyword).ToList();
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    students = students.Where(p => p.User != null && p.User.PhoneNumber == request.Keyword).ToList();
                }
                else
                {
                    students = students.Where(p => p.User != null && !string.IsNullOrEmpty(p.User.FullName) && p.User.FullName.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
            }

            methodResult.Result = _mapper.Map<IList<StudentModel>>(students);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
