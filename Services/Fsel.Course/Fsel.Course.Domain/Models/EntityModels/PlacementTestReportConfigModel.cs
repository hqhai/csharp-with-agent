// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;

    public class PlacementTestConfigAgeLevelModel
    {
        public EnumCourseLevel CurrentLevel { get; set; }
        public int AgeStart { get; set; }
        public int? AgeEnd { get; set; }
    }

    public class PlacementTestReportConfigModel
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public EnumTestResultScenario ResultScenario { get; set; }
        public IList<PlacementTestReportConfigTranslationModel> Translations { get; set; } = new List<PlacementTestReportConfigTranslationModel>();
    }

    public class PlacementTestReportConfigTranslationModel : ITranslationObject
    {
        public Guid Id { get; set; }
        public string? Language { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
    }

    public class PlacementTestReportViewConfigModel
    {
        public Guid Id { get; set; }
        public EnumCourseLevel SuggestedLevel { get; set; }
        public int AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public string? Text { get; set; }
        public IList<PlacementTestReportViewConfigTranslationModel> Translations { get; set; } = new List<PlacementTestReportViewConfigTranslationModel>();
    }

    public class PlacementTestReportViewConfigTranslationModel : ITranslationObject
    {
        public Guid Id { get; set; }
        public string? Language { get; set; }
        public string? Text { get; set; }
    }
}
