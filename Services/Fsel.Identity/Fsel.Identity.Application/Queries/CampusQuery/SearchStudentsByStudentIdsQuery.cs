// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.CampusQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsByStudentIdsQuery : SearchStudentsCampusByStudentIdsQueryModel, IRequest<MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
    }

    public class SearchStudentsByStudentIdsQueryHandler : IRequestHandler<SearchStudentsByStudentIdsQuery, MethodResult<PagingItemsModel<StudentCampusModel>>>
    {
        private readonly Core.Base.Managers.UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISchoolClassRepository _schoolClassRepository;

        public SearchStudentsByStudentIdsQueryHandler(Core.Base.Managers.UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, ISchoolClassRepository schoolClassRepository)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _schoolClassRepository = schoolClassRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentCampusModel>>> Handle(SearchStudentsByStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentCampusModel>>();

            var query = from u in _userManager.Users
                        join h in _humanRepository.Queryable on u.Id equals h.UserId
                        join s in _studentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.Id) on h.Id equals s.HumanId
                        select new StudentCampusModel()
                        {
                            Id = u.Id,
                            CreatedDate = u.CreatedDate,
                            FullName = u.FullName,
                            Class = s.SchoolClass,
                            Email = u.Email,
                            UserName = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            StudentId = s.Id,
                            SchoolClassId = s.SchoolClassId,
                        };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.Email) && p.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.PhoneNumber) && p.PhoneNumber == request.Keyword);
                }
                else
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.FullName) && p.FullName.Contains(request.Keyword));
                }
            }
            if (request.SchoolClassIds != null && request.SchoolClassIds.Any())
            {
                query = query.Where(p => p.SchoolClassId.HasValue && request.SchoolClassIds.Contains(p.SchoolClassId.Value));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                        .AsNoTracking()
                                        .ToListAsync(cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

            var schoolClassIds = lists.Where(p => p.SchoolClassId.HasValue).Select(p => p.SchoolClassId).Distinct().ToList();
            var schoolClasses = await _schoolClassRepository.Queryable.WhereBulkContains(schoolClassIds, p => p.Id).ToListAsync(cancellationToken);
            lists.ForEach(p =>
            {
                p.Class = schoolClasses.FirstOrDefault(x => x.Id == p.SchoolClassId)?.Name;
            });

            methodResult.Result = new PagingItemsModel<StudentCampusModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
