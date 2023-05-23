// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionConverter
    {
        private readonly IMapper _mapper;
        public SectionConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public SectionGroupModel? GetSectionGroupModel(SectionGroup? sectionGroup)
        {
            if (sectionGroup == null)
            {
                return null;
            }
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.Sections = GetSectionModels(sectionGroup.Sections.ToList());

            return sectionGroupModel;
        }

        public IList<SectionModel> GetSectionModels(IList<Section> sections)
        {
            return sections.Select(x => new SectionModel
            {
                Id = x.Id,
                Name = x.Name,
                MediaPost = x.MediaPost,
                TargetWord = x.TargetWord,
                SectionParts = GetSectionPartModels(x.SectionParts.ToList()),
                Questions = GetQuestionModels(x.SectionQuestions.ToList()),
            }).ToList();
        }

        public IList<SectionPartModel> GetSectionPartModels(IList<SectionPart> sectionParts)
        {
            return sectionParts.Select(x => new SectionPartModel
            {
                Id = x.Id,
                PartName = x.PartName,
                SectionId = x.SectionId,
                Questions = GetQuestionModels(x.SectionQuestions.ToList())
            }).ToList();
        }

        public IList<QuestionModel> GetQuestionModels(IList<SectionQuestion> sectionQuestions)
        {
            return sectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
            {
                Id = x!.Id,
                QuestionType = x.QuestionType,
                Explanation = x.Explanation,
                Ungraded = x.Ungraded,
                CorrectTotal = x.CorrectTotal,
                Config = x.Config
            }).ToList();
        }
    }
}
