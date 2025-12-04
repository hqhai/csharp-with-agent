// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsInClassQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SearchStudentsInClassModel>>>
    {
        public Guid? ClassId { get; set; }
    }

    public class SearchStudentsInClassQueryHandler : IRequestHandler<SearchStudentsInClassQuery, MethodResult<PagingItemsModel<SearchStudentsInClassModel>>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ITrainingService _trainingService;
        private readonly UserManager<User> _userManager;

        public SearchStudentsInClassQueryHandler(IStudentRepository studentRepository, ILmsCourseService lmsCourseService,
            ITrainingService trainingService,
            UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _trainingService = trainingService;
            _userManager = userManager;
        }

        public async Task<MethodResult<PagingItemsModel<SearchStudentsInClassModel>>> Handle(SearchStudentsInClassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<SearchStudentsInClassModel>> methodResult = new MethodResult<PagingItemsModel<SearchStudentsInClassModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var userQuery = _userManager.Users;
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.InvariantCulture);
                if (request.Keyword.IsValidEmail())
                {
                    userQuery = userQuery.Where(m => m.Email!.Contains(request.Keyword));
                }
                else
                {
                    var queryFullName = userQuery.Where(m => m.FullName!.Contains(request.Keyword));
                    var queryCode = userQuery.Where(m => m.Code!.Contains(request.Keyword));
                    userQuery = queryFullName.Union(queryCode);
                }
            }
            var studentQuery = _studentRepository.Queryable.Where(p => p.ClassId.HasValue);
            if (request.ClassId.HasValue)
            {
                var studentIdsResult = await _trainingService.GetStudentIdsByClassId(request.ClassId.Value);
                if (!studentIdsResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentIdsResult.Error);
                    return methodResult;
                }
                var studentIds = studentIdsResult.Content?.Result;
                if (studentIds != null && studentIds.Any())
                {
                    studentQuery = studentQuery.WhereBulkContains(studentIds, p => p.Id);
                }
            }

            var query = from baseQ in studentQuery
                        join u in userQuery on baseQ.UserId equals u.Id
                        select new SearchStudentsInClassModel
                        {
                            Id = baseQ.Id,
                            FullName = u.FullName,
                            BirthDay = u.Birthday,
                            Code = u.Code,
                            CreatedDate = baseQ.CreatedDate,
                            UpdatedDate = baseQ.UpdatedDate,
                            Email = u.Email,
                            ClassId = baseQ.ClassId,
                        };

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            if (lists.Count > 0)
            {
                var ptrResult = await _lmsCourseService.GetPTPointByIds(lists.Select(p => p.Id).ToList());
                var ptr = ptrResult.Content?.Result;

                foreach (var item in lists)
                {
                    item.PTPoint = ptr?.FirstOrDefault(p => p.StudentId == item.Id) == null ? 0 : Math.Round(ptr?.FirstOrDefault(p => p.StudentId == item.Id)?.PTPoint ?? default, 2);
                }
            }

            methodResult.Result = new PagingItemsModel<SearchStudentsInClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
