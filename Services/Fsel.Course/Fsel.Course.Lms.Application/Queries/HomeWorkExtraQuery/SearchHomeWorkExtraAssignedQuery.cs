// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
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
            var now = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var curriculumConfigId = await GetCurriculumConfigIdAsync(student, cancellationToken);

            var hwcQ = _homeWorkConfigRepository.Queryable.Where(x => x.CurriculumId == curriculumConfigId);

            var herQ = from baseQResult in _homeWorkExtraPracticeResultRepository.Queryable.Where(x => x.WorkingStatus == EnumWorkingStatus.Active && x.StudentId == student.Id)
                       join hwr in _homeWorkRetryRepository.Queryable on baseQResult.HomeWorkRetryId equals hwr.Id
                       where hwr.HomeWorkConfigId != null
                       select new
                       {
                           baseQResult.Status,
                           baseQResult.UpdatedDate,
                           baseQResult.CreatedDate,
                           hwr.HomeWorkConfigId,
                           hwr.HomeWorkId
                       };

            var baseQ =
                from hw in _homeWorkRepository.Queryable
                where hw.Type == EnumHomeWorkType.HomeworkExtra && !hw.IsArchive
                join hwc in hwcQ on hw.Id equals hwc.HomeWorkId
                select new
                {
                    hw,
                    HomeWorkConfigId = hwc.Id,
                    hwc.StartDate,
                    hwc.EndDate,
                    hwc.NumberRetry
                };

            var statuses = request.WorkFilterStr.ToList<EnumWorkFilterStatus>() ?? new List<EnumWorkFilterStatus>();
            bool wantCompleted = statuses.Contains(EnumWorkFilterStatus.Completed);
            bool wantNotCompleted = statuses.Contains(EnumWorkFilterStatus.NotCompleted);
            bool wantOverdue = statuses.Contains(EnumWorkFilterStatus.Overdue);

            if (wantCompleted || wantNotCompleted || wantOverdue)
            {
                baseQ = from x in baseQ
                        let doneAny = herQ.Where(r => r.HomeWorkId == x.hw.Id && r.HomeWorkConfigId == x.HomeWorkConfigId)
                                          .Any(r => r.Status == EnumResultStatus.Done)
                        // Hoàn thành đúng hạn: có bản Done và thời điểm hoàn thành <= EndDate
                        let doneOnTime = herQ.Where(r => r.HomeWorkId == x.hw.Id && r.Status == EnumResultStatus.Done && r.HomeWorkConfigId == x.HomeWorkConfigId)
                                             .Any(r => (r.UpdatedDate ?? r.CreatedDate).AddHours(7) <= x.EndDate)
                        let isOverdue = now > x.EndDate && !doneOnTime
                        where
                            (wantCompleted && doneAny) ||
                            (wantNotCompleted && !doneAny) ||
                            (wantOverdue && isOverdue)
                        select x;
            }

            var queryData =
                from x in baseQ
                select new HomeWorkExtraModel
                {
                    Id = x.hw.Id,
                    Name = x.hw.Name,
                    CourseLevel = x.hw.CourseLevel,
                    CourseSkill = x.hw.CourseSkill,
                    CreatedDate = x.hw.CreatedDate,
                    UpdatedDate = x.hw.UpdatedDate,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    HomeWorkConfigId = x.HomeWorkConfigId,
                    TopicName = x.hw.Topic != null ? x.hw.Topic.Name : null,
                    TotalQuestion = x.hw.HomeWorkQuestions.Count,
                    CorrectTotal = x.hw.HomeWorkQuestions.Sum(q => q.Question!.CorrectTotal),
                    NumberRetry = x.NumberRetry
                };

            int totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);
            await SetCorrectCountsAsync(lists, student, cancellationToken);
            foreach (var item in lists)
            {
                item.ExpiryState = ComputeExpiryState(now, item.EndDate, item.HomeWorkExtraPracticeResult?.UpdatedDate, item.HomeWorkExtraPracticeResult?.Status);
            }
            methodResult.Result = new PagingItemsModel<HomeWorkExtraModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public static EnumExpiryState ComputeExpiryState(DateTime now, DateTime? endDate, DateTime? updatedDate, EnumResultStatus? status)
        {
            if (!endDate.HasValue)
            {
                return EnumExpiryState.NotExpired;
            }
            var deadline = endDate.Value;
            var isDone = status == EnumResultStatus.Done;

            // Đã hoàn thành và có thời điểm cập nhật
            if (isDone && updatedDate is DateTime doneAt)
            {
                return doneAt.AddHours(7) <= deadline ? EnumExpiryState.ExpiredMetTarget : EnumExpiryState.ExpiredUnmetTarget;  // Hoàn thành muộn
            }

            // Chưa hoàn thành (hoặc status null/khác Done) và đã quá hạn
            if (!isDone && now >= deadline)
            {
                return EnumExpiryState.ExpiredUnmetTarget;
            }
            return EnumExpiryState.NotExpired;
        }

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
            var keys = lists.Select(x => new { HomeWorkId = x.Id, x.HomeWorkConfigId }).ToList();

            var homeWorkRetrys = await _homeWorkRetryRepository.Queryable.WhereBulkContains(keys, x => new { x.HomeWorkId, x.HomeWorkConfigId })
                                                               .Where(x => x.StudentId == student.Id)
                                                               .AsNoTracking()
                                                               .ToListAsync(cancellationToken);

            var homeWorkExtraResults = await (from baseQ in _homeWorkExtraPracticeResultRepository.Queryable.WhereBulkContains(homeWorkRetrys.Select(x => x.Id).ToList(), x => x.HomeWorkRetryId)
                                              where baseQ.WorkingStatus == EnumWorkingStatus.Active
                                              select baseQ).AsNoTracking().ToListAsync(cancellationToken);

            var homeWorkExtraResultDict = homeWorkExtraResults
                                            .ToDictionary(k => (k.HomeWorkId, k.HomeWorkRetryId), x => x);

            var homeWorkExtraRetrytDict = homeWorkRetrys
                                            .ToDictionary(k => (k.HomeWorkId, k.HomeWorkConfigId), x => x);

            var homeWorkExtraPracticeResultIds = homeWorkExtraResults.Where(x => x != null).Select(x => x!.Id).ToList();
            var homeWorkExtraAnswers = await _homeWorkExtraPracticeAnswerRepository.Queryable
                                                                    .WhereBulkContains(homeWorkExtraPracticeResultIds, x => x.HomeWorkExtraPracticeResultId)
                                                                    .ToListAsync(cancellationToken);
            var answers = homeWorkExtraAnswers.GroupBy(x => x.HomeWorkExtraPracticeResultId)
                                              .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in lists)
            {
                var key = (item.Id, item.HomeWorkConfigId);
                homeWorkExtraRetrytDict.TryGetValue(key, out var retry);
                if (retry == null)
                {
                    continue;
                }
                var keyRetry = (item.Id, HomeWorkConfigId: retry.Id);
                homeWorkExtraResultDict.TryGetValue(keyRetry, out var result);

                item.NumberRetry = retry.NumberRetry;
                item.HomeWorkExtraPracticeResult = _mapper.Map<HomeWorkExtraPracticeResultModel>(result);
                if (result == null)
                {
                    continue;
                }
                item.CorrectCount = result.CorrectCount;

                answers.TryGetValue(result.Id, out var listAnswer);
                if (listAnswer == null || !listAnswer.Any())
                {
                    continue;
                }
                item.CountQuestion = listAnswer.Count;
                item.ProgressPercent = NumberHelper.GetPercent(item.CountQuestion, item.TotalQuestion);
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
