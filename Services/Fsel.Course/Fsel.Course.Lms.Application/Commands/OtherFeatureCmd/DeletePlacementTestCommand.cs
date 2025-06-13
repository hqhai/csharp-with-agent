// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeletePlacementTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class DeletePlacementTestCommandHandler : IRequestHandler<DeletePlacementTestCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;
        private readonly IUserService _userService;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public DeletePlacementTestCommandHandler(IPlacementTestGroupResultRepository placementTestGroupResultRepository,
            IUserService userService,
            IPlacementTestResultRepository placementTestResultRepository,
            ISectionGroupResultRepository sectionGroupResultRepository,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            ICourseResultRepository courseResultRepository)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
            _userService = userService;
            _placementTestResultRepository = placementTestResultRepository;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeletePlacementTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var studentResult = await _userService.GetUserByStudentId(request.StudentId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), request.StudentId);
                return methodResult;
            }
            var courseResults = await _courseResultRepository.Queryable.Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active).ToListAsync(cancellationToken);
            if (courseResults.Any())
            {
                await _courseResultRepository.BulkUpdateList(courseResults.Select(courseResult =>
                {
                    courseResult.WorkingStatus = EnumWorkingStatus.NotWorking;
                    return courseResult;
                }), bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.WorkingStatus };
                });
            }

            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId, cancellationToken);
            var placementTestResults = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(x => x.StudentId == request.StudentId).ToListAsync(cancellationToken);
            var placementTestAnswers = await _placementTestAnswerRepository.Queryable.WhereBulkContains(placementTestResults.Select(x => x.Id), x => x.PlacementTestResultId)
                                                                                     .ToListAsync(cancellationToken);
            if (placementTestAnswers.Any())
            {
                await _placementTestAnswerRepository.DeleteListAsync(placementTestAnswers);
                await _placementTestAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (sectionGroupResults.Any())
            {
                await _sectionGroupResultRepository.DeleteListAsync(sectionGroupResults);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (placementTestResults.Any())
            {
                await _placementTestResultRepository.DeleteListAsync(placementTestResults);
                await _placementTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (placementTestGroupResult != null)
            {
                await _placementTestGroupResultRepository.DeleteAsync(placementTestGroupResult);
                await _placementTestGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = true;
            return methodResult;
        }
    }
}
