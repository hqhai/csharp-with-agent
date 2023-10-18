// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.MockTestQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetSectionBySectionGroupIdQuery : IRequest<MethodResult<IList<SectionDetailModel>>>
    {
        public Guid SectionGroupId { get; set; }
    }

    public class GetSectionBySectionGroupIdQueryHandler : IRequestHandler<GetSectionBySectionGroupIdQuery, MethodResult<IList<SectionDetailModel>>>
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IMapper _mapper;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GetSectionBySectionGroupIdQueryHandler(ISectionRepository sectionRepository, IMapper mapper, ISectionGroupRepository sectionGroupRepository)
        {
            _sectionRepository = sectionRepository;
            _mapper = mapper;
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<IList<SectionDetailModel>>> Handle(GetSectionBySectionGroupIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SectionDetailModel>>();
            var sectionGroup = await _sectionGroupRepository.GetByIdAsync(request.SectionGroupId);
            if (sectionGroup == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup));
                return methodResult;
            }

            var sections = await GetSectionsAsync(request.SectionGroupId, sectionGroup.CourseSkill);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = GetSections(sections, sectionGroup.CourseSkill);
            return methodResult;
        }

        private IList<SectionDetailModel> GetSections(IList<Section> sections, EnumCourseSkill skill)
        {
            var listSection = new List<SectionDetailModel>();
            return sections.Select(x => GetSection(x, skill)).ToList();
        }

        private SectionDetailModel GetSection(Section section, EnumCourseSkill skill)
        {
            var sectionDetail = _mapper.Map<SectionDetailModel>(section);
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
            }
            else if (skill == EnumCourseSkill.Writing)
            {
            }
            else
            {
            }
            return sectionDetail;
        }

        private async Task<IList<Section>> GetSectionsAsync(Guid sectionGroupId, EnumCourseSkill skill)
        {
            var sections = new List<Section>();
            if (skill == EnumCourseSkill.Reading || skill == EnumCourseSkill.Listening)
            {
                sections = await _sectionRepository.Queryable.Include(x => x.SectionParts).ThenInclude(x => x.SectionQuestions)
                                                                    .Where(x => x.SectionGroupId == sectionGroupId)
                                                                    .ToListAsync();
            }
            else if (skill == EnumCourseSkill.Writing)
            {
                sections = await _sectionRepository.Queryable.Where(x => x.SectionGroupId == sectionGroupId)
                                                                    .ToListAsync();
            }
            else
            {
                sections = await _sectionRepository.Queryable.Include(x => x.SectionTimeCodes)
                                                            .Where(x => x.SectionGroupId == sectionGroupId)
                                                            .ToListAsync();
            }
            return sections;
        }
    }
}
