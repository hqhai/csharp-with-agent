// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.FinalTestQuery
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
        public Guid FinalTestResultId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<SectionGroupDetailModel>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GetSectionBySectionGroupIdQueryHandler(ISectionRepository sectionRepository, SectionConverter sectionConverter, ISectionGroupResultRepository sectionGroupResultRepository, IFinalTestResultRepository finalTestResultRepository, AuthContext authContext, IUserService userService, IMapper mapper, ISectionGroupRepository sectionGroupRepository)
        {
            _sectionRepository = sectionRepository;
            _sectionConverter = sectionConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
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

            var finalTestResult = await _finalTestResultRepository.GetByIdAsync(request.FinalTestResultId);
            if (finalTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(finalTestResult));
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
            sectonGroupDetail.Sections = GetSections(sections);
            sectonGroupDetail.TotalQuestion = totalCount;
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = sectonGroupDetail;
            return methodResult;
        }

        private async Task<SectionGroupResult> GetAndAddSectionGroupResult(GetSectionBySectionGroupIdQuery request, Guid studentId)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Where(x => x.SectionGroupId == request.SectionGroupId && x.FinalTestResultId == request.FinalTestResultId && x.StudentId == studentId).FirstOrDefaultAsync();
            if (sectionGroupResult == null)
            {
                sectionGroupResult = _sectionGroupResultRepository.Add(new SectionGroupResult { StudentId = studentId, SectionGroupId = request.SectionGroupId, FinalTestResultId = request.FinalTestResultId });
                await _sectionGroupResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return sectionGroupResult;
        }

        private IList<SectionDetailModel> GetSections(IList<Section> sections)
        {
            var listSection = new List<SectionDetailModel>();
            return sections.Select(x => GetSection(x)).ToList();
        }

        private SectionDetailModel GetSection(Section section)
        {
            var sectionDetail = _mapper.Map<SectionDetailModel>(section);
            sectionDetail.QuestionIds = section.SectionQuestions.OrderBy(x => x.CreatedDate).Where(x => x.QuestionId.HasValue).Select(x => x.QuestionId!.Value).ToList();
            return sectionDetail;
        }

        private async Task<(IList<Section>, long)> GetSectionsAsync(Guid sectionGroupId, EnumCourseSkill skill)
        {
            var sections = await _sectionRepository.Queryable.Include(x => x.SectionQuestions)
                                                                    .Where(x => x.SectionGroupId == sectionGroupId).OrderBy(x => x.DisplayOrder)
                                                                    .ToListAsync();
            return (sections, _sectionConverter.GetTotalQuestion(sections, skill));
        }
    }
}
