// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.SurveyConfigQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.QueryModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentSurveyRecordQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<StudentSurveyRecordModel>>>
    {
        public Guid SurveyConfigId { get; set; }
    }

    public class SearchStudentSurveyRecordQueryHandler : IRequestHandler<SearchStudentSurveyRecordQuery, MethodResult<PagingItemsModel<StudentSurveyRecordModel>>>
    {
        private readonly ISurveyConfigRepository _surveyConfigRepository;
        private readonly ISurveyQuestionRepository _surveyQuestionRepository;
        private readonly ICustomerSurveyRepository _customerSurveyRepository;
        private readonly ICustomerSurveyGroupRepository _customerSurveyGroupRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private const int ChunkSize = 10000;

        public SearchStudentSurveyRecordQueryHandler(ISurveyConfigRepository surveyConfigRepository, ISurveyQuestionRepository surveyQuestionRepository, ICustomerSurveyRepository customerSurveyRepository, ICustomerSurveyGroupRepository customerSurveyGroupRepository, IUserService userService, IMapper mapper)
        {
            _surveyConfigRepository = surveyConfigRepository;
            _surveyQuestionRepository = surveyQuestionRepository;
            _customerSurveyRepository = customerSurveyRepository;
            _customerSurveyGroupRepository = customerSurveyGroupRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<StudentSurveyRecordModel>>> Handle(SearchStudentSurveyRecordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentSurveyRecordModel>>();

            var surveyConfig = await _surveyConfigRepository.Queryable.Include(p => p.SurveyQuestions).FirstOrDefaultAsync(p => p.Id == request.SurveyConfigId, cancellationToken);
            if (surveyConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(surveyConfig));
                return methodResult;
            }

            var surveyQuestionIds = surveyConfig.SurveyQuestions.Select(p => p.Id).ToList();

            var customerSurveys = await (from cs in _customerSurveyRepository.Queryable.WhereBulkContains(surveyQuestionIds, p => p.SurveyQuestionId)
                                         join csg in _customerSurveyGroupRepository.Queryable on cs.CustomerSurveyGroupId equals csg.Id
                                         where csg.Status == EnumSurveyGroupStatus.Done
                                         select new StudentSurveyRecordModel
                                         {
                                             UserId = csg.UserId,
                                             CreatedDate = csg.CreatedDate,
                                             CustomerSurveyGroupId = csg.Id
                                         }).ToListAsync(cancellationToken);

            customerSurveys = customerSurveys.DistinctBy(p => p.CustomerSurveyGroupId).ToList();

            var distinctUserIds = customerSurveys.Select(p => p.UserId).Distinct().ToList();
            var userIdsChunk = distinctUserIds.Chunk(ChunkSize);
            var students = new List<StudentModel>();

            foreach (var item in userIdsChunk)
            {
                var studentResults = await _userService.SearchStudentsByUserIds(new SearchStudentsByUserIdsQueryModel()
                {
                    UserIds = item.ToList(),
                    Keyword = request.Keyword
                });

                if (studentResults?.Content?.Result != null)
                {
                    students.AddRange(studentResults.Content.Result);
                }
            }

            int totalItem = customerSurveys.Count;

            var lists = customerSurveys.OrderByDescending(p => p.CreatedDate)
                    .ApplySortAndPaging(request)
                    .ToList();

            lists.ForEach(p =>
            {
                var student = students?.FirstOrDefault(x => x.User?.Id == p.UserId);
                if (student != null)
                {
                    p.FullName = student.User?.FullName;
                    p.Email = student.User?.Email;
                    p.StudentCode = student.User?.Code;
                }
                else
                {
                    p.FullName = "Not Found";
                }
            });

            methodResult.Result = new PagingItemsModel<StudentSurveyRecordModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
