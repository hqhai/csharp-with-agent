// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.HomeWorks;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchHomeWorkExtraQuery : SearchHomeWorkExtraQueryModel, IRequest<MethodResult<PagingItemsModel<HomeWorkExtraModel>>>
    {
    }

    public class SearchHomeWorkExtraQueryHandler : IRequestHandler<SearchHomeWorkExtraQuery, MethodResult<PagingItemsModel<HomeWorkExtraModel>>>
    {
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly IHomeWorkExtraPracticeAnswerRepository _homeWorkExtraPracticeAnswerRepository;
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IHomeWorkConfigRepository _homeWorkConfigRepository;
        private const int MaxRetry = 1;

        public SearchHomeWorkExtraQueryHandler(IHomeWorkRepository homeWorkRepository,
            IUserService userService,
            AuthContext authContext,
            IMapper mapper,
            IHomeWorkExtraPracticeAnswerRepository homeWorkExtraPracticeAnswerRepository,
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            ICurriculumRepository curriculumRepository,
            ICurriculumStudentRepository curriculumStudentRepository,
            IHomeWorkConfigRepository homeWorkConfigRepository)
        {
            _homeWorkRepository = homeWorkRepository;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
            _homeWorkExtraPracticeAnswerRepository = homeWorkExtraPracticeAnswerRepository;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _homeWorkConfigRepository = homeWorkConfigRepository;
        }

        public async Task<MethodResult<PagingItemsModel<HomeWorkExtraModel>>> Handle(SearchHomeWorkExtraQuery request, CancellationToken cancellationToken)
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

            var curriculumConfigId = await GetCurriculumConfigIdAsync(student, cancellationToken);

            var query = _homeWorkRepository.Queryable.Where(x => x.Type == EnumHomeWorkType.HomeworkExtra).Where(x => !x.IsArchive);
            if (curriculumConfigId.HasValue)
            {
                query = from baseQ in query
                        join hwc in _homeWorkConfigRepository.Queryable on baseQ.Id equals hwc.HomeWorkId into hwcGroup
                        from hwc in hwcGroup
                        where hwc.CurriculumId != curriculumConfigId
                        select baseQ;
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => !string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Keyword));
            }
            if (!string.IsNullOrEmpty(request.CourseLevelStr))
            {
                var courseLevels = request.CourseLevelStr.ToList<EnumCourseLevel>() ?? new List<EnumCourseLevel>();
                query = query.Where(m => courseLevels.Contains(m.CourseLevel));
            }
            if (!string.IsNullOrEmpty(request.CourseSkillStr))
            {
                var courseSkills = request.CourseSkillStr.ToList<EnumCourseSkill>() ?? new List<EnumCourseSkill>();
                query = query.Where(m => courseSkills.Contains(m.CourseSkill));
            }
            if (!string.IsNullOrEmpty(request.TopicIdStr))
            {
                var topicIds = request.TopicIdStr.ToList<Guid>() ?? new List<Guid>();
                query = query.WhereBulkContains(topicIds, x => x.TopicId);
            }

            if (!string.IsNullOrEmpty(request.WorkFilterStr))
            {
                var workFilterStatuses = request.WorkFilterStr.ToList<EnumWorkFilterStatus>() ?? new List<EnumWorkFilterStatus>();

                bool hasCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.Completed);
                bool hasNotCompleted = workFilterStatuses.Contains(EnumWorkFilterStatus.NotCompleted);
                if (!hasCompleted && hasNotCompleted)
                {
                    query = from baseQ in query
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId into herGroup
                            from her in herGroup.DefaultIfEmpty()
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id
                            && (her == null || her.Status != EnumResultStatus.Done)
                            select baseQ;
                }
                else if (hasCompleted && !hasNotCompleted)
                {
                    query = from baseQ in query
                            join her in _homeWorkExtraPracticeResultRepository.Queryable on baseQ.Id equals her.HomeWorkId
                            where her.WorkingStatus == EnumWorkingStatus.Active && her.StudentId == student.Id && her.Status == EnumResultStatus.Done
                            select baseQ;
                }
            }

            var queryData = query.Select(x => new HomeWorkExtraModel
            {
                Id = x.Id,
                CourseLevel = x.CourseLevel,
                CourseSkill = x.CourseSkill,
                Name = x.Name,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                CorrectTotal = x.HomeWorkQuestions.Sum(x => x.Question!.CorrectTotal),
                TopicName = x.Topic != null ? x.Topic.Name : string.Empty,
                NumberRetry = x.HomeWorkRetrys.FirstOrDefault(x => x.StudentId == student.Id) != null ? x.HomeWorkRetrys.FirstOrDefault(x => x.StudentId == student.Id)!.NumberRetry : MaxRetry,
                HomeWorkExtraPracticeResult = _mapper.Map<HomeWorkExtraPracticeResultModel>(x.HomeWorkExtraPracticeResults.FirstOrDefault(x => x.WorkingStatus == EnumWorkingStatus.Active && x.StudentId == student.Id))
            });

            int totalItem = await queryData.CountAsync(cancellationToken);
            var lists = await queryData.ApplySortAndPaging(request)
                                       .AsNoTracking()
                                       .ToListAsync(cancellationToken);
            await SetCorrectCountsAsync(lists, cancellationToken);

            methodResult.Result = new PagingItemsModel<HomeWorkExtraModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Guid?> GetCurriculumConfigIdAsync(StudentModel student, CancellationToken cancellationToken)
        {
            Guid? curriculumId = await (from baseQ in _curriculumRepository.Queryable
                                        join cs in _curriculumStudentRepository.Queryable on baseQ.Id equals cs.CurriculumId
                                        where baseQ.CourseCloneId == student.CourseId && cs.StudentId == student.Id
                                        select baseQ.Id).FirstOrDefaultAsync(cancellationToken);
            return curriculumId.HasValue && curriculumId.Value != Guid.Empty ? curriculumId.Value : null;
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
