// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
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
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<SectionGroupDtoModel>>
    {
        public Guid SectionGroupId { get; set; }
        public Guid FinalTestResultId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<SectionGroupDtoModel>>
    {
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ILogger<GetSectionBySectionGroupIdQuery> _logger;
        private readonly IFinalTestSectionRepository _finalTestSectionRepository;

        public GetSectionBySectionGroupIdQueryHandler(SectionGroupConverter sectionGroupConverter,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            AuthContext authContext,
            IUserService userService,
            ISectionGroupRepository sectionGroupRepository,
            ILogger<GetSectionBySectionGroupIdQuery> logger,
            IFinalTestSectionRepository finalTestSectionRepository)
        {
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _sectionGroupRepository = sectionGroupRepository;
            _logger = logger;
            _finalTestSectionRepository = finalTestSectionRepository;
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
            //var studentId = student?.Id ?? default;

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
                return methodResult;
            }
            else if (finalTestResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(finalTestResult));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var isNotInModule = !await (from baseQ in _sectionGroupRepository.Queryable
                                        join fsg in _finalTestSectionRepository.Queryable on baseQ.Id equals fsg.SectionGroupId
                                        where baseQ.Id == request.SectionGroupId && fsg.FinalTestId == finalTestResult.FinalTestId
                                        select baseQ).AnyAsync(cancellationToken);
            if (isNotInModule)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isNotInModule), request.SectionGroupId);
                return methodResult;
            }
            if (finalTestResult.Status == EnumResultStatus.New)
            {
                await UpdateFinalTestResult(finalTestResult);
            }

            var sectionGroupResult = await GetAndAddSectionGroupResult(request, finalTestResult.StudentId);
            methodResult.Result = await _sectionGroupConverter.GetSectionGroupDto(sectionGroup, sectionGroupResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateFinalTestResult(FinalTestResult finalTestResult)
        {
            finalTestResult.ProcessDate = DateTime.UtcNow;
            finalTestResult.Status = EnumResultStatus.Process;
            await _finalTestResultRepository.BulkUpdateList(new List<FinalTestResult> { finalTestResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.FinalTestId };
            });
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, Guid studentId)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.SectionGroupId == request.SectionGroupId && x.FinalTestResultId == request.FinalTestResultId && x.StudentId == studentId).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                _logger.LoggerRequest(request);
                sectionGroupResult = new SectionGroupResult
                {
                    StudentId = studentId,
                    SectionGroupId = request.SectionGroupId,
                    FinalTestResultId = request.FinalTestResultId,
                    Status = EnumResultStatus.New
                };

                try
                {
                    await _sectionGroupResultRepository.BulkMergeAsync(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = c => new { c.SectionGroupId, c.StudentId, c.FinalTestResultId, c.IsDeleted };
                    });

                    sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.Id == sectionGroupResult.Id) ?? sectionGroupResult;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate SectionGroupResult FinalTest : {ex.Message}");
                }
            }
            else if (sectionGroupResult.Status != EnumResultStatus.Done)
            {
                sectionGroupResult.Status = EnumResultStatus.Process;
                await _sectionGroupResultRepository.BulkUpdateList(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.SectionGroupId, c.StudentId, c.FinalTestResultId, c.WorkingTime, c.MockTestResultId, c.PlacementTestResultId };
                });
            }
            return sectionGroupResult;
        }
    }
}
