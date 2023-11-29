// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetFinalTestByIdQuery : IRequest<MethodResult<FinalTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
    }

    public class GetFinalTestByIdQueryHandler : IRequestHandler<GetFinalTestByIdQuery, MethodResult<FinalTestModel>>
    {
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetFinalTestByIdQueryHandler(IFinalTestRepository finalTestRepository, SectionConverter sectionConverter, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _finalTestRepository = finalTestRepository;
            _sectionConverter = sectionConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(GetFinalTestByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<FinalTestModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var finalTest = await _finalTestRepository.GetAsync(request.FinalTestId, studentId);
            if (finalTest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTest));
                return methodResult;
            }
            var finalTestResult = finalTest.FinalTestResults.FirstOrDefault();
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
            methodResult.Result = GetFinalTest(finalTest, finalTestResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private FinalTestModel GetFinalTest(FinalTest finalTest, FinalTestResult finalTestResult)
        {
            var finalTestDetail = _mapper.Map<FinalTestModel>(finalTest);
            var sectionGroups = finalTest.FinalTestSections.OrderBy(x => x.CreatedDate).Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            finalTestDetail.TotalQuestion = _sectionConverter.GetTotalQuestion(sectionGroups);
            finalTestDetail.FinalTestResult = _mapper.Map<FinalTestResultModel>(finalTestResult);
            finalTestDetail.CourseSkills = _sectionConverter.GetCourseSkill(sectionGroups);
            finalTestDetail.SectionGroups = _sectionConverter.GetSectionGroups(sectionGroups, finalTestDetail.FinalTestResult.Id, "FinalTestResultId");
            return finalTestDetail;
        }
    }
}
