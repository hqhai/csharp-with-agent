// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReportFeedbackMockTestCommand : ReportFeedbackMockTestCommandModel, IRequest<MethodResult<MockTestResultModel>>
    {
    }

    public class ReportFeedbackMockTestCommandHandler : IRequestHandler<ReportFeedbackMockTestCommand, MethodResult<MockTestResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public ReportFeedbackMockTestCommandHandler(IMapper mapper, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService)
        {
            _mapper = mapper;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(ReportFeedbackMockTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).Where(e => e.Id == request.MockTestResultId && e.StudentId == studentId).FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            if (mockTestResult.FeedBackStars > 0 && mockTestResult.FeedBackNote != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(mockTestResult.FeedBackStars));
                return methodResult;
            }
            mockTestResult.FeedBackNote = request.FeedBackNote;
            mockTestResult.FeedBackStars = request.FeedBackStars;
            await _mockTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<MockTestResultModel>(mockTestResult);
                return methodResult;
            });

            return methodResult;
        }
    }
}
