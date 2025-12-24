// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TestServices.Extensions
{
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Lms.Application.Services.TestServices.Helpers;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;
    using Fsel.Shared.Enums;

    public static class TestSectionScoringExtensions
    {
        public static double CalculateScore(
            this TestSection section,
            EnumScoringFormulaType formulaType,
            double rawValue,
            IBandPercentConverter? converter = null,
            double? defaultWhenNoRule = 0d)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));

            // rules lấy từ JSON
            var rules = (section.ScoringFormulaConfigs ?? new List<ScoringFormulaConfig>()).ToList();

            return TestSectionScoringHelper.MapScore(
                formulaType: formulaType,
                rules: rules,
                rawValue: rawValue,
                converter: converter,
                defaultWhenNoRule: defaultWhenNoRule);
        }
    }
}
