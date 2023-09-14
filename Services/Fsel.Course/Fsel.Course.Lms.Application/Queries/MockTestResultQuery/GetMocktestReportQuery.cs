// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.MockTestResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestReportQuery : IRequest<MethodResult<MockTestResultModel>>
    {
        public Guid MockTestResultId { get; set; }
    }

    public class GetMockTestReportQueryHandler : IRequestHandler<GetMockTestReportQuery, MethodResult<MockTestResultModel>>
    {
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetMockTestReportQueryHandler(IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(GetMockTestReportQuery request, CancellationToken cancellationToken)
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

            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTestScores).OrderBy(x => x.CreatedDate)
                                    .Where(x => x.Id == request.MockTestResultId && x.StudentId == studentId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken);

            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            mockTestResult.CheckStartDate = DateTime.Now;
            var mockTestResultModel = new MockTestResultModel
            {
                Id = mockTestResult.Id,
                Percent = mockTestResult.Percent,
                CorrectCount = mockTestResult.CorrectCount,
                CorrectTotal = mockTestResult.CorrectTotal,
                CourseId = mockTestResult.CourseId,
                CreatedDate = mockTestResult.CreatedDate,
                SkillScores = mockTestResult.SkillScores,
                Status = mockTestResult.Status,
                StudentId = mockTestResult.StudentId,
                MockTestId = mockTestResult.MockTestId,
                MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(mockTestResult.MockTestScores?.OrderBy(x => x.CreatedDate))
            };

            if (mockTestResultModel.SkillScores != null)
            {
                mockTestResultModel.Scores = mockTestResult.SkillScores!.Average(x => x.Scores);
            }

            mockTestResult = _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = mockTestResultModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
