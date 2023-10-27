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
    using Microsoft.EntityFrameworkCore;

    public class GetFinalTestByIdQuery : IRequest<MethodResult<FinalTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid FinalTestId { get; set; }
    }

    public class GetFinalTestByIdQueryHandler : IRequestHandler<GetFinalTestByIdQuery, MethodResult<FinalTestModel>>
    {
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetFinalTestByIdQueryHandler(IFinalTestRepository finalTestRepository, SectionConverter sectionConverter, IFinalTestResultRepository finalTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _finalTestRepository = finalTestRepository;
            _sectionConverter = sectionConverter;
            _finalTestResultRepository = finalTestResultRepository;
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

            methodResult.Result = await GetFinalTestAsync(finalTest, studentId, request);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<FinalTestModel> GetFinalTestAsync(FinalTest finalTest, Guid studentId, GetFinalTestByIdQuery request)
        {
            var finalTestDetail = _mapper.Map<FinalTestModel>(finalTest);
            var finalTestResult = await GetMockTestResult(request, studentId);
            var sectionGroups = finalTest.FinalTestSections.OrderBy(x => x.CreatedDate).Select(x => x.SectionGroup ?? new SectionGroup()).ToList();
            finalTestDetail.TotalQuestion = _sectionConverter.GetTotalQuestion(sectionGroups);
            finalTestDetail.ExecutionTime = _sectionConverter.GetExecutionTime(sectionGroups);
            finalTestDetail.FinalTestResult = finalTestResult;
            finalTestDetail.CourseSkills = _sectionConverter.GetCourseSkill(sectionGroups);
            finalTestDetail.SectionGroups = GetSectionGroups(sectionGroups, finalTestDetail.Id);
            return finalTestDetail;
        }

        private async Task<FinalTestResultModel> GetMockTestResult(GetFinalTestByIdQuery request, Guid studentId)
        {
            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == request.CourseId && x.FinalTestId == request.FinalTestId && x.StudentId == studentId)
                .FirstOrDefaultAsync();
            if (finalTestResult == null)
            {
                finalTestResult = _finalTestResultRepository.Add(new FinalTestResult { StudentId = studentId, CourseId = request.CourseId, FinalTestId = request.FinalTestId });
                await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return _mapper.Map<FinalTestResultModel>(finalTestResult);
        }

        private IList<SectionGroupModel> GetSectionGroups(IList<SectionGroup>? sectionGroups, Guid mockTestResultId)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);

            var indexProcess = GetIndexProcess(sectionGroups, mockTestResultId);
            return sectionGroups.Select(x =>
            {
                var index = sectionGroups.IndexOf(x);
                var sectionGroup = _mapper.Map<SectionGroupModel>(x);
                sectionGroup.Status = GetResultStatus(indexProcess, index);
                sectionGroup.SectionGroupResult = _mapper.Map<SectionGroupResultModel>(x.SectionGroupResults.FirstOrDefault());
                return sectionGroup;
            }).ToList();
        }

        private static EnumResultStatus GetResultStatus(int? indexProcess, int index)
        {
            var resultStatus = EnumResultStatus.Unfinished;
            if (indexProcess < index)
            {
                return resultStatus;
            }
            else if (indexProcess == index)
            {
                resultStatus = EnumResultStatus.Process;
            }
            else if (indexProcess > index || indexProcess == null)
            {
                resultStatus = EnumResultStatus.Done;
            }
            return resultStatus;
        }

        private static int? GetIndexProcess(IList<SectionGroup>? sectionGroups, Guid mockTestResultId)
        {
            ArgumentNullException.ThrowIfNull(sectionGroups);
            var timeCode = sectionGroups.Where(x => !x.SectionGroupResults.Any() || x.SectionGroupResults.Any(x => x.MockTestResultId == mockTestResultId && x.Status != EnumResultStatus.Done)).FirstOrDefault();
            if (timeCode == null)
            {
                return null;
            }
            return sectionGroups.IndexOf(timeCode);
        }
    }
}
