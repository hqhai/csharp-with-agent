using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
using Fsel.ExamPractice.Domain.Models.QueryModels.ExamPractices;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Application.Queries.ExamPracticeQuery.V1i1
{
    public class SearchExamPracticeQuery : SearchExamPracticeQueryModel, IRequest<MethodResult<PagingItemsModel<ExamPracticeSearchModel>>>
    {
    }

    public class SearchExamPracticeQueryHandler : IRequestHandler<SearchExamPracticeQuery, MethodResult<PagingItemsModel<ExamPracticeSearchModel>>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;

        public SearchExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository)
        {
            _examPracticeRepository = examPracticeRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ExamPracticeSearchModel>>> Handle(SearchExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ExamPracticeSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var query = _examPracticeRepository.Queryable.Where(x => x.Type == request.Type && x.VersionStatus == EnumVersionStatus.LastVersion && !x.IsArchive);
            request.Keyword = request.Keyword?.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.Id == guid);
                }
                else
                {
                    var queryCode = query.Where(m => m.Code != null && m.Code.Contains(request.Keyword));
                    var queryName = query.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                    query = queryCode.Union(queryName);
                }
            }
            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.StartDate >= request.StartDate);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.EndDate <= request.EndDate);
            }
            if (request.SubTypes != null && request.SubTypes.Any())
            {
                query = query.Where(x => request.SubTypes.Contains(x.SubType));
            }
            var queryData = query.Select(x => new ExamPracticeSearchModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                CreatedFullName = x.CreatedFullName,
                CreatedDate = x.CreatedDate,
                Status = x.Status,
                Type = x.Type,
                SubType = x.SubType,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                UpdatedDate = x.UpdatedDate,
                CourseSkills = x.ExamPracticeSections.Where(x => x.CourseSkill.HasValue).Select(x => x.CourseSkill.GetValueOrDefault()).Distinct().ToList(),
                CourseSubType = x.SubType,
                TotalAttempts = x.ExamPracticeResults.Count,
            });
            int totalItem = await queryData.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await queryData.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                                   .ApplyPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ExamPracticeSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
