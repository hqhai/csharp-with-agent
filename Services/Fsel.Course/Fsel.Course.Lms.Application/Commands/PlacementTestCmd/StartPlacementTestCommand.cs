// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartPlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid PlacementTestId { get; set; }
        public Guid SectionGroupId { get; set; }
    }

    public class StartPlacementTestCommandHandler : IRequestHandler<StartPlacementTestCommand, MethodResult<bool>>
    {
        private readonly AuthContext _authContext;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IPlacementTestRepository _placementTestRepository;

        public StartPlacementTestCommandHandler(AuthContext authContext
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IPlacementTestRepository placementTestRepository)
        {
            _authContext = authContext;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<bool>> Handle(StartPlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var placementTest = await _placementTestRepository.Queryable.Include(x => x.PlacementTestSections)
                                                    .FirstOrDefaultAsync(x => x.Id == request.PlacementTestId && x.PlacementTestSections.Select(x => x.SectionGroupId).Contains(request.SectionGroupId), cancellationToken);
            if (placementTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist), nameof(request.PlacementTestId), request.PlacementTestId);
                return methodResult;
            }

            if (!placementTest.IsActive)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestInActiveState), nameof(placementTest.IsActive), placementTest.IsActive);
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }

            var studentId = student?.Content?.Result?.Id;
            var placementTestId = placementTest.PlacementTestSections.FirstOrDefault()!.Id;
            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.PlacementTestId == placementTestId && x.StudentId == studentId, cancellationToken);
            if (placementTestResult == null)
            {
                _placementTestResultRepository.Add(new PlacementTestResult { StudentId = studentId ?? default, PlacementTestId = placementTestId });
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
