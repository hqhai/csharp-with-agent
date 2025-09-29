// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery.Classes
{
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchSchoolClassQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SchoolClassModel>>>
    {
        public Guid? CreatedUserId { get; set; }
    }

    public class SearchSchoolClassQueryHandler : IRequestHandler<SearchSchoolClassQuery, MethodResult<PagingItemsModel<SchoolClassModel>>>
    {
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;

        public SearchSchoolClassQueryHandler(ISchoolClassRepository schoolClassRepository, UserManager<User> userManager, IStudentRepository studentRepository, AuthContext authContext)
        {
            _schoolClassRepository = schoolClassRepository;
            _userManager = userManager;
            _studentRepository = studentRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolClassModel>>> Handle(SearchSchoolClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolClassModel>>();

            var schoolIdStr = _authContext.ClaimsPrincipal?.FindFirstValue("SchoolId");

            if (string.IsNullOrEmpty(schoolIdStr) || !Guid.TryParse(schoolIdStr, out Guid schoolId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId), _authContext.CurrentUserId);
                return methodResult;
            }

            var query = _schoolClassRepository.Queryable.Where(p => p.SchoolId == schoolId).Select(p => new SchoolClassModel()
            {
                Id = p.Id,
                CreatedUserId = p.CreatedUserId,
                Name = p.Name,
                TeacherId = p.TeacherId,
                CreatedDate = p.CreatedDate
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword));
            }

            if (request.CreatedUserId.HasValue)
            {
                query = query.Where(p => p.CreatedUserId == request.CreatedUserId);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

            var ids = lists.Select(l => l.Id).Distinct();

            var students = await _studentRepository.Queryable.WhereBulkContains(ids, p => p.SchoolClassId).ToListAsync(cancellationToken);

            var createdUserIds = lists.Select(l => l.CreatedUserId).Distinct();
            var users = await _userManager.Users.WhereBulkContains(createdUserIds, p => p.Id).ToListAsync(cancellationToken);

            lists.ForEach(p =>
            {
                p.CreatedFullName = users.FirstOrDefault(x => x.Id == p.CreatedUserId)?.FullName;
                p.NumberOfStudent = students.Where(x => x.SchoolClassId == p.Id).Count();
            });

            methodResult.Result = new PagingItemsModel<SchoolClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
