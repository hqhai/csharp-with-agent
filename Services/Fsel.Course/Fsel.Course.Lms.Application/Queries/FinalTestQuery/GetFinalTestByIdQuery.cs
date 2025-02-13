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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
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
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetFinalTestByIdQueryHandler(IFinalTestRepository finalTestRepository, ISectionGroupRepository sectionGroupRepository, ICourseResultRepository courseResultRepository, SectionGroupConverter sectionGroupConverter, AuthContext authContext, IUserService userService, IMapper mapper)
        {
            _finalTestRepository = finalTestRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _courseResultRepository = courseResultRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<FinalTestModel>> Handle(GetFinalTestByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<FinalTestModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            var finalTest = await _finalTestRepository.Queryable.Include(x => x.FinalTestResults.Where(x => x.CourseId == courseResult.CourseId && x.StudentId == studentId))
                                                                .Include(x => x.FinalTestSections.Where(n => !n.IsDeleted))
                                                                .ThenInclude(x => x.SectionGroup)
                                                                .FirstOrDefaultAsync(x => x.Id == request.FinalTestId, cancellationToken);
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
            methodResult.Result = await GetFinalTestAsync(finalTest, finalTestResult, cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<List<SectionGroup>> GetSectionGroupsAsync(FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            return await _sectionGroupRepository.Queryable.Include(x => x.FinalTestSections.Where(n => !n.IsDeleted))
                                    .Include(x => x!.Sections.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x!.SectionQuestions.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x!.SectionGroupResults.Where(x => x.FinalTestResultId == finalTestResult.Id))
                                    .Where(x => x.FinalTestSections.Any(x => x.FinalTestId == finalTestResult.FinalTestId))
                                    .OrderBy(x => x.CourseSkill)
                                    .AsNoTracking()
                                    .ToListAsync(cancellationToken);
        }

        private async Task<FinalTestModel> GetFinalTestAsync(FinalTest finalTest, FinalTestResult finalTestResult, CancellationToken cancellationToken)
        {
            var finalTestDetail = _mapper.Map<FinalTestModel>(finalTest);
            var sectionGroups = await GetSectionGroupsAsync(finalTestResult, cancellationToken);
            finalTestDetail.TotalQuestion = _sectionGroupConverter.GetTotalQuestion(sectionGroups);
            finalTestDetail.FinalTestResult = _mapper.Map<FinalTestResultModel>(finalTestResult);
            finalTestDetail.FinalTestResult.ProgressPercent = NumberHelper.GetPercent(sectionGroups.SelectMany(x => x.SectionGroupResults).Count(x => x.Status == EnumResultStatus.Done), sectionGroups.Count);
            finalTestDetail.SectionGroups = await _sectionGroupConverter.GetSectionGroupsAsync(sectionGroups, finalTestDetail.FinalTestResult.Id, nameof(SectionGroupResult.FinalTestResultId));
            return finalTestDetail;
        }
    }
}
