// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<SectionGroupDetailModel>>
    {
        public Guid SectionGroupId { get; set; }
        public Guid MockTestResultId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<SectionGroupDetailModel>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GetSectionBySectionGroupIdQueryHandler(ISectionRepository sectionRepository, SectionConverter sectionConverter, ISectionGroupResultRepository sectionGroupResultRepository, IMockTestResultRepository mockTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper, ISectionGroupRepository sectionGroupRepository)
        {
            _sectionRepository = sectionRepository;
            _sectionConverter = sectionConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mockTestResultRepository = mockTestResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<SectionGroupDetailModel>> Handle(GetSectionBySectionGroupIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SectionGroupDetailModel>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id ?? default;

            var mockTestResult = await _mockTestResultRepository.GetByIdAsync(request.MockTestResultId);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(mockTestResult));
                return methodResult;
            }
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }

            var (sections, totalCount) = await GetSectionsAsync(request.SectionGroupId, sectionGroup.CourseSkill);

            var sectonGroupDetail = _mapper.Map<SectionGroupDetailModel>(sectionGroup);
            sectonGroupDetail.SectionGroupResult = _mapper.Map<SectionGroupResultModel>(await GetAndAddSectionGroupResult(request, studentId));
            sectonGroupDetail.Sections = GetSections(sections, sectionGroup.CourseSkill);
            sectonGroupDetail.TotalQuestion = totalCount;
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = sectonGroupDetail;
            return methodResult;
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, Guid studentId)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId && x.MockTestResultId == request.MockTestResultId && x.StudentId == studentId).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                sectionGroupResult = _sectionGroupResultRepository.Add(new SectionGroupResult { StudentId = studentId, SectionGroupId = request.SectionGroupId, MockTestResultId = request.MockTestResultId });
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return sectionGroupResult;
        }

        private IList<SectionDetailModel> GetSections(IList<Section> sections, EnumCourseSkill skill)
        {
            var listSection = new List<SectionDetailModel>();
            return sections.Select(x => GetSection(x, skill)).ToList();
        }

        private SectionDetailModel GetSection(Section section, EnumCourseSkill skill)
        {
            var sectionDetail = _mapper.Map<SectionDetailModel>(section);
            sectionDetail.Answer = section.MockTestAnswers.FirstOrDefault()?.Answer;
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
                sectionDetail.SectionParts = section.SectionParts.OrderBy(x => x.CreatedDate).Select(x => GetSectionPart(x)).ToList();
            }
            else if (skill == EnumCourseSkill.Speaking)
            {
                sectionDetail.SectionTimeCodes = section.SectionTimeCodes.Select(x => GetSectionTimeCode(x)).ToList();
            }
            return sectionDetail;
        }

        private SectionTimeCodeDetailModel GetSectionTimeCode(SectionTimeCode sectionTimeCode)
        {
            var sectionTimeCodeModel = _mapper.Map<SectionTimeCodeDetailModel>(sectionTimeCode);
            sectionTimeCodeModel.Answer = sectionTimeCode.MockTestAnswers.FirstOrDefault()?.Answer;
            return sectionTimeCodeModel;
        }

        private SectionPartDetailModel GetSectionPart(SectionPart sectionPart)
        {
            var sectionPartModel = _mapper.Map<SectionPartDetailModel>(sectionPart);
            sectionPartModel.QuestionIds = sectionPart.SectionQuestions.OrderBy(x => x.CreatedDate).Select(x => x.QuestionId ?? default).ToList();
            return sectionPartModel;
        }

        private async Task<(IList<Section>, long)> GetSectionsAsync(Guid sectionGroupId, EnumCourseSkill skill)
        {
            var sections = new List<Section>();
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
                sections = await _sectionRepository.Queryable.Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions)
                                                                    .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                    .ToListAsync();
            }
            else if (skill == EnumCourseSkill.Writing)
            {
                sections = await _sectionRepository.Queryable.Include(x => x.MockTestAnswers).Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                    .ToListAsync();
            }
            else
            {
                sections = await _sectionRepository.Queryable.Include(x => x.SectionTimeCodes).ThenInclude(x => x.MockTestAnswers)
                                                            .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                            .ToListAsync();
            }
            return (sections, _sectionConverter.GetTotalQuestion(sections, skill));
        }
    }
}
