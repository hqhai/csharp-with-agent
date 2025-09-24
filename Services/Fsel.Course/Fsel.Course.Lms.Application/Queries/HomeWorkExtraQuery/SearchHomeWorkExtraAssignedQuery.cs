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

        public SearchHomeWorkExtraAssignedQueryHandler(IHomeWorkRepository homeWorkRepository,
            AuthContext authContext,
            IUserService userService,
            IHomeWorkConfigRepository homeWorkConfigRepository,
            IMapper mapper,
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            IHomeWorkExtraPracticeAnswerRepository homeWorkExtraPracticeAnswerRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _userService = userService;
            _homeWorkConfigRepository = homeWorkConfigRepository;
            _mapper = mapper;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkExtraPracticeAnswerRepository = homeWorkExtraPracticeAnswerRepository;
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

            var curriculumIds = new List<Guid> { Guid.NewGuid() };
            if (!curriculumIds.Any())
            {
                return methodResult;
            }
            var query = _homeWorkRepository.Queryable;
            if (!string.IsNullOrEmpty(request.WorkFilterStr))
            {
                var workFilterStatuses = request.WorkFilterStr.ToList<EnumWorkFilterStatus>() ?? new List<EnumWorkFilterStatus>();
                var dateTime = DateTime.Now;

                bool hasCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.Completed);
                bool hasNotCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.NotCompleted);
                bool hasOverdue = workFilterStatuses.Contains(EnumWorkFilterStatus.Overdue);
                if (!hasCompleted && !hasOverdue && hasNotCompleted)
                {
                    query = from baseQ in query
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId into herGroup
                            from her in herGroup.DefaultIfEmpty()
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id
                            && (her == null || her.Status != EnumResultStatus.Done)
                            select baseQ;
                }
                else if (hasCompleted && !hasOverdue && !hasNotCompleted)
                {
                    query = from baseQ in query
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id && her.Status == EnumResultStatus.Done
                            select baseQ;
                }
                else if (hasOverdue && !hasNotCompleted && !hasCompleted)
                {
                    query = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId
                            where dateTime >= hwc.StartDate && dateTime <= hwc.EndDate
                            select baseQ;
                }
            }

            var queryData = from baseQ in query
                            join hwc in _homeWorkConfigRepository.Queryable.WhereBulkContains(curriculumIds, x => x.CurriculumId) on baseQ.Id equals hwc.HomeWorkId
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
                                TopicName = baseQ.Topic != null ? baseQ.Topic.Name : null,
                                CorrectTotal = baseQ.HomeWorkQuestions.Sum(x => x.Question!.CorrectTotal),
                                NumberRetry = baseQ.HomeWorkRetrys.FirstOrDefault(x => x.StudentId == student.Id) != null ? baseQ.HomeWorkRetrys.FirstOrDefault(x => x.StudentId == student.Id)!.NumberRetry : hwc.NumberRetry,
                                HomeWorkExtraPracticeResult = _mapper.Map<HomeWorkExtraPracticeResultModel>(baseQ.HomeWorkExtraPracticeResults.FirstOrDefault(x => x.WorkingStatus == EnumWorkingStatus.Active && x.StudentId == student.Id))
                            };

            int totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken);
            await SetCorrectCountsAsync(lists, cancellationToken);

            methodResult.Result = new PagingItemsModel<HomeWorkExtraModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task SetCorrectCountsAsync(IList<HomeWorkExtraModel> lists, CancellationToken cancellationToken)
        {
            var homeWorkExtraPracticeResultIds = lists.Select(x => x.HomeWorkExtraPracticeResult).Where(x => x != null).Select(x => x!.Id).ToList();
            if (homeWorkExtraPracticeResultIds.Any())
            {
                var homeWorkExtraAnswers = await _homeWorkExtraPracticeAnswerRepository.Queryable
                                                        .WhereBulkContains(homeWorkExtraPracticeResultIds, x => x.HomeWorkExtraPracticeResultId)
                                                        .ToListAsync(cancellationToken);
                var answers = homeWorkExtraAnswers.GroupBy(x => x.HomeWorkExtraPracticeResultId)
                                                  .ToDictionary(x => x.Key, x => x.Sum(x => x.CorrectCount));

                foreach (var item in lists)
                {
                    answers.TryGetValue(item.Id, out var correctAnswer);
                    item.CorrectCount = correctAnswer;
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
