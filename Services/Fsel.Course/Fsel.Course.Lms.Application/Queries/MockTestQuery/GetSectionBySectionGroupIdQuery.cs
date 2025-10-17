// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<SectionGroupDtoModel>>
    {
        public Guid SectionGroupId { get; set; }
        public Guid MockTestResultId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<SectionGroupDtoModel>>
    {
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ILogger<GetSectionBySectionGroupIdQuery> _logger;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IMockTestSectionRepository _mockTestSectionRepository;

        public GetSectionBySectionGroupIdQueryHandler(SectionGroupConverter sectionGroupConverter,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            AuthContext authContext,
            IUserService userService,
            ISectionGroupRepository sectionGroupRepository,
            ILogger<GetSectionBySectionGroupIdQuery> logger,
            QuestBoardPublisher questBoardPublisher,
            IMockTestSectionRepository mockTestSectionRepository)
        {
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _sectionGroupRepository = sectionGroupRepository;
            _logger = logger;
            _questBoardPublisher = questBoardPublisher;
            _mockTestSectionRepository = mockTestSectionRepository;
        }

        public async Task<MethodResult<SectionGroupDtoModel>> Handle(GetSectionBySectionGroupIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SectionGroupDtoModel>();
            //var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            //if (!studentResult.IsSuccessStatusCode)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
            //    return methodResult;
            //}
            //var student = studentResult?.Content?.Result;
            //if (student == null)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
            //    return methodResult;
            //}

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == request.MockTestResultId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            else if (mockTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(mockTestResult));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }

            var isNotInModule = !await (from baseQ in _sectionGroupRepository.Queryable
                                        join mtsg in _mockTestSectionRepository.Queryable on baseQ.Id equals mtsg.SectionGroupId
                                        where baseQ.Id == request.SectionGroupId && mtsg.MockTestId == mockTestResult.MockTestId
                                        select baseQ).AnyAsync(cancellationToken);
            if (isNotInModule)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isNotInModule), request.SectionGroupId);
                return methodResult;
            }
            if (mockTestResult.Status == EnumResultStatus.New)
            {
                await UpdateMockTestResultAsync(mockTestResult, cancellationToken);
            }

            var sectionGroupResult = await GetAndAddSectionGroupResult(request, mockTestResult);
            methodResult.Result = await _sectionGroupConverter.GetSectionGroupDto(sectionGroup, sectionGroupResult, mockTestResult.MockTest?.Version ?? (int)EnumVersion.V1);
            methodResult.Result.Version = mockTestResult.MockTest?.Version ?? default;

            #region Do QuestBoard

            var gradingAlFeedback = methodResult.Result.Sections?.FirstOrDefault()?.MockTestAnswer?.GradingAlFeedback;
            if (!string.IsNullOrEmpty(gradingAlFeedback))
            {
                await DoQuestBoard(mockTestResult.StudentId, EnumQuestBoardCategory.MessagesFromAI, cancellationToken);
            }

            #endregion Do QuestBoard

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateMockTestResultAsync(MockTestResult mockTestResult, CancellationToken cancellationToken)
        {
            mockTestResult.Status = EnumResultStatus.Process;

            await _mockTestResultRepository.BulkUpdateList(new List<MockTestResult> { mockTestResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId };
            });
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, MockTestResult mockTestResult)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId && x.StudentId == mockTestResult.StudentId && x.CreatedDate >= mockTestResult.CreatedDate).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                _logger.LoggerRequest(request);
                sectionGroupResult = new SectionGroupResult
                {
                    StudentId = mockTestResult.StudentId,
                    SectionGroupId = request.SectionGroupId,
                    MockTestResultId = request.MockTestResultId,
                    Status = EnumResultStatus.New
                };

                try
                {
                    await _sectionGroupResultRepository.BulkMergeAsync(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.SectionGroupId, c.StudentId, c.PlacementTestResultId, c.FinalTestResultId, c.MockTestResultId, c.IsDeleted };
                    });
                    sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.Id == sectionGroupResult.Id) ?? sectionGroupResult;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate SectionGroupResult MockTest : {ex.Message}");
                }
            }
            else if (sectionGroupResult.Status != EnumResultStatus.Done)
            {
                sectionGroupResult.Status = EnumResultStatus.Process;
                await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.SectionGroupId, c.StudentId, c.PlacementTestResultId, c.FinalTestResultId, c.MockTestResultId, c.WorkingTime };
                });
            }
            return sectionGroupResult;
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardCategory category, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = category,
                Value = 1
            }, cancellationToken);
        }
    }
}
