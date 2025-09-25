// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkExtraAssignedQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkExtraModel>>>
    {
        public string? WorkFilterStr { get; set; }
    }

    public class SearchHomeWorkExtraAssignedQueryHandler : IRequestHandler<SearchHomeWorkExtraAssignedQuery, MethodResult<PagingItemsModel<HomeWorkExtraModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private readonly IMapper _mapper;
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;
        private readonly IHomeWorkExtraPracticeAnswerRepository _homeWorkExtraPracticeAnswerRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IHomeWorkRetryRepository _homeWorkRetryRepository;

        public SearchHomeWorkExtraAssignedQueryHandler(IHomeWorkRepository homeWorkRepository,
            AuthContext authContext,
            IUserService userService,
            IHomeWorkConfigRepository homeWorkConfigRepository,
            IMapper mapper,
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            IHomeWorkExtraPracticeAnswerRepository homeWorkExtraPracticeAnswerRepository,
            ICurriculumRepository curriculumRepository,
            ICurriculumStudentRepository curriculumStudentRepository,
            IHomeWorkRetryRepository homeWorkRetryRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _userService = userService;
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _mapper = mapper;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkExtraPracticeAnswerRepository = homeWorkExtraPracticeAnswerRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkExtraModel>>> Handle(SearchHomeWorkExtraAssignedQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<HomeWorkExtraModel>>();

            var studentResult = await GetStudentAsync();
            if (!studentResult.IsOK)
            {
                methodResult.AddErrorBadRequest(studentResult.ErrorMessages);
                return methodResult;
            }
            var student = studentResult.Result!;
            var dateTime = DateTime.Now;
            var curriculumConfigId = await GetCurriculumConfigIdAsync(student, cancellationToken);

            var query = _homeWorkRepository.Queryable.Where(x => x.Type == EnumHomeWorkType.HomeworkExtra).Where(x => !x.IsArchive);
            if (!string.IsNullOrEmpty(request.WorkFilterStr))
            {
                var workFilterStatuses = request.WorkFilterStr.ToList<EnumWorkFilterStatus>() ?? new List<EnumWorkFilterStatus>();

                bool hasCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.Completed);
                bool hasNotCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.NotCompleted);
                bool hasOverdue = workFilterStatuses.Contains(EnumWorkFilterStatus.Overdue);

                if (!hasCompleted && !hasOverdue && hasNotCompleted)
                {
                    query = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId into herGroup
                            from her in herGroup.DefaultIfEmpty()
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id
                            && (her == null || her.Status != EnumResultStatus.Done) && hwc.CurriculumId == curriculumConfigId
                            select baseQ;
                }
                else if (hasCompleted && !hasOverdue && !hasNotCompleted)
                {
                    query = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id
                            && her.Status == EnumResultStatus.Done && hwc.CurriculumId == curriculumConfigId
                            select baseQ;
                }
                else if (hasOverdue && !hasNotCompleted && !hasCompleted)
                {
                    query = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId into herGroup
                            from her in herGroup.DefaultIfEmpty()
                            where dateTime >= hwc.StartDate && dateTime <= hwc.EndDate && hwc.CurriculumId == curriculumConfigId
                            && (her == null || her.Status != EnumResultStatus.Done || (her.UpdatedDate ?? her.CreatedDate).AddHours(7) > hwc.EndDate)
                            select baseQ;
                }
            }

            var queryData = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId
                            where hwc.CurriculumId == curriculumConfigId
                            select new HomeWorkExtraModel
                            {
                                Id = baseQ.Id,
                                Name = baseQ.Name,
                                CourseLevel = baseQ.CourseLevel,
                                CourseSkill = baseQ.CourseSkill,
                                CreatedDate = baseQ.CreatedDate,
                                UpdatedDate = baseQ.UpdatedDate,
                                EndDate = hwc.EndDate,
                                StartDate = hwc.StartDate,
                                HomeWorkConfigId = hwc.Id,
                                TopicName = baseQ.Topic != null ? baseQ.Topic.Name : null,
                                TotalQuestion = baseQ.HomeWorkQuestions.Count,
                                CorrectTotal = baseQ.HomeWorkQuestions.Sum(x => x.Question!.CorrectTotal),
                            };

            int totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);
            await SetCorrectCountsAsync(lists, student, cancellationToken);
            foreach (var item in lists)
            {
                item.ExpiryState = ComputeExpiryState(dateTime, item.EndDate, item.HomeWorkExtraPracticeResult?.UpdatedDate, item.HomeWorkExtraPracticeResult?.Status);
            }
            methodResult.Result = new PagingItemsModel<HomeWorkExtraModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public static EnumExpiryState ComputeExpiryState(DateTime now, DateTime? endDate, DateTime? updatedDate, EnumResultStatus? status) =>
             endDate is null || now <= endDate.Value ? EnumExpiryState.NotExpired
            : (status == EnumResultStatus.Done && updatedDate.HasValue && updatedDate <= endDate ? EnumExpiryState.ExpiredMetTarget
            : EnumExpiryState.ExpiredUnmetTarget);

        private async Task<Guid?> GetCurriculumConfigIdAsync(StudentModel student, CancellationToken cancellationToken)
        {
            Guid? curriculumId = await (from baseQ in _curriculumRepository.Queryable
                                        join cs in _curriculumStudentRepository.Queryable on baseQ.Id equals cs.CurriculumId
                                        where baseQ.CourseCloneId == student.CourseId && cs.StudentId == student.Id
                                        select baseQ.Id).FirstOrDefaultAsync(cancellationToken);

            return curriculumId.HasValue && curriculumId.Value != Guid.Empty ? curriculumId.Value : null;
        }

        private async Task SetCorrectCountsAsync(IList<HomeWorkExtraModel> lists, StudentModel student, CancellationToken cancellationToken)
        {
            var homeWorkExtraResults = await (from baseQ in _homeWorkExtraPracticeResultRepository.Queryable
                                              join hwr in _homeWorkRetryRepository.Queryable on baseQ.HomeWorkRetryId equals hwr.Id
                                              where baseQ.StudentId == student.Id && baseQ.WorkingStatus == EnumWorkingStatus.Active
                                              select new
                                              {
                                                  hwr.HomeWorkId,
                                                  hwr.HomeWorkConfigId,
                                                  baseQ,
                                                  hwr
                                              }).AsNoTracking().ToListAsync(cancellationToken);
            var homeWorkExtraResultDict = homeWorkExtraResults
                                            .ToDictionary(k => (k.HomeWorkId, k.HomeWorkConfigId), x => x.baseQ);

            var homeWorkExtraRetrytDict = homeWorkExtraResults
                                            .ToDictionary(k => (k.HomeWorkId, k.HomeWorkConfigId), x => x.hwr.NumberRetry);

            var homeWorkExtraPracticeResultIds = homeWorkExtraResults.Select(x => x.baseQ).Where(x => x != null).Select(x => x!.Id).ToList();
            if (homeWorkExtraPracticeResultIds.Any())
            {
                var homeWorkExtraAnswers = await _homeWorkExtraPracticeAnswerRepository.Queryable
                                                        .WhereBulkContains(homeWorkExtraPracticeResultIds, x => x.HomeWorkExtraPracticeResultId)
                                                        .ToListAsync(cancellationToken);
                var answers = homeWorkExtraAnswers.GroupBy(x => x.HomeWorkExtraPracticeResultId)
                                                  .ToDictionary(x => x.Key, x => x.ToList());

                foreach (var item in lists)
                {
                    var key = (item.Id, item.HomeWorkConfigId);

                    homeWorkExtraResultDict.TryGetValue(key, out var result);
                    item.HomeWorkExtraPracticeResult = _mapper.Map<HomeWorkExtraPracticeResultModel>(result);

                    homeWorkExtraRetrytDict.TryGetValue(key, out var retry);
                    item.NumberRetry = retry;

                    answers.TryGetValue(item.Id, out var listAnswer);
                    if (listAnswer == null || !listAnswer.Any())
                    {
                        continue;
                    }
                    item.CountQuestion = listAnswer.Count;
                    item.CorrectCount = listAnswer.Sum(x => x.CorrectCount);
                    item.ProgressPercent = NumberHelper.GetPercent(item.CountQuestion, item.TotalQuestion);
                }
            }
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }
    }
}
