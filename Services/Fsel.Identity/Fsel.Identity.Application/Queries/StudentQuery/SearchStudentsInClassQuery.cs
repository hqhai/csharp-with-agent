// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.TrainingService;
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

        public SearchStudentsInClassQueryHandler(IStudentRepository studentRepository, ILmsCourseService lmsCourseService, ITrainingService trainingService)
        {
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _trainingService = trainingService;
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

            var query = _studentRepository.Queryable.Where(p => p.ClassId.HasValue);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(CultureInfo.InvariantCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.Human != null && m.Human.Email != null && m.Human.Email.Contains(request.Keyword));
                }
                else
                {
                    var queryFullName = query.Where(m => m.Human != null && m.Human.FullName != null && m.Human.FullName.Contains(request.Keyword));
                    var queryCode = query.Where(m => m.Human != null && m.Human.Code != null && m.Human.Code.Contains(request.Keyword));
                    query = queryFullName.Union(queryCode);
                }
            }
            if (request.ClassId.HasValue)
            {
                var studentIdsResult = await _trainingService.GetStudentIdsByClassId(request.ClassId.Value);
                if (!studentIdsResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentIdsResult.Error);
                    return methodResult;
                }
                var studentIds = studentIdsResult.Content?.Result;
                query = query.Where(p => studentIds != null && studentIds.Contains(p.Id));
            }

            var dataQuery = query.Select(i => new SearchStudentsInClassModel
            {
                Id = i.Id,
                FullName = i.Human!.FullName,
                BirthDay = i.Human.Birthday,
                Code = i.Human.Code,
                CreatedDate = i.CreatedDate,
                Email = i.Human.Email,
                ClassId = i.ClassId
            });
            int totalItem = await dataQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await dataQuery
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
                    item.PTPoint = ptr!.FirstOrDefault(p => p.StudentId == item.Id) == null ? 0 : Math.Round(ptr!.FirstOrDefault(p => p.StudentId == item.Id)!.PTPoint, 2);
                }
            }

            methodResult.Result = new PagingItemsModel<SearchStudentsInClassModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
