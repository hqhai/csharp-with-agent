// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<SectionGroupDtoModel>>
    {
        public Guid SectionGroupId { get; set; }
        public Guid PlacementTestResultId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<SectionGroupDtoModel>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ILogger<GetSectionBySectionGroupIdQuery> _logger;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IPlacementTestSectionRepository _placementTestSectionRepository;

        public GetSectionBySectionGroupIdQueryHandler(IPlacementTestResultRepository placementTestResultRepository,
            SectionGroupConverter sectionGroupConverter,
            ISectionGroupResultRepository sectionGroupResultRepository,
            AuthContext authContext,
            IUserService userService,
            ISectionGroupRepository sectionGroupRepository,
            ILogger<GetSectionBySectionGroupIdQuery> logger,
            IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IPlacementTestSectionRepository placementTestSectionRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _authContext = authContext;
            _userService = userService;
            _sectionGroupRepository = sectionGroupRepository;
            _logger = logger;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _placementTestSectionRepository = placementTestSectionRepository;
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

            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.PlacementTestResultId);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }

            var isNotInModule = !await (from baseQ in _sectionGroupRepository.Queryable
                                        join psg in _placementTestSectionRepository.Queryable on baseQ.Id equals psg.SectionGroupId
                                        where baseQ.Id == request.SectionGroupId && psg.PlacementTestId == placementTestResult.PlacementTestId
                                        select baseQ).AnyAsync(cancellationToken);
            if (isNotInModule)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isNotInModule), request.SectionGroupId);
                return methodResult;
            }

            if (placementTestResult.Status == EnumResultStatus.New)
            {
                await UpdatePlacementTestResult(placementTestResult);
                await SavePlacementGroupResultAsync(placementTestResult.StudentId);
            }

            var sectionGroupResult = await GetAndAddSectionGroupResult(request, placementTestResult.StudentId);
            methodResult.Result = await _sectionGroupConverter.GetSectionGroupDto(sectionGroup, sectionGroupResult);

            if (sectionGroupResult.Status == EnumResultStatus.Done)
            {
                _logger.LogInformation($"Logger PT Done : {methodResult.Result.Serialize()}");
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdatePlacementTestResult(PlacementTestResult placementTestResult)
        {
            placementTestResult.Status = EnumResultStatus.Process;
            _placementTestResultRepository.Update(placementTestResult);
            await _placementTestResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<PlacementTestGroupResult> SavePlacementGroupResultAsync(Guid studentId)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (placementTestGroupResult == null || placementTestGroupResult.Status != EnumResultStatus.New)
            {
                return placementTestGroupResult ?? new PlacementTestGroupResult();
            }
            placementTestGroupResult.Status = EnumResultStatus.Process;
            placementTestGroupResult.ProcessDate = DateTime.UtcNow;
            _placementTestGroupResultRepository.Update(placementTestGroupResult);
            await _placementTestGroupResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            return placementTestGroupResult;
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, Guid studentId)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == request.PlacementTestResultId && x.StudentId == studentId).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                _logger.LoggerRequest(request);
                sectionGroupResult = _sectionGroupResultRepository.Add(new SectionGroupResult { StudentId = studentId, SectionGroupId = request.SectionGroupId, PlacementTestResultId = request.PlacementTestResultId, Status = EnumResultStatus.New });

                try
                {
                    await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate SectionGroupResult PlacementTest : {ex.Message}");
                }
            }
            else if (sectionGroupResult.Status != EnumResultStatus.Done)
            {
                sectionGroupResult.Status = EnumResultStatus.Process;
                sectionGroupResult = _sectionGroupResultRepository.Update(sectionGroupResult, false, x => x.WorkingTime);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
            return sectionGroupResult;
        }
    }
}
