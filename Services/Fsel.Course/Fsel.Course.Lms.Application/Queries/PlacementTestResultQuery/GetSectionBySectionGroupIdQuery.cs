// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
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
        private readonly ISectionRepository _sectionRepository;
        private readonly ILogger<object> _logger;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly GetTimeToCompleteTestPublisher _getTimeToCompleteTestPublisher;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GetSectionBySectionGroupIdQueryHandler(ISectionRepository sectionRepository, ILogger<object> logger, DateTimeConverter dateTimeConverter, IPlacementTestResultRepository placementTestResultRepository, GetTimeToCompleteTestPublisher getTimeToCompleteTestPublisher, SectionGroupConverter sectionGroupConverter, ISectionGroupResultRepository sectionGroupResultRepository, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper, ISectionGroupRepository sectionGroupRepository)
        {
            _sectionRepository = sectionRepository;
            _logger = logger;
            _dateTimeConverter = dateTimeConverter;
            _placementTestResultRepository = placementTestResultRepository;
            _getTimeToCompleteTestPublisher = getTimeToCompleteTestPublisher;
            _sectionGroupConverter = sectionGroupConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<SectionGroupDtoModel>> Handle(GetSectionBySectionGroupIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SectionGroupDtoModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var placementTestResult = await _placementTestResultRepository.GetByIdAsync(request.PlacementTestResultId);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestResult));
                return methodResult;
            }
            if (placementTestResult.Status == EnumResultStatus.New)
            {
                await UpdatePlacementTestResult(placementTestResult);
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }
            var sectionGroupResult = await GetAndAddSectionGroupResult(request, studentId, sectionGroup);
            methodResult.Result = await _sectionGroupConverter.GetSectionGroupDto(sectionGroup, sectionGroupResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdatePlacementTestResult(PlacementTestResult placementTestResult)
        {
            placementTestResult.Status = EnumResultStatus.Process;
            _placementTestResultRepository.Update(placementTestResult);
            await _placementTestResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, Guid studentId, SectionGroup sectionGroup)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId && x.PlacementTestResultId == request.PlacementTestResultId && x.StudentId == studentId).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                var requestInfo = new
                {
                    Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                    Request = ConvertHelper.Serialize(request)
                };
                _logger.LogError(ConvertHelper.Serialize(requestInfo));
                sectionGroupResult = _sectionGroupResultRepository.Add(new SectionGroupResult { StudentId = studentId, SectionGroupId = request.SectionGroupId, PlacementTestResultId = request.PlacementTestResultId, Status = EnumResultStatus.New });
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                await _getTimeToCompleteTestPublisher.Publish(new SetTimeToCompleteTestModel
                {
                    ExecutionTime = sectionGroup.ExecutionTime,
                    ObjectResultId = sectionGroupResult.Id,
                    ObjectResultType = nameof(PlacementTest)
                }, CancellationToken.None).ConfigureAwait(false);
            }
            else if (sectionGroupResult.Status != EnumResultStatus.Done)
            {
                sectionGroupResult.Status = EnumResultStatus.Process;
                sectionGroupResult.WorkingTime = _dateTimeConverter.GetWorkingTime(sectionGroup.ExecutionTime, sectionGroupResult.CreatedDate);
                sectionGroupResult = _sectionGroupResultRepository.Update(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return sectionGroupResult;
        }
    }
}
