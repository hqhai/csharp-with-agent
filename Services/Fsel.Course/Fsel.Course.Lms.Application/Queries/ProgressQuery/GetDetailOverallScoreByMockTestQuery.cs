// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDetailOverallScoreByMockTestQuery : IRequest<MethodResult<MockTestResultModel>>
    {
        public Guid MockTestResultId { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }

    public class GetDetailOverallScoreByMockTestQueryHandler : IRequestHandler<GetDetailOverallScoreByMockTestQuery, MethodResult<MockTestResultModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetDetailOverallScoreByMockTestQueryHandler(AuthContext authContext
            , IMockTestResultRepository mockTestResultRepository
            , IMockTestRepository mockTestRepository
            , IMapper mapper
            , IUserService userService)
        {
            _authContext = authContext;
            _mockTestResultRepository = mockTestResultRepository;
            _mockTestRepository = mockTestRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<MockTestResultModel>> Handle(GetDetailOverallScoreByMockTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<MockTestResultModel> methodResult = new MethodResult<MockTestResultModel>();

            if (!(request.CourseSkill == EnumCourseSkill.Writing || request.CourseSkill == EnumCourseSkill.Speaking))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.SkillSpeakingOrWriting), nameof(request.CourseSkill));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var mockTestResult = await _mockTestResultRepository.ReadQueryable.Include(x => x.MockTestScores)
                                                                .FirstOrDefaultAsync(x => x.Id == request.MockTestResultId && x.StudentId == studentId, cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }

            if (mockTestResult.Status != EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestResultMustDone), nameof(mockTestResult));
                return methodResult;
            }
            var mockTest = await _mockTestRepository.ReadQueryable.Include(x => x.MockTestSections)
                                                    .ThenInclude(x => x.SectionGroup)
                                                    .FirstOrDefaultAsync(x => x.Id == mockTestResult.MockTestId, cancellationToken);
            if (mockTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTest));
                return methodResult;
            }

            var sectionGroup = mockTest.MockTestSections.Select(x => x.SectionGroup).FirstOrDefault(x => x!.CourseSkill == request.CourseSkill);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }

            if (!(sectionGroup.CourseSkill == EnumCourseSkill.Writing || sectionGroup.CourseSkill == EnumCourseSkill.Speaking))
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestErrorCode.SkillSpeakingOrWriting), nameof(request.CourseSkill));
                return methodResult;
            }
            mockTestResult.MockTestScores = mockTestResult.MockTestScores.Where(x => x.SectionGroupId == sectionGroup.Id).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<MockTestResultModel>(mockTestResult);
            return methodResult;
        }
    }
}
