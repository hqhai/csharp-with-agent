using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Interaction.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Interaction.Application.Queries.SurveyConfigQuery
{
    public class SearchSurveyConfigQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<SurveyConfigModel>>>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }

    public class SearchSurveyConfigQueryHandler : IRequestHandler<SearchSurveyConfigQuery, MethodResult<PagingItemsModel<SurveyConfigModel>>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;

        public SearchSurveyConfigQueryHandler(ISurveyConfigRepository surveyConfigRepository, ICustomerSurveyRepository customerSurveyRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
        }

        public async Task<MethodResult<PagingItemsModel<SurveyConfigModel>>> Handle(SearchSurveyConfigQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SurveyConfigModel>>();

            var surveyConfigs = await _surveyConfigRepository.Queryable.ToListAsync(cancellationToken);

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate < request.EndDate)
            {
                surveyConfigs = surveyConfigs.Where(p => p.StartDate >= request.StartDate && request.EndDate >= p.EndDate).ToList();
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                surveyConfigs = surveyConfigs.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            if (request.CourseLevels != null && request.CourseLevels.Any())
            {
                surveyConfigs = surveyConfigs.Where(p => p.ApplicableSubjects != null && p.ApplicableSubjects.Any(x => request.CourseLevels.Contains(x.CourseLevel))).ToList();
            }

            var models = surveyConfigs.Select(p => new SurveyConfigModel()
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedDate = p.CreatedDate.AddHours(7),
                Status = p.Status
            });

            int totalItem = models.Count();

            var lists = models
                    .ApplySortAndPaging(request)
                    .ToList();

            var surveyConfigIds = lists.Select(p => p.Id);
            var surveyQuestions = await _surveyQuestionRepository.Queryable.WhereBulkContains(surveyConfigIds, p => p.SurveyConfigId).ToListAsync(cancellationToken);
            var surveyQuestionIds = surveyQuestions.Select(p => p.Id);
            var customerSurveys = await (from cs in _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId)
                                         join csg in _customerSurveyGroupRepository.Queryable on cs.CustomerSurveyGroupId equals csg.Id
                                         where csg.Status == EnumSurveyGroupStatus.Done
                                         select new
                                         {
                                             CustomerSurvey = cs,
                                             CustomerSurveyGroup = csg
                                         }).ToListAsync(cancellationToken);

            lists.ForEach(p =>
            {
                var questionIds = surveyQuestions.Where(x => x.SurveyConfigId == p.Id).Select(x => x.Id);
                p.CompletedUserCount = customerSurveys.Where(x => questionIds.Contains(x.CustomerSurvey.SurveyQuestionId)).GroupBy(p => p.CustomerSurvey.UserId).Count();
            });

            methodResult.Result = new PagingItemsModel<SurveyConfigModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
